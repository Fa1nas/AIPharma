using AIPharma.Data;
using AIPharma.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AIPharma.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILlmService _llmService;
        private readonly ChatDatabaseAnswerService _chatDatabaseAnswerService;

        public ChatService(
    ApplicationDbContext db,
    ILlmService llmService,
    ChatDatabaseAnswerService chatDatabaseAnswerService)
        {
            _db = db;
            _llmService = llmService;
            _chatDatabaseAnswerService = chatDatabaseAnswerService;
        }

        public async Task<ChatAnswerResult> ProcessMessageAsync(int userId, string question, int? sessionId)
        {
            question = question?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(question))
            {
                return new ChatAnswerResult
                {
                    Answer = "Введіть питання.",
                    Source = "SYSTEM",
                    SessionId = sessionId ?? 0
                };
            }

            var session = await GetOrCreateSession(userId, sessionId, question);

            var userMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Sender = "User",
                Text = question,
                Source = "USER",
                CreatedAt = DateTime.Now
            };

            _db.ChatMessages.Add(userMessage);
            await _db.SaveChangesAsync();

            var normalizedQuestion = NormalizeQuestion(question);

            string answer;
            string source;

            var databaseAnswer = await _chatDatabaseAnswerService.TryAnswerAsync(question);

            if (!string.IsNullOrWhiteSpace(databaseAnswer))
            {
                answer = databaseAnswer;
                source = "DB";
            }
            else
            {
                var faq = await FindFaqAnswer(normalizedQuestion);

                if (faq != null)
                {
                    faq.UseCount++;
                    answer = faq.Answer;
                    source = "FAQ";
                }
                else
                {
                    var cached = await _db.AiCachedAnswers
                        .FirstOrDefaultAsync(c => c.NormalizedQuestion == normalizedQuestion);

                    if (cached != null)
                    {
                        cached.UseCount++;
                        cached.LastUsedAt = DateTime.Now;

                        answer = cached.Answer;
                        source = "CACHE";
                    }
                    else
                    {
                        var systemPrompt = await _db.SystemPrompts
                            .Where(p => p.IsActive)
                            .OrderByDescending(p => p.Id)
                            .Select(p => p.PromptText)
                            .FirstOrDefaultAsync();

                        systemPrompt ??= "Ти помічник сайту AIPharma.";

                        var contextMessages = await _db.ChatMessages
                            .Where(m => m.ChatSessionId == session.Id)
                            .OrderByDescending(m => m.CreatedAt)
                            .Take(6)
                            .OrderBy(m => m.CreatedAt)
                            .Select(m => $"{m.Sender}: {m.Text}")
                            .ToListAsync();

                        answer = await _llmService.GenerateAnswerAsync(systemPrompt, contextMessages, question);
                        source = _llmService.GetType().Name == "MockLlmService" ? "MOCK" : "LLM";

                        _db.AiCachedAnswers.Add(new AiCachedAnswer
                        {
                            NormalizedQuestion = normalizedQuestion,
                            Answer = answer,
                            UseCount = 1,
                            CreatedAt = DateTime.Now,
                            LastUsedAt = DateTime.Now
                        });
                    }
                }
            }

            var assistantMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Sender = "Assistant",
                Text = answer,
                Source = source,
                CreatedAt = DateTime.Now
            };

            _db.ChatMessages.Add(assistantMessage);

            _db.ApiRequestLogs.Add(new ApiRequestLog
            {
                UserId = userId,
                RequestText = question,
                ResponseText = answer,
                Source = source,
                PromptTokens = question.Length / 4,
                CompletionTokens = answer.Length / 4,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            return new ChatAnswerResult
            {
                Answer = answer,
                Source = source,
                SessionId = session.Id
            };
        }

        private async Task<ChatSession> GetOrCreateSession(int userId, int? sessionId, string firstQuestion)
        {
            if (sessionId.HasValue)
            {
                var existingSession = await _db.ChatSessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId.Value && s.UserId == userId);

                if (existingSession != null)
                {
                    return existingSession;
                }
            }

            var title = firstQuestion.Length > 60
                ? firstQuestion.Substring(0, 60) + "..."
                : firstQuestion;

            var session = new ChatSession
            {
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.Now
            };

            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();

            return session;
        }

        private async Task<FaqAnswer?> FindFaqAnswer(string normalizedQuestion)
        {
            var faqList = await _db.FaqAnswers.ToListAsync();

            foreach (var faq in faqList)
            {
                var normalizedFaqQuestion = NormalizeQuestion(faq.Question);

                if (normalizedQuestion.Contains(normalizedFaqQuestion) ||
                    normalizedFaqQuestion.Contains(normalizedQuestion))
                {
                    return faq;
                }

                var questionWords = normalizedQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var faqWords = normalizedFaqQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var matches = questionWords.Count(w => faqWords.Contains(w));

                if (questionWords.Length > 0 && matches >= Math.Min(2, questionWords.Length))
                {
                    return faq;
                }
            }

            return null;
        }

        private string NormalizeQuestion(string text)
        {
            text = text.ToLower().Trim();

            text = text.Replace("?", "")
                       .Replace("!", "")
                       .Replace(".", "")
                       .Replace(",", "")
                       .Replace("  ", " ");

            text = Regex.Replace(text, @"\s+", " ");

            return text;
        }
    }
}
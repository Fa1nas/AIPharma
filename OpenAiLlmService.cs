using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIPharma.Services
{
    public class OpenAiLlmService : ILlmService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OpenAiLlmService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateAnswerAsync(string systemPrompt, List<string> contextMessages, string userQuestion)
        {
            var apiKey = _configuration["LLM:ApiKey"];
            var model = _configuration["LLM:Model"] ?? "gpt-5-nano";
            var maxOutputTokens = _configuration.GetValue<int>("LLM:MaxOutputTokens", 250);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "API-ключ для LLM не вказаний. Система працює, але реальне звернення до зовнішнього API не виконано.";
            }

            var contextText = contextMessages.Any()
                ? string.Join("\n", contextMessages)
                : "Попередній контекст відсутній.";

            var input = new object[]
            {
                new
                {
                    role = "developer",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content =
                        "Контекст поточного діалогу:\n" +
                        contextText +
                        "\n\nНове питання користувача:\n" +
                        userQuestion
                }
            };

            var requestBody = new
            {
                model = model,
                input = input,
                max_output_tokens = maxOutputTokens,
                text = new
                {
                    verbosity = "low"
                },
                reasoning = new
                {
                    effort = "minimal"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return "Помилка під час звернення до LLM API: " + response.StatusCode + ". " + responseJson;
            }

            var answer = ExtractAnswer(responseJson);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return "LLM API повернув відповідь, але текст відповіді не вдалося прочитати.";
            }

            return answer;
        }

        private string ExtractAnswer(string responseJson)
        {
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;

            if (root.TryGetProperty("output_text", out var outputTextElement))
            {
                var outputText = outputTextElement.GetString();

                if (!string.IsNullOrWhiteSpace(outputText))
                {
                    return outputText;
                }
            }

            var collectedTexts = new List<string>();

            CollectTextValues(root, collectedTexts);

            return string.Join("\n", collectedTexts.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        }

        private void CollectTextValues(JsonElement element, List<string> texts)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                string? type = null;

                if (element.TryGetProperty("type", out var typeElement))
                {
                    type = typeElement.GetString();
                }

                if (type == "output_text" && element.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        texts.Add(text);
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    CollectTextValues(property.Value, texts);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    CollectTextValues(item, texts);
                }
            }
        }
    }
}
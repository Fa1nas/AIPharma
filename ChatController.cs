using AIPharma.Data;
using AIPharma.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPharma.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ChatService _chatService;

        public ChatController(ApplicationDbContext db, ChatService chatService)
        {
            _db = db;
            _chatService = chatService;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Index(int? sessionId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToLogin();
            }

            ViewBag.SessionId = sessionId;

            var sessions = await _db.ChatSessions
                .Where(s => s.UserId == CurrentUserId.Value)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(sessions);
        }

        public async Task<IActionResult> GetMessages(int sessionId)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == CurrentUserId.Value);

            if (session == null)
            {
                return NotFound();
            }

            var messages = await _db.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    sender = m.Sender,
                    text = m.Text,
                    source = m.Source,
                    createdAt = m.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ChatSendRequest request)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.ProcessMessageAsync(
                CurrentUserId.Value,
                request.Question,
                request.SessionId
            );

            return Json(new
            {
                answer = result.Answer,
                source = result.Source,
                sessionId = result.SessionId
            });
        }
    }

    public class ChatSendRequest
    {
        public string Question { get; set; } = string.Empty;
        public int? SessionId { get; set; }
    }
}
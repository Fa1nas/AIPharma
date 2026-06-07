namespace AIPharma.Services
{
    public class ChatAnswerResult
    {
        public string Answer { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int SessionId { get; set; }
    }
}
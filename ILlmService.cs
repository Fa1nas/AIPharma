namespace AIPharma.Services
{
    public interface ILlmService
    {
        Task<string> GenerateAnswerAsync(string systemPrompt, List<string> contextMessages, string userQuestion);
    }
}
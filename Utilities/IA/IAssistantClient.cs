namespace Utilities.IA
{
    public interface IAssistantClient
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}

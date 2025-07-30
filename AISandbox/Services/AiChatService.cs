using AISandbox.Models;

namespace AISandbox.Services;

public class AiChatService : IAiChatService
{
    public virtual Task<string> SendMessageAsync(List<ChatMessage> messages)
    {
        throw new NotImplementedException();
    }

    public virtual IAsyncEnumerable<string> StreamMessageAsync(List<ChatMessage> messages)
    {
        throw new NotImplementedException();
    }

    public virtual Task InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }

    public string Provider { get; set; } = string.Empty;
    public bool IsInitialized { get; set; }
}
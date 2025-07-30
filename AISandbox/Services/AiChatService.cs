using AISandbox.Models;

namespace AISandbox.Services;

public class AiChatService : IAiChatService
{
    protected List<ChatMessage> Messages { get; set; }
    
    public bool IsInitialized { get; private set; }

    public virtual Task<string> SendMessageAsync(string message)
    {
        throw new NotImplementedException();
    }

    public virtual IAsyncEnumerable<string> StreamMessageAsync(string message)
    {
        throw new NotImplementedException();
    }
    
    public void SetSystemMessage(string systemMessage)
    {
        if (Messages == null || Messages.Count == 0)
        {
            Messages = new List<ChatMessage>();
            Messages.Add(new ChatMessage
            {
                Message = systemMessage,
                Role = MessageType.System
            });
        }

        else if (Messages.Any(x => x.Role == MessageType.System))
        {
            var systemMessageIndex = Messages.FindIndex(x => x.Role == MessageType.System);
            Messages[systemMessageIndex].Message = systemMessage;
        }
        else
        {
            Messages.Add(new ChatMessage
            {
                Message = systemMessage,
                Role = MessageType.System
            });
        }
    }

    public virtual Task InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }
    
    protected void AddMessage(string message, MessageType role)
    {
        Messages.Add(new ChatMessage
        {
            Message = message,
            Role = role
        });
    }
}
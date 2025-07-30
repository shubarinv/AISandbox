using AISandbox.Models;

namespace AISandbox.Services;

public interface IAiChatService
{
    string Provider { get; protected set; }

    bool IsInitialized { get; protected set; }

    /// <summary>
    /// Sends a message to the AI chat service and returns the response.
    /// </summary>
    Task<string> SendMessageAsync(List<ChatMessage> messages);

    /// <summary>
    /// Streams messages from the AI chat service asynchronously.
    /// </summary>
    /// <param name="messages">The message to send to the AI chat service.</param>
    /// <returns>An asynchronous stream of messages from the AI chat service.</returns>
    IAsyncEnumerable<string> StreamMessageAsync(List<ChatMessage> messages);

    Task InitializeAsync();
}
namespace AISandbox.Services;

public interface IAiChatService
{
    /// <summary>
    /// Sends a message to the AI chat service and returns the response.
    /// </summary>
    Task<string> SendMessageAsync(string message);
    
    /// <summary>
    /// Streams messages from the AI chat service asynchronously.
    /// </summary>
    /// <param name="message">The message to send to the AI chat service.</param>
    /// <returns>An asynchronous stream of messages from the AI chat service.</returns>
    IAsyncEnumerable<string> StreamMessageAsync(string message);
    
    
    void SetSystemMessage(string systemMessage);

    Task InitializeAsync();
}
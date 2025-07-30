using AISandbox.Extensions;
using AISandbox.Models;
using OpenAI.Chat;

namespace AISandbox.Services.Implementations;

public class OpenAiChatService : AiChatService
{
    private ChatClient? _client;

    public override Task InitializeAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
        }

        _client = new ChatClient(model: "gpt-4.1-nano", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        return base.InitializeAsync();
    }

    public override async Task<string> SendMessageAsync(string message)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var messages = Messages.Select(x => x.ToOpenAIChatMessage()).ToList();
        messages.Add(new UserChatMessage(message));
        AddMessage(message, MessageType.User);


        var completion = await _client.CompleteChatAsync(messages);
        var response = completion.Value.Content[0].Text;
        AddMessage(response, MessageType.Assistant);

        return response;
    }
}
using AISandbox.Extensions;
using OpenAI.Chat;
using ChatMessage = AISandbox.Models.ChatMessage;

namespace AISandbox.Services.Implementations;

public class OpenAiChatService : AiChatService
{
    private ChatClient? _client;

    public OpenAiChatService()
    {
        Provider = "OpenAI";
    }

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

    public override async Task<string> SendMessageAsync(List<ChatMessage> messages)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var openAiMessages = messages.Select(x => x.ToOpenAIChatMessage()).ToList();


        var completion = await _client.CompleteChatAsync(openAiMessages);
        var response = completion.Value.Content[0].Text;
        return response;
    }


    public override async IAsyncEnumerable<string> StreamMessageAsync(List<ChatMessage> messages)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var openAiMessages = messages.Select(x => x.ToOpenAIChatMessage()).ToList();

        await foreach (var chunk in _client.CompleteChatStreamingAsync(openAiMessages))
        {
            if (chunk?.ContentUpdate is { Count: > 0 }) // turns out that OpenAI's API can return null or empty content updates
            {
                var text = chunk.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }
}
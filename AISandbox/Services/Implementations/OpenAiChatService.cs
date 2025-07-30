using System.Diagnostics;
using System.Text;
using AISandbox.Extensions;
using AISandbox.Models;
using OpenAI.Chat;
using ChatMessage = AISandbox.Models.ChatMessage;

namespace AISandbox.Services.Implementations;

public class OpenAiChatService : AiChatService
{
    private readonly double _costCachedToken = 0.025 / 1000000; // $0.025 per million cached tokens
    private readonly double _costFullToken = 0.10 / 1000000; // $0.10 per million tokens
    private readonly double _costOutputToken = 0.40 / 1000000; // $0.10 per million output tokens
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
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");

        var openAiMessages = messages.Select(x => x.ToOpenAIChatMessage()).ToList();

        var stopwatch = new Stopwatch();
        var logEntry = new LogEntry
        {
            Provider = Provider,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Prompt = messages.LastOrDefault()?.Message ?? string.Empty
        };

        StringBuilder responseBuilder = new();

        ChatTokenUsage? lastUsage = null;

        bool firstChunk = true;
        await foreach (var chunk in _client.CompleteChatStreamingAsync(openAiMessages))
        {
            if (chunk?.ContentUpdate is { Count: > 0 })
            {
                if (firstChunk)
                {
                    stopwatch.Start();

                    logEntry.Model = chunk.Model;

                    firstChunk = false;
                }

                var text = chunk.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    responseBuilder.Append(text);
                    yield return text;
                }
            }

            if (chunk?.Usage != null)
            {
                lastUsage = chunk.Usage;
            }
        }

        stopwatch.Stop();
        logEntry.Latency = stopwatch.ElapsedMilliseconds;
        logEntry.ShortResponse = responseBuilder.ToString();

        if (lastUsage != null)
        {
            logEntry.TokensTotal = lastUsage.TotalTokenCount;
            logEntry.TokensPrompt = lastUsage.InputTokenCount;
            logEntry.TokensCompletion = lastUsage.OutputTokenCount;
            logEntry.Cost = (decimal)(((lastUsage.TotalTokenCount - lastUsage.InputTokenDetails.CachedTokenCount) * _costFullToken) + (lastUsage.InputTokenDetails.CachedTokenCount * _costCachedToken) + (lastUsage.OutputTokenCount * _costOutputToken));
        }

        LoggerService.AddEntry(logEntry);
    }
}
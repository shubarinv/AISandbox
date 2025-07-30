using System.Diagnostics;
using System.Text;
using AISandbox.Extensions;
using AISandbox.Models;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Chat;
using ChatMessage = AISandbox.Models.ChatMessage;

namespace AISandbox.Services.Implementations;

public class AzureAiChatService : AiChatService
{
    private readonly double _costCachedToken = 0.03 / 1000000; // $0.025 per million cached tokens

    private readonly double _costFullToken = 0.10 / 1000000; // $0.10 per million tokens
    private readonly double _costOutputToken = 0.40 / 1000000; // $0.10 per million output tokens
    private ChatClient? _chatClient;

    public AzureAiChatService()
    {
        Provider = "Azure AI Foundry";
    }

    public override Task InitializeAsync()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_PROJECT_ENDPOINT");

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("AZURE_PROJECT_ENDPOINT environment variable is not set.");
        }

        var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_MODEL_DEPLOYMENT_NAME");

        if (string.IsNullOrEmpty(modelDeploymentName))
        {
            throw new InvalidOperationException("AZURE_MODEL_DEPLOYMENT_NAME environment variable is not set.");
        }

        AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

        _chatClient = projectClient.GetAzureOpenAIChatClient(deploymentName: modelDeploymentName);

        return base.InitializeAsync();
    }

    public override async Task<string> SendMessageAsync(List<ChatMessage> messages)
    {
        if (_chatClient == null)
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var openAiMessages = messages.Select(x => x.ToOpenAIChatMessage()).ToList();

        try
        {
            var response = await _chatClient!.CompleteChatAsync(openAiMessages);
            var assistantMessage = response.Value.Content[0].Text;
            return assistantMessage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return $"Error: {e.Message}";
        }
    }

    public override async IAsyncEnumerable<string> StreamMessageAsync(List<ChatMessage> messages)
    {
        if (_chatClient == null)
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
        await foreach (var chunk in _chatClient.CompleteChatStreamingAsync(openAiMessages))
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
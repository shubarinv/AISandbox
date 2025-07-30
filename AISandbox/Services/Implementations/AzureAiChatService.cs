using AISandbox.Extensions;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Chat;
using ChatMessage = AISandbox.Models.ChatMessage;

namespace AISandbox.Services.Implementations;

public class AzureAiChatService : AiChatService
{
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
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var openAiMessages = messages.Select(x => x.ToOpenAIChatMessage()).ToList();


        await foreach (var chunk in _chatClient!.CompleteChatStreamingAsync(openAiMessages))
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
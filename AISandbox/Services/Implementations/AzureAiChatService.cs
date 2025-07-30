using AISandbox.Extensions;
using AISandbox.Models;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Chat;

namespace AISandbox.Services.Implementations;

public class AzureAiChatService : AiChatService
{
    private ChatClient? _chatClient;

    public override Task InitializeAsync()
    {
        var endpoint = System.Environment.GetEnvironmentVariable("AZURE_PROJECT_ENDPOINT");
        
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("AZURE_PROJECT_ENDPOINT environment variable is not set.");
        }
        
        var modelDeploymentName = System.Environment.GetEnvironmentVariable("AZURE_MODEL_DEPLOYMENT_NAME");
        
        if (string.IsNullOrEmpty(modelDeploymentName))
        {
            throw new InvalidOperationException("AZURE_MODEL_DEPLOYMENT_NAME environment variable is not set.");
        }
        
        AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

        _chatClient = projectClient.GetAzureOpenAIChatClient(deploymentName: modelDeploymentName);

        return base.InitializeAsync();
    }

    public override async Task<string> SendMessageAsync(string message)
    {
        if (_chatClient == null)
        {
            throw new InvalidOperationException("Chat client is not initialized. Call InitializeAsync first.");
        }

        var messages = GetMessages();

        messages.Add(new UserChatMessage(message));
        AddMessage(message, MessageType.User);

        try
        {
            var response = await _chatClient!.CompleteChatAsync(messages);
            var assistantMessage = response.Value.Content[0].Text;
            AddMessage(assistantMessage, MessageType.Assistant);
            return assistantMessage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return $"Error: {e.Message}";
        }
        
     
    }

    private List<OpenAI.Chat.ChatMessage> GetMessages()
    {
        return Messages.Select(x => x.ToOpenAIChatMessage()).ToList();
    }
}
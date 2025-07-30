using System.Text;
using AISandbox.Models;
using AISandbox.Services.Implementations;

namespace AISandbox.Services;

public class AiServicesOrchestrator
{
    private readonly List<IAiChatService> _aiChatServices;
    private IAiChatService? _activeAiChatService;

    public AiServicesOrchestrator()
    {
        _aiChatServices =
        [
            new AzureAiChatService(),
            new OpenAiChatService()
        ];
    }

    protected List<ChatMessage> Messages { get; set; } = [];

    public string ActiveAiChatServiceProvider => _activeAiChatService?.Provider ?? "None";

    public async Task InitializeAsync()
    {
        foreach (var service in _aiChatServices)
        {
            try
            {
                await service.InitializeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        _activeAiChatService = _aiChatServices.FirstOrDefault(x => x.IsInitialized);
        if (_activeAiChatService == null)
        {
            throw new InvalidOperationException("No AI chat service is initialized.");
        }

        Console.WriteLine($"Active AI Chat Service: {_activeAiChatService.Provider}");
    }

    public void SetSystemMessage(string systemMessage)
    {
        if (Messages.Count == 0)
        {
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

    public void SetActiveAiChatService(string provider)
    {
        var newProvider = _aiChatServices.FirstOrDefault(x => x.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
        _activeAiChatService = newProvider ?? throw new InvalidOperationException($"No AI chat service found with provider: {provider}");

        Console.WriteLine($"Active AI Chat Service set to: {_activeAiChatService.Provider}");
    }

    public List<string> GetActiveAiChatServices()
    {
        return _aiChatServices
            .Where(x => x.IsInitialized)
            .Select(x => x.Provider)
            .ToList();
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (_activeAiChatService != null)
        {
            Messages.Add(new ChatMessage
            {
                Message = message,
                Role = MessageType.User
            });

            var response = await _activeAiChatService.SendMessageAsync(Messages);

            Messages.Add(new ChatMessage
            {
                Message = response,
                Role = MessageType.Assistant
            });

            return response;
        }

        Console.WriteLine("No active AI chat service available.");
        return string.Empty;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string message)
    {
        if (_activeAiChatService == null)
        {
            Console.WriteLine("No active AI chat service available for streaming.");
            yield break;
        }

        Messages.Add(new ChatMessage
        {
            Message = message,
            Role = MessageType.User
        });

        var responseStringBuilder = new StringBuilder();

        await foreach (var response in _activeAiChatService.StreamMessageAsync(Messages))
        {
            responseStringBuilder.Append(response);
            yield return response;
        }

        var responseMessage = responseStringBuilder.ToString();
        Messages.Add(new ChatMessage
        {
            Message = responseMessage,
            Role = MessageType.Assistant
        });
    }
}
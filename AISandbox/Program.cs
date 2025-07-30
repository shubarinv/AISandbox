using AISandbox.Services;

var aiServicesOrchestrator = new AiServicesOrchestrator();
await aiServicesOrchestrator.InitializeAsync();
aiServicesOrchestrator.SetSystemMessage("You are a helpful assistant.");

while (true)
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
    {
        continue;
    }

    if (userMessage.Equals("/stop", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (userMessage.Equals("/set", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"Current AI provider: {aiServicesOrchestrator.ActiveAiChatServiceProvider}; Available providers: {string.Join(", ", aiServicesOrchestrator.GetActiveAiChatServices())}");

        Console.Write("Enter new AI provider: ");
        userMessage = Console.ReadLine();
        var provider = userMessage?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(provider))
        {
            Console.WriteLine("Provider cannot be empty.");
            continue;
        }

        try
        {
            aiServicesOrchestrator.SetActiveAiChatService(provider);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error setting active AI chat service: {e.Message}");
        }

        continue;
    }

    Console.Write($"[{aiServicesOrchestrator.ActiveAiChatServiceProvider}-AI]: ");
    // Send the message to the AI chat service and get the response

    var chunkId = 0;

    await foreach (var chunk in aiServicesOrchestrator.StreamMessageAsync(userMessage))
    {
        Console.Write(chunk);
        chunkId++;
    }

    Console.WriteLine();
}
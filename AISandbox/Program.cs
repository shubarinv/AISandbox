using AISandbox.Services.Implementations;

var azureAiChatService = new AzureAiChatService();
await azureAiChatService.InitializeAsync();
var openAiChatService = new OpenAiChatService();
await openAiChatService.InitializeAsync();
azureAiChatService.SetSystemMessage("You are a helpful assistant.");
openAiChatService.SetSystemMessage("You are a helpful assistant.");

while (true)
{
    Console.Write("You: ");
    var userMessage = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userMessage))
        continue;

    if (userMessage.Equals("/stop", StringComparison.OrdinalIgnoreCase))
        break;

    // Send the message to the AI chat service and get the response
    var response = await azureAiChatService.SendMessageAsync(userMessage);
    Console.WriteLine($"[Azure-AI]: {response}");
    
    response= await openAiChatService.SendMessageAsync(userMessage);
    Console.WriteLine($"[OpenAI]: {response}");
    
    
}

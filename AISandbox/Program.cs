using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Chat;

// ChatClient client = new(model: "gpt-4.1-nano", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
//
// ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");
//
// Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");

var endpoint = System.Environment.GetEnvironmentVariable("AZURE_PROJECT_ENDPOINT");
var modelDeploymentName = System.Environment.GetEnvironmentVariable("AZURE_MODEL_DEPLOYMENT_NAME");
AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

var chatClient = projectClient.GetAzureOpenAIChatClient(deploymentName: modelDeploymentName);
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a helpful assistant."),
};
var userRequest = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(userRequest))
{
    messages.Add(new UserChatMessage(userRequest));
}


var response=chatClient.CompleteChat(messages);

Console.WriteLine($"[ASSISTANT]: {response.Value.Content[0].Text}");

// var connections=projectClient.Connections.GetConnections();
// foreach (var connection in connections)
// {
//     Console.WriteLine($"Connection Name: {connection.Name} - Type: {connection.Type}");
// }

using AISandbox.Models;

namespace AISandbox.Extensions;

public static class ChatMessageExtensions
{
    public static OpenAI.Chat.ChatMessage ToOpenAIChatMessage(this ChatMessage message)
    {
        switch (message.Role)
        {
            case MessageType.System:
                return new OpenAI.Chat.SystemChatMessage(message.Message);
            case MessageType.User:
                return new  OpenAI.Chat.UserChatMessage(message.Message);
            case MessageType.Assistant:
                return new  OpenAI.Chat.AssistantChatMessage(message.Message);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
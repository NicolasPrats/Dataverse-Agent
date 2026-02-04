using Microsoft.Extensions.AI;

namespace TestAgentFramework.Services;

public interface IChatClientFactory
{
    IChatClient CreateChatClient();
}

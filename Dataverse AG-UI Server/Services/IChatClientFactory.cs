using Microsoft.Extensions.AI;

namespace Dataverse_AG_UI_Server.Services;

public interface IChatClientFactory
{
    IChatClient CreateChatClient();
}

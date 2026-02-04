using Microsoft.Extensions.AI;
using TestAgentFramework.Agents;
using TestAgentFramework.Agents.Base;
using TestAgentFramework.Agents.Tools.Tools;
using TestAgentFramework.Model;

namespace TestAgentFramework.Services;

public class AgentFactory(AppConfiguration config, IChatClientFactory chatClientFactory, DataverseAgentTools tools)
{
    private readonly AppConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly IChatClientFactory _chatClientFactory = chatClientFactory ?? throw new ArgumentNullException(nameof(chatClientFactory));


    public DataModelBuilderAgent CreateDataModelBuilderAgent()
    {
        var agent = new DataModelBuilderAgent(tools);
        agent.Initialize(_chatClientFactory.CreateChatClient());
        return agent;
    }

}



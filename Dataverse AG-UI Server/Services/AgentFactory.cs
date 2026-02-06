using Dataverse_AG_UI_Server.Agents;
using Dataverse_AG_UI_Server.Agents.Tools;
using Dataverse_AG_UI_Server.Diagnostics;
using Dataverse_AG_UI_Server.Model;
using Dataverse_AG_UI_Server.Services;

namespace Dataverse_AG_UI_Server.Services;

public class AgentFactory(IDiagnosticBus diagBus, AppConfiguration config, IChatClientFactory chatClientFactory, DataverseServiceClientFactory serviceClientFactory)
{
    private readonly AppConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly IChatClientFactory _chatClientFactory = chatClientFactory ?? throw new ArgumentNullException(nameof(chatClientFactory));
    private readonly DataverseServiceClientFactory _serviceClientFactory = serviceClientFactory ?? throw new ArgumentNullException(nameof(serviceClientFactory));

    public DataModelBuilderAgent CreateDataModelBuilderAgent()
    {
        var dataModelTools = new DataverseDataModelTools(_serviceClientFactory);
        var agent = new DataModelBuilderAgent(diagBus, dataModelTools);
        agent.Initialize(_chatClientFactory.CreateChatClient());
        return agent;
    }

    public UIBuilderAgent CreateUIBuilderAgent()
    {
        var uiTools = new DataverseUITools(_serviceClientFactory);
        var agent = new UIBuilderAgent(diagBus, uiTools);
        agent.Initialize(_chatClientFactory.CreateChatClient());
        return agent;
    }

    public ArchitectAgent CreateArchitectAgent()
    {
        // Create the tools for direct access
        var dataModelTools = new DataverseDataModelTools(_serviceClientFactory);
        var uiTools = new DataverseUITools(_serviceClientFactory);
        
        // Create the specialized agents that the ArchitectAgent can delegate to
        var dataModelBuilderAgent = CreateDataModelBuilderAgent();
        var uiBuilderAgent = CreateUIBuilderAgent();
        
        var agent = new ArchitectAgent(
            diagBus,
            dataModelBuilderAgent, 
            uiBuilderAgent,
            dataModelTools,
            uiTools);
        agent.Initialize(_chatClientFactory.CreateChatClient());
        return agent;
    }

}



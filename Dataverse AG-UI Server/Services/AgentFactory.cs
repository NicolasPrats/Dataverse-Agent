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

    public async Task<DataModelBuilderAgent> CreateDataModelBuilderAgentAsync()
    {
        var dataModelTools = new DataverseDataModelTools(_serviceClientFactory);
        var agent = new DataModelBuilderAgent(diagBus, dataModelTools);
        await agent.InitializeAsync(_chatClientFactory.CreateChatClient());
        return agent;
    }

    public async Task<UIBuilderAgent> CreateUIBuilderAgentAsync()
    {
        var uiTools = new DataverseUITools(_serviceClientFactory);
        var agent = new UIBuilderAgent(diagBus, uiTools);
        await agent.InitializeAsync(_chatClientFactory.CreateChatClient());
        return agent;
    }

    public async Task<HandymanAgent> CreateHandymanAgentAsync ()
    {
        var scriptTools = new ScriptTools(_serviceClientFactory);
        var agent = new HandymanAgent(diagBus, scriptTools);
        await agent.InitializeAsync(_chatClientFactory.CreateChatClient());
        return agent;
    }

    public async Task<ArchitectAgent> CreateArchitectAgentAsync()
    {
        // Create the tools for direct access
        var dataModelTools = new DataverseDataModelTools(_serviceClientFactory);
        var uiTools = new DataverseUITools(_serviceClientFactory);
        
        // Create the specialized agents that the ArchitectAgent can delegate to
        var dataModelBuilderAgent = await CreateDataModelBuilderAgentAsync();
        var uiBuilderAgent = await CreateUIBuilderAgentAsync();
        var handymanAgent = await CreateHandymanAgentAsync();

        var agent = new ArchitectAgent(
            diagBus,
            dataModelBuilderAgent, 
            uiBuilderAgent,
            handymanAgent,
            dataModelTools,
            uiTools);
        await agent.InitializeAsync(_chatClientFactory.CreateChatClient());
        return agent;
    }

}



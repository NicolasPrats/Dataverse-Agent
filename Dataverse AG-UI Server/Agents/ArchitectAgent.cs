using TestAgentFramework.Agents.Base;
using TestAgentFramework.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using TestAgentFramework.Agents.Tools.Tools;
using Dataverse_AG_UI_Server.Agents.Tools;
using System.ComponentModel;

namespace TestAgentFramework.Agents;

public class ArchitectAgent : AgentBase
{
    private const string DefaultInstructions = @$"You are a Solution Architect Agent specialized in Microsoft Power Platform and Dynamics 365. 

You will discuss with a business user or with a Power Platform developer who need to do some modifications on an existing environment. 

You have direct access to read-only tools to query the existing data model and UI components:
- dataverse_get_tables: List all tables in the environment
- dataverse_get_table_metadata: Get detailed metadata for a specific table
- dataverse_get_attributes: Get all columns/attributes of a table
- dataverse_get_relationships: Get relationships for a table
- dataverse_get_global_optionsets: List all global option sets (choices) in the environment
- dataverse_get_global_optionset_details: Get detailed information about a specific global option set including all its values
- dataverse_get_views: Get all views for a table
- dataverse_get_forms: Get all forms for a table


Use these tools directly when you need to:
- Understand the current data model structure
- Check existing tables, columns, and relationships
- Review existing global option sets and their values
- Review existing forms and views
- Validate XML before recommending changes

When the user wants to implement any new feature, you capture business requirements, assess feasibility, design the solution at a high level.

Once user has approved your design, you drive other agents to do the implementation:
- Transfer to Data Model Builder Agent: For creating/modifying tables, columns, relationships
- Transfer to UI Builder Agent: For creating/modifying forms, views, dashboards
When some implementation is needed but cannot be handled by any of the available agents, you can propose to the user to provide him/her detailed design and instructions.

Your outputs must be unambiguous, consistent, and aligned to best practices, with explicit constraints, risks, and acceptance criteria.
Goals

Clarify and restate the requirement.
Decompose it into independent features with acceptance criteria.
Classify features: OOB/Standard, Custom on Power Platform/D365, or Not suitable (recommend alternative).
Your design must include data model, security model, UI impacts, automation, integrations
Ensure downstream agents can implement without further interpretation.

Method

Elicit & Clarify: Ask minimal targeted questions only if critical information is missing to determine feasibility and design. Use your read-only tools to explore the existing environment.
Feature Decomposition: Split into atomic features; define dependencies and acceptance criteria.
Feasibility & Fit: Evaluate feasibility within Power Platform/D365; mark Yes/Partial/No with justification.
Design Choices:

Prefer configuration/OOB first.
Use pro‑code (plugins, PCF, Azure) when needed for robustness, performance, or governance.
Allow low‑code only when code first options are too complex or costly.


Architecture:

Data model: tables, fields, relationships, ownership, column types, calculated/rollup, alternate keys.
Security: roles, row‑level security, teams, field security profiles, environment DLP.
UX: model‑driven apps, canvas apps, forms/views/dashboards, command bars, controls/PCF.
Business logic: Power Automate vs Plugins vs JavaScript vs Business Rules; triggers; idempotency; error handling.
Integrations: connectors, APIs, virtual tables, eventing (Dataverse events), rate limits, retry policies, secrets handling.

Available Transfer Tools:
- transfer_to_datamodel_builder: When you need to CREATE or MODIFY the data model (create tables, add columns, create relationships, option sets), transfer the conversation to the Data Model Builder Agent. Use this only for WRITE operations. For READ operations, use your direct tools.
- transfer_to_ui_builder: When you need to CREATE or MODIFY the user interface (create/update forms, create/update views, design dashboards), transfer the conversation to the UI Builder Agent. Use this only for WRITE operations. For READ operations, use your direct tools. Please ensure to give logical names to the UIBuilderAgent.





Content Quality Guardrails

No ambiguous statements.
No hidden assumptions—state assumptions explicitly.
Provide at least one implementation option per custom feature with pros/cons and decision justification.
";

    private readonly DataModelBuilderAgent? _dataModelBuilderAgent;
    private readonly UIBuilderAgent? _uiBuilderAgent;
    private readonly DataverseDataModelTools? _dataModelTools;
    private readonly DataverseUITools? _uiTools;

    public ArchitectAgent(
        DataModelBuilderAgent? dataModelBuilderAgent = null, 
        UIBuilderAgent? uiBuilderAgent = null,
        DataverseDataModelTools? dataModelTools = null,
        DataverseUITools? uiTools = null)
        : base("Architect", DefaultInstructions)
    {
        _dataModelBuilderAgent = dataModelBuilderAgent;
        _uiBuilderAgent = uiBuilderAgent;
        _dataModelTools = dataModelTools;
        _uiTools = uiTools;
    }

    [Description("Transfer the conversation to the Data Model Builder Agent to CREATE or MODIFY the data model (create tables, add columns, create relationships, option sets). Do NOT use this for reading data model - use the direct read tools instead.")]
    private async Task<string> TransferToDataModelBuilderAsync(
        [Description("The request to send to the Data Model Builder Agent")] string request)
    {
        if (_dataModelBuilderAgent == null)
        {
            return "Error: Data Model Builder Agent is not available. Cannot transfer the conversation.";
        }

        try
        {
            var result = await _dataModelBuilderAgent.RunAsync(request);
            return $"Data Model Builder Agent response:\n{result}";
        }
        catch (Exception ex)
        {
            return $"Error transferring to Data Model Builder Agent: {ex.Message}";
        }
    }

    [Description("Transfer the conversation to the UI Builder Agent to CREATE or MODIFY user interface components (create/update forms, create/update views, design layouts). Do NOT use this for reading UI components - use the direct read tools instead.")]
    private async Task<string> TransferToUIBuilderAsync(
        [Description("The request to send to the UI Builder Agent")] string request)
    {
        if (_uiBuilderAgent == null)
        {
            return "Error: UI Builder Agent is not available. Cannot transfer the conversation.";
        }

        try
        {
            var result = await _uiBuilderAgent.RunAsync(request);
            return $"UI Builder Agent response:\n{result}";
        }
        catch (Exception ex)
        {
            return $"Error transferring to UI Builder Agent: {ex.Message}";
        }
    }

    protected override AIAgent BuildAgent(IChatClient chatClient)
    {
        var tools = new List<AIFunction>();

        // Add read-only tools for direct access to data model and UI information
        if (_dataModelTools != null)
        {
            tools.AddRange(_dataModelTools.ReadOnlyTools);
        }

        if (_uiTools != null)
        {
            tools.AddRange(_uiTools.ReadOnlyTools);
        }

        // Add transfer tools if agents are available
        if (_dataModelBuilderAgent != null)
        {
            tools.Add(AIFunctionFactory.Create(TransferToDataModelBuilderAsync, "transfer_to_datamodel_builder"));
        }

        if (_uiBuilderAgent != null)
        {
            tools.Add(AIFunctionFactory.Create(TransferToUIBuilderAsync, "transfer_to_ui_builder"));
        }

        return chatClient.AsAIAgent(
            instructions: Instructions,
            name: Name,
            tools: [.. tools]
        );
    }
}



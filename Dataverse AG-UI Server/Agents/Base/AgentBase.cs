using Dataverse_AG_UI_Server.Agents.Base;
using Dataverse_AG_UI_Server.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace Dataverse_AG_UI_Server.Agents.Base;

public abstract class AgentBase : IAgent
{
    public IDiagnosticBus DiagBus { get; }
    public string Name { get; protected set; }
    public string Instructions { get; protected set; }
    public AIAgent InternalAgent { get; protected set; }

    private List<AIFunction> ToolFunctions { get; } = [];

    protected AgentBase(IDiagnosticBus diagBus, string name, string instructions)
    {
        DiagBus = diagBus;
        Name = name;
        Instructions = instructions;
        InternalAgent = null!; // Will be initialized in BuildAgent
    }

    protected void AddTools(params Tool[] tools)
    {
        foreach (var tool in tools)
        {
            var aiFunction = AIFunctionFactory.Create(tool.Delegate, tool.Name);
            var diagnosticFunction = new DiagnosticAIFunction(aiFunction, Name, tool.Name, DiagBus, TargetType.Tool);
            this.ToolFunctions.Add(diagnosticFunction);
        }
    }

    protected void AddTool(Tool tool)
    {
        var aiFunction = AIFunctionFactory.Create(tool.Delegate, tool.Name);
        var diagnosticFunction = new DiagnosticAIFunction(aiFunction, Name, tool.Name, DiagBus, TargetType.Tool);
        this.ToolFunctions.Add(diagnosticFunction);
    }

    protected void AddAgentTool(AgentBase targetAgent, string toolName, string description)
    {
        var aiFunction = AIFunctionFactory.Create(targetAgent.TransferRequestAsync, toolName, description);
        var diagnosticFunction = new DiagnosticAIFunction(aiFunction, Name, targetAgent.Name, DiagBus, TargetType.Agent);
        this.ToolFunctions.Add(diagnosticFunction);
    }


    public void Initialize(IChatClient chatClient)
    {
        InternalAgent = chatClient.AsAIAgent(
            instructions: Instructions,
            name: Name,
            tools: [.. ToolFunctions]
            
        );
    }

    public virtual async Task<string> RunAsync(string input)
    {
        return await RunAsync(input, CancellationToken.None);
    }

    public virtual async Task<string> RunAsync(string input, CancellationToken cancellationToken)
    {
        if (InternalAgent == null)
            throw new InvalidOperationException($"Agent '{Name}' has not been initialized. Call Initialize() first.");

        var result = await InternalAgent.RunAsync(input, cancellationToken: cancellationToken);
        return result?.ToString() ?? string.Empty;
    }

    private async Task<string> TransferRequestAsync(
    [Description("The request to send to the agent")] string request)
    {
        try
        {
            var result = await this.RunAsync(request);
            return $"Agent response:\n{result}";
        }
        catch (Exception ex)
        {
            return $"Error transferring to Agent: {ex.Message}";
        }
    }

}


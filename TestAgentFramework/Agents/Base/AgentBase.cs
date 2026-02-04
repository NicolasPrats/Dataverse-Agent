using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace TestAgentFramework.Agents.Base;

public abstract class AgentBase : IAgent
{
    public string Name { get; protected set; }
    public string Instructions { get; protected set; }
    public AIAgent Agent { get; protected set; }

    protected AgentBase(string name, string instructions)
    {
        Name = name;
        Instructions = instructions;
        Agent = null!; // Will be initialized in BuildAgent
    }

    protected abstract AIAgent BuildAgent(IChatClient chatClient);

    public void Initialize(IChatClient chatClient)
    {
        Agent = BuildAgent(chatClient);
    }

    public virtual async Task<string> RunAsync(string input)
    {
        return await RunAsync(input, CancellationToken.None);
    }

    public virtual async Task<string> RunAsync(string input, CancellationToken cancellationToken)
    {
        if (Agent == null)
            throw new InvalidOperationException($"Agent '{Name}' has not been initialized. Call Initialize() first.");
            
        var result = await Agent.RunAsync(input);
        return result?.ToString() ?? string.Empty;
    }
}


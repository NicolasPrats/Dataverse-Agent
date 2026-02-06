using Dataverse_AG_UI_Server.Agents.Base;

namespace Dataverse_AG_UI_Server.Services;

public class AgentOrchestrator
{
    private readonly Dictionary<string, IAgent> _agents = [];

    public void RegisterAgent(string key, IAgent agent)
    {
        _agents[key] = agent;
    }

    public IAgent? GetAgent(string key)
    {
        return _agents.TryGetValue(key, out var agent) ? agent : null;
    }

    public async Task<string> RunAgentAsync(string agentKey, string input)
    {
        var agent = GetAgent(agentKey);
        if (agent == null)
        {
            throw new InvalidOperationException($"Agent '{agentKey}' not found. Available agents: {string.Join(", ", _agents.Keys)}");
        }

        return await agent.RunAsync(input);
    }

    public IEnumerable<string> GetRegisteredAgents()
    {
        return _agents.Keys;
    }

    public async Task<Dictionary<string, string>> RunMultipleAgentsAsync(string input, params string[] agentKeys)
    {
        var results = new Dictionary<string, string>();
        
        foreach (var key in agentKeys)
        {
            try
            {
                var result = await RunAgentAsync(key, input);
                results[key] = result;
            }
            catch (Exception ex)
            {
                results[key] = $"Error: {ex.Message}";
            }
        }

        return results;
    }
}

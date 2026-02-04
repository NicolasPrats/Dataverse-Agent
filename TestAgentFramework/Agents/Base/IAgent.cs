using Microsoft.Agents.AI;

namespace TestAgentFramework.Agents.Base;

public interface IAgent
{
    string Name { get; }
    string Instructions { get; }
    AIAgent Agent { get; }
    Task<string> RunAsync(string input);
    Task<string> RunAsync(string input, CancellationToken cancellationToken);
}

using Microsoft.Agents.AI;

namespace Dataverse_AG_UI_Server.Agents.Base;

public interface IAgent
{
    string Name { get; }
    string Instructions { get; }
    AIAgent InternalAgent { get; }
    Task<string> RunAsync(string input);
    Task<string> RunAsync(string input, CancellationToken cancellationToken);
}

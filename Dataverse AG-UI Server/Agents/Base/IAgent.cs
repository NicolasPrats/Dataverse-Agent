using Microsoft.Agents.AI;

namespace Dataverse_AG_UI_Server.Agents.Base;

public interface IAgent
{
    string Name { get; }
    string Instructions { get; }
    AIAgent? InternalAgent { get; }
}

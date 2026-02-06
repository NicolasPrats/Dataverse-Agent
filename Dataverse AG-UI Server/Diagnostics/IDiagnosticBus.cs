namespace Dataverse_AG_UI_Server.Diagnostics
{
    public interface IDiagnosticBus
    {
        void Publish(AgentDiagnosticEvent evt);
        IAsyncEnumerable<AgentDiagnosticEvent> Stream(CancellationToken ct);
    }

}

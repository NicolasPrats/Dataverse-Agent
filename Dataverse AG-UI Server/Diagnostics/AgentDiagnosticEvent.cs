namespace Dataverse_AG_UI_Server.Diagnostics
{
    public record AgentDiagnosticEvent
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public required string SourceAgent { get; init; }
        public required string Target { get; init; }
        public required TargetType TargetType { get; init; }
        public object? Payload { get; init; }
        public object? Result { get; init; }
        public TimeSpan? Duration { get; init; }
        public Guid EventId { get; init; }
    }

    public enum TargetType
    {
        Tool,
        Agent,
        SimulatedResponse
    }

}

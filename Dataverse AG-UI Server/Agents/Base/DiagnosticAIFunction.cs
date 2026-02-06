using Dataverse_AG_UI_Server.Diagnostics;
using Microsoft.Extensions.AI;
using System.Reflection;
using System.Text.Json;

namespace Dataverse_AG_UI_Server.Agents.Base;

public class DiagnosticAIFunction(AIFunction innerFunction, string agentName, string target, IDiagnosticBus diagBus, TargetType targetType) : AIFunction
{
    private readonly AIFunction _innerFunction = innerFunction;
    private readonly string _agentName = agentName;
    private readonly string _target = target;
    private readonly IDiagnosticBus _diagBus = diagBus;
    private readonly TargetType _targetType = targetType;

    public override IReadOnlyDictionary<string, object?> AdditionalProperties
    {
        get
        {
            return InnerFunction.AdditionalProperties;
        }
    }

    public override string Name => innerFunction.Name;
    public override MethodInfo?UnderlyingMethod => innerFunction.UnderlyingMethod;
    public override JsonElement JsonSchema => innerFunction.JsonSchema;
    public override JsonElement? ReturnJsonSchema => innerFunction.ReturnJsonSchema;
    public override JsonSerializerOptions JsonSerializerOptions => innerFunction.JsonSerializerOptions;

    public AIFunction InnerFunction => _innerFunction;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments? arguments,
        CancellationToken cancellationToken)
    {
        var id= Guid.NewGuid(); 
        var startEvent = new AgentDiagnosticEvent
        {
            EventId = id,
            SourceAgent = _agentName,
            Target = _target,
            TargetType = _targetType,
            Payload = new { Status = "Calling", Arguments = arguments }
        };
        
        _diagBus.Publish(startEvent);
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await InnerFunction.InvokeAsync(arguments, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            var completeEvent = new AgentDiagnosticEvent
            {
                EventId = id,
                SourceAgent = _agentName,
                Target = _target,
                TargetType = _targetType,
                Payload = new { Status = "Called successfully.", Arguments = arguments },
                Result = result,
                Duration = duration
            };
            _diagBus.Publish(completeEvent);
            if (_targetType == TargetType.Agent)
            {
               var simulateResponseEvent = new AgentDiagnosticEvent
                {
                    EventId = Guid.NewGuid(),
                   SourceAgent = _target,
                    Target = _agentName,
                    TargetType = TargetType.SimulatedResponse,
                    Payload = new { Status = "Called successfully.", Arguments = arguments },
                    Result = result,
                    Duration = duration
                };
                _diagBus.Publish(simulateResponseEvent);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            var errorEvent = new AgentDiagnosticEvent
            {
                EventId = id,
                SourceAgent = _agentName,
                Target = _target,
                TargetType = _targetType,
                Payload = new { Status = "Called failed.", Arguments = arguments },
                Result = ex,                
            };
            _diagBus.Publish(errorEvent);
            throw;
        }
    }
}

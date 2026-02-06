using Dataverse_AG_UI_Server.Diagnostics;
using Microsoft.Extensions.AI;
using System.Reflection;
using System.Text.Json;

namespace Dataverse_AG_UI_Server.Agents.Base;

public class DiagnosticAIFunction(AIFunction innerFunction, string agentName, string target, IDiagnosticBus diagBus) : AIFunction
{
    private readonly AIFunction _innerFunction = innerFunction;
    private readonly string _agentName = agentName;
    private readonly string _target = target;
    private readonly IDiagnosticBus _diagBus = diagBus;

    public override IReadOnlyDictionary<string, object?> AdditionalProperties
    {
        get
        {
            return _innerFunction.AdditionalProperties;
        }
    }

    public override string Name => innerFunction.Name;
    public override MethodInfo?UnderlyingMethod => innerFunction.UnderlyingMethod;
    public override JsonElement JsonSchema => innerFunction.JsonSchema;
    public override JsonElement? ReturnJsonSchema => innerFunction.ReturnJsonSchema;
    public override JsonSerializerOptions JsonSerializerOptions => innerFunction.JsonSerializerOptions;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments? arguments,
        CancellationToken cancellationToken)
    {
        var startEvent = new AgentDiagnosticEvent
        {
            SourceAgent = _agentName,
            Target = _target,
            Payload = new { Phase = "Start", Arguments = arguments }
        };
        
        _diagBus.Publish(startEvent);
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await _innerFunction.InvokeAsync(arguments, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            var completeEvent = new AgentDiagnosticEvent
            {
                SourceAgent = _agentName,
                Target = _target,
                Payload = new { Phase = "Complete" },
                Result = result,
                Duration = duration
            };
            _diagBus.Publish(completeEvent);
            
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            var errorEvent = new AgentDiagnosticEvent
            {
                SourceAgent = _agentName,
                Target = _target,
                Payload = new { Phase = "Error", Error = ex.Message },
                Duration = duration
            };
            _diagBus.Publish(errorEvent);
            throw;
        }
    }
}

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using TestAgentFramework.Agents.Base;
using TestAgentFramework.Agents.Tools.Tools;
using TestAgentFramework.Model;
using TestAgentFramework.Services;

namespace TestAgentFramework.Examples;

public static class MultiAgentExample
{
    public static async Task RunExamplesAsync(IConfiguration configuration)
    {
        Console.WriteLine("=== Multi-Agent Framework Example ===\n");

        var appConfig = new AppConfiguration();
        configuration.Bind(appConfig);
        appConfig.AzureOpenAI.ApiKey = configuration["AZURE_OPENAI_API_KEY"] ?? string.Empty;
        appConfig.Validate();

        var dataverseTools = new DataverseAgentTools(appConfig.Dataverse);

     

        var chatClientFactory = new AzureOpenAIChatClientFactory(appConfig);
        var agentFactory = new AgentFactory(appConfig, chatClientFactory, dataverseTools);
        var orchestrator = new AgentOrchestrator();

        Console.WriteLine("Creating agents...");
        var dataModelAgent = agentFactory.CreateDataModelBuilderAgent();

        orchestrator.RegisterAgent("datamodel", dataModelAgent);


        Console.WriteLine($"Registered agents: {string.Join(", ", orchestrator.GetRegisteredAgents())}\n");



        Console.WriteLine("=== Testing DataModel Agent ===");
        AgentSession session = await dataModelAgent.Agent.GetNewSessionAsync();
        Console.WriteLine("Ready...");
        while(true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            var prompt = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrWhiteSpace(prompt))
                break;
            var response = await dataModelAgent.Agent.RunAsync(prompt, session);


            var approvalRequests = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionApprovalRequestContent>()
                .ToList();

            foreach (var req in approvalRequests)
            {
                Console.WriteLine($"Agent wants to use this tool which requires human validation: {req.FunctionCall.Name}");
                foreach (var param in req.FunctionCall.Arguments)
                {
                    Console.WriteLine($" - {param.Key} : {param.Value}");
                }
            }


           
            Console.WriteLine($"Response: {response}\n");
        }





    }
}

public class SimpleServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = [];

    public void AddService<T>(T service) where T : notnull
    {
        _services[typeof(T)] = service;
    }

    public object? GetService(Type serviceType)
    {
        return _services.TryGetValue(serviceType, out var service) ? service : null;
    }
}

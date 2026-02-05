using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using TestAgentFramework.Agents.Tools.Tools;
using TestAgentFramework.Model;
using TestAgentFramework.Services;

namespace Dataverse_AG_UI_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient().AddLogging();
            builder.Services.AddAGUI();

            WebApplication app = builder.Build();


            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
            var appConfig = new AppConfiguration();
            configuration.Bind(appConfig);
            appConfig.AzureOpenAI.ApiKey = configuration["AZURE_OPENAI_API_KEY"] ?? string.Empty;
            appConfig.Validate();


            var dataverseTools = new DataverseAgentTools(appConfig.Dataverse);

            var chatClientFactory = new AzureOpenAIChatClientFactory(appConfig);
            var agentFactory = new AgentFactory(appConfig, chatClientFactory, dataverseTools);
            var orchestrator = new AgentOrchestrator();

            var dataModelAgent = agentFactory.CreateDataModelBuilderAgent();
            var architectAgent = agentFactory.CreateArchitectAgent();

            app.MapAGUI("/", architectAgent.InternalAgent);
            app.MapAGUI("/architect", architectAgent.InternalAgent);
            app.MapAGUI("/datamodel", dataModelAgent.InternalAgent);
            app.Run();
        }
    }
}

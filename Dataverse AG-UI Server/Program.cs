using Azure.AI.OpenAI;
using Azure.Identity;
using Dataverse_AG_UI_Server.Diagnostics;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text.Json;
using Dataverse_AG_UI_Server.Agents.Tools.Tools;
using Dataverse_AG_UI_Server.Model;
using Dataverse_AG_UI_Server.Services;

namespace Dataverse_AG_UI_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient().AddLogging();
            builder.Services.AddAGUI();
            builder.Services.AddSingleton<IDiagnosticBus, DiagnosticBus>();


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


            var serviceClientFactory = new DataverseServiceClientFactory(appConfig.Dataverse);
            var diagBus = app.Services.GetRequiredService<IDiagnosticBus>();

            var chatClientFactory = new AzureOpenAIChatClientFactory(appConfig);
            var agentFactory = new AgentFactory(diagBus, appConfig, chatClientFactory, serviceClientFactory);
            var orchestrator = new AgentOrchestrator();

            var architectAgent = agentFactory.CreateArchitectAgent();
            var dataModelAgent = agentFactory.CreateDataModelBuilderAgent();
            var uiBuilderAgent = agentFactory.CreateUIBuilderAgent();
            var handymanAgent = agentFactory.CreateHandymanAgent();

            app.MapAGUI("/", architectAgent.InternalAgent);
            app.MapAGUI("/architect", architectAgent.InternalAgent);
            app.MapAGUI("/datamodel", dataModelAgent.InternalAgent);
            app.MapAGUI("/ui", uiBuilderAgent.InternalAgent);
            app.MapAGUI("/handyman", handymanAgent.InternalAgent);

            app.MapGet("/diagnostics", async (
    HttpContext ctx,
    IDiagnosticBus bus) =>
            {
                ctx.Response.Headers.Append("Content-Type", "text/event-stream");
                ctx.Response.Headers.Append("Cache-Control", "no-cache");
                ctx.Response.Headers.Append("Connection", "keep-alive");

                await foreach (var evt in bus.Stream(ctx.RequestAborted))
                {
                    var json = JsonSerializer.Serialize(evt);

                    await ctx.Response.WriteAsync($"data: {json}\n\n");
                    await ctx.Response.Body.FlushAsync();
                }
            });


            app.Run();
        }
    }
}

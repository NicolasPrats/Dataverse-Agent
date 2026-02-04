using Microsoft.Extensions.Configuration;
using TestAgentFramework.Examples;

namespace TestAgentFramework
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();



            await MultiAgentExample.RunExamplesAsync(configuration);
        }
    }
}

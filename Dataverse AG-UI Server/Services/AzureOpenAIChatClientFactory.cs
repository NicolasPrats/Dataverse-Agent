using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Dataverse_AG_UI_Server.Model;

namespace Dataverse_AG_UI_Server.Services;

public class AzureOpenAIChatClientFactory(AppConfiguration configuration) : IChatClientFactory
{
    private readonly AppConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public IChatClient CreateChatClient()
    {
        _configuration.AzureOpenAI.Validate();
        
        var credential = new AzureKeyCredential(_configuration.AzureOpenAI.ApiKey);
        var openAIClient = new AzureOpenAIClient(new Uri(_configuration.AzureOpenAI.Endpoint), credential);
        return openAIClient.GetChatClient(_configuration.AzureOpenAI.Model).AsIChatClient();
    }
}

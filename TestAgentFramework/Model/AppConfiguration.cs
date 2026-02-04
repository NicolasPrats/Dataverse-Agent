namespace TestAgentFramework.Model;

public class AppConfiguration
{
    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public DataverseSettings Dataverse { get; set; } = new();

    public void Validate()
    {
        AzureOpenAI.Validate();
        Dataverse.Validate();
    }
}


public class AgentDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public List<string> Tools { get; set; } = [];
}

public class AzureOpenAISettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException("AzureOpenAI:Endpoint is not set in configuration.");
        
        if (string.IsNullOrWhiteSpace(Model))
            throw new InvalidOperationException("AzureOpenAI:Model is not set in configuration.");
        
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");
    }
}

public class DataverseSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("Dataverse:ConnectionString must be set in configuration.");
        }
    }
}

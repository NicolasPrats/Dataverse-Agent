using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TestAgentFramework.Model;

namespace TestAgentFramework.Services;

/// <summary>
/// Factory for creating and configuring Dataverse ServiceClient instances.
/// </summary>
public class DataverseServiceClientFactory
{
    private readonly DataverseSettings _configuration;
    private ServiceClient? _serviceClient;
    private int? _baseLcid;

    public DataverseServiceClientFactory(DataverseSettings configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets or creates a ServiceClient instance.
    /// </summary>
    public ServiceClient GetServiceClient()
    {
        if (_serviceClient != null && _serviceClient.IsReady)
        {
            return _serviceClient;
        }

        _serviceClient = new ServiceClient(_configuration.ConnectionString);

        if (!_serviceClient.IsReady)
        {
            throw new InvalidOperationException($"Failed to connect to Dataverse: {_serviceClient.LastError}");
        }

        return _serviceClient;
    }

    /// <summary>
    /// Retrieves the base language LCID from the Dataverse organization.
    /// Returns 1033 (English US) as a fallback if unable to retrieve.
    /// </summary>
    public int GetBaseLcid()
    {
        if (_baseLcid.HasValue)
        {
            return _baseLcid.Value;
        }

        try
        {
            var serviceClient = GetServiceClient();
            
            var query = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet("languagecode"),
                TopCount = 1
            };

            var results = serviceClient.RetrieveMultiple(query);
            
            if (results.Entities.Count > 0 && results.Entities[0].Contains("languagecode"))
            {
                _baseLcid = results.Entities[0].GetAttributeValue<int>("languagecode");
                return _baseLcid.Value;
            }
            
            // Fallback to English (US) if unable to retrieve
            _baseLcid = 1033;
            return _baseLcid.Value;
        }
        catch
        {
            // Fallback to English (US) in case of any error
            _baseLcid = 1033;
            return _baseLcid.Value;
        }
    }

    /// <summary>
    /// Disposes the ServiceClient if it exists.
    /// </summary>
    public void Dispose()
    {
        _serviceClient?.Dispose();
        _serviceClient = null;
    }
}

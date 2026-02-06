using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Dataverse_AG_UI_Server.Model;
using Dataverse_AG_UI_Server.Services;

namespace Dataverse_AG_UI_Server.Agents.Tools;

/// <summary>
/// Base class for all Dataverse tools, providing common functionality for solution management.
/// </summary>
public abstract class DataverseToolsBase
{
    protected const string SolutionUniqueName = "AgentCustomizations";
    protected const string SolutionDisplayName = "Agent Customizations";
    protected const string PublisherPrefix = "agent";

    protected readonly ServiceClient ServiceClient;
    protected readonly int BaseLcid;
    private Guid? _solutionId;

    protected DataverseToolsBase(DataverseServiceClientFactory serviceClientFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceClientFactory);

        ServiceClient = serviceClientFactory.GetServiceClient();
        BaseLcid = serviceClientFactory.GetBaseLcid();
    }

    /// <summary>
    /// Ensures the solution exists in Dataverse. Creates it if needed and createIfNotExists is true.
    /// </summary>
    /// <param name="createIfNotExists">If true, creates the solution if it doesn't exist. If false, throws an exception.</param>
    /// <returns>The GUID of the solution.</returns>
    /// <exception cref="InvalidOperationException">Thrown when solution doesn't exist and createIfNotExists is false.</exception>
    protected Guid EnsureSolutionExists(bool createIfNotExists = true)
    {
        if (_solutionId.HasValue)
            return _solutionId.Value;

        // Check if solution already exists
        var query = new QueryExpression("solution")
        {
            ColumnSet = new ColumnSet("solutionid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("uniquename", ConditionOperator.Equal, SolutionUniqueName)
                }
            }
        };

        var results = ServiceClient.RetrieveMultiple(query);

        if (results.Entities.Count > 0)
        {
            _solutionId = results.Entities[0].Id;
            return _solutionId.Value;
        }

        if (!createIfNotExists)
        {
            throw new InvalidOperationException(
                $"Solution '{SolutionUniqueName}' not found. Please ensure the solution exists before creating components.");
        }

        // Solution doesn't exist, create it
        var publisherId = EnsurePublisherExists();

        var solution = new Entity("solution")
        {
            ["uniquename"] = SolutionUniqueName,
            ["friendlyname"] = SolutionDisplayName,
            ["publisherid"] = new EntityReference("publisher", publisherId),
            ["version"] = "1.0.0.0",
            ["description"] = "Solution containing all customizations created by AI agents"
        };

        _solutionId = ServiceClient.Create(solution);
        return _solutionId.Value;
    }

    /// <summary>
    /// Ensures the publisher exists in Dataverse. Creates it if it doesn't exist.
    /// </summary>
    /// <returns>The GUID of the publisher.</returns>
    protected Guid EnsurePublisherExists()
    {
        // Check if publisher already exists
        var query = new QueryExpression("publisher")
        {
            ColumnSet = new ColumnSet("publisherid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("customizationprefix", ConditionOperator.Equal, PublisherPrefix)
                }
            }
        };

        var results = ServiceClient.RetrieveMultiple(query);

        if (results.Entities.Count > 0)
        {
            return results.Entities[0].Id;
        }

        // Publisher doesn't exist, create it
        var publisher = new Entity("publisher")
        {
            ["uniquename"] = "AgentPublisher",
            ["friendlyname"] = "Agent Publisher",
            ["customizationprefix"] = PublisherPrefix,
            ["customizationoptionvalueprefix"] = 10000,
            ["description"] = "Publisher for AI agent customizations"
        };

        return ServiceClient.Create(publisher);
    }

    /// <summary>
    /// Adds a component to the solution.
    /// </summary>
    /// <param name="componentId">The GUID of the component to add.</param>
    /// <param name="componentType">The type of the component.</param>
    protected void AddComponentToSolution(Guid componentId, SolutionComponentType componentType)
    {
        EnsureSolutionExists();

        var request = new AddSolutionComponentRequest
        {
            ComponentId = componentId,
            ComponentType = (int)componentType,
            SolutionUniqueName = SolutionUniqueName,
            AddRequiredComponents = true
        };

        ServiceClient.Execute(request);
    }

    /// <summary>
    /// Creates a standardized error response object.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="additionalContext">Optional additional context to include in the error response.</param>
    /// <returns>An anonymous object containing error details.</returns>
    protected object CreateErrorResponse(Exception ex, object? additionalContext = null)
    {
        var errorResponse = new
        {
            Success = false,
            Error = ex.Message,
            ErrorType = ex.GetType().Name,
            Details = ex.InnerException?.Message
        };

        if (additionalContext == null)
        {
            return errorResponse;
        }

        // Merge the error response with additional context
        var errorDict = new Dictionary<string, object?>
        {
            ["Success"] = false,
            ["Error"] = ex.Message,
            ["ErrorType"] = ex.GetType().Name,
            ["Details"] = ex.InnerException?.Message
        };

        // Add properties from additional context
        foreach (var prop in additionalContext.GetType().GetProperties())
        {
            errorDict[prop.Name] = prop.GetValue(additionalContext);
        }

        return errorDict;
    }
}

using Microsoft.Xrm.Sdk;
using TestAgentFramework.Model;
using ModelAttributeMetadata = TestAgentFramework.Model.AttributeMetadata;
using ModelOptionMetadata = TestAgentFramework.Model.OptionMetadata;

namespace TestAgentFramework.Services;

public interface IDataverseService : IDisposable
{
    Task<bool> TestConnectionAsync();
    Task<IEnumerable<TableMetadata>> GetTablesAsync();
    Task<TableMetadata?> GetTableMetadataAsync(string logicalName);
    Task<IEnumerable<ModelAttributeMetadata>> GetAttributesAsync(string entityLogicalName);
    Task<IEnumerable<RelationshipMetadata>> GetRelationshipsAsync(string entityLogicalName);
    Task<string> CreateTableAsync(string logicalName, string schemaName, string displayName, string? description = null);
    Task UpdateTableAsync(string logicalName, string? displayName = null, string? description = null);
    Task<string> CreateAttributeAsync(string entityLogicalName, string attributeLogicalName, string attributeType, 
        string displayName, string? description = null, bool isRequired = false, int? maxLength = null);
    Task UpdateAttributeAsync(string entityLogicalName, string attributeLogicalName, 
        string? displayName = null, string? description = null);
    Task<string> CreateOptionSetAsync(string entityLogicalName, string attributeLogicalName, string displayName, 
        List<ModelOptionMetadata> options, string? description = null);
    Task<Guid> CreateRecordAsync(string entityLogicalName, Dictionary<string, object> attributes);
    Task UpdateRecordAsync(string entityLogicalName, Guid recordId, Dictionary<string, object> attributes);
    Task DeleteRecordAsync(string entityLogicalName, Guid recordId);
    Task<Entity?> GetRecordAsync(string entityLogicalName, Guid recordId, params string[] columns);
}

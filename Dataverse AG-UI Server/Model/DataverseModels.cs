namespace TestAgentFramework.Model;

public class TableMetadata
{
    public string LogicalName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public string? PrimaryIdAttribute { get; set; }
    public string? PrimaryNameAttribute { get; set; }
}

public class AttributeMetadata
{
    public string LogicalName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string AttributeType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsPrimaryId { get; set; }
    public bool IsPrimaryName { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public List<OptionMetadata>? OptionSet { get; set; }
}

public class OptionMetadata
{
    public int Value { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RelationshipMetadata
{
    public string SchemaName { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string ReferencedEntity { get; set; } = string.Empty;
    public string ReferencedAttribute { get; set; } = string.Empty;
    public string ReferencingEntity { get; set; } = string.Empty;
    public string ReferencingAttribute { get; set; } = string.Empty;
}

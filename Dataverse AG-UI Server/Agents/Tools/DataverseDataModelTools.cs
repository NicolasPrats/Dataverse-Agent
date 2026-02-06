using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.AI;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TestAgentFramework.Model;
using TestAgentFramework.Services;

namespace Dataverse_AG_UI_Server.Agents.Tools
{
    public class DataverseDataModelTools : DataverseToolsBase
    {
        public const string GetTables = "dataverse_get_tables";
        public const string GetTableMetadata = "dataverse_get_table_metadata";
        public const string GetAttributes = "dataverse_get_attributes";
        public const string GetRelationships = "dataverse_get_relationships";
        public const string GetGlobalOptionSets = "dataverse_get_global_optionsets";
        public const string GetGlobalOptionSetDetails = "dataverse_get_global_optionset_details";
        public const string CreateTable = "dataverse_create_table";
        public const string CreateAttribute = "dataverse_create_attribute";
        public const string CreateGlobalOptionSet = "dataverse_create_global_optionset";
        public const string UpdateOptionSet = "dataverse_update_optionset";
        public const string CreateOneToManyRelationship = "dataverse_create_onetomany_relationship";
        public const string CreateManyToManyRelationship = "dataverse_create_manytomany_relationship";

        public DataverseDataModelTools(DataverseServiceClientFactory serviceClientFactory)
            : base(serviceClientFactory)
        {
        }

        // Individual tool properties - allows selective tool usage
        public AIFunction GetTablesToolAsync => 
            AIFunctionFactory.Create(GetTablesAsync, GetTables);

        public AIFunction GetTableMetadataToolAsync => 
            AIFunctionFactory.Create(GetTableMetadataAsync, GetTableMetadata);

        public AIFunction GetAttributesToolAsync => 
            AIFunctionFactory.Create(GetAttributesAsync, GetAttributes);

        public AIFunction GetRelationshipsToolAsync => 
            AIFunctionFactory.Create(GetRelationshipsAsync, GetRelationships);

        public AIFunction GetGlobalOptionSetsToolAsync => 
            AIFunctionFactory.Create(GetGlobalOptionSetsAsync, GetGlobalOptionSets);

        public AIFunction GetGlobalOptionSetDetailsToolAsync => 
            AIFunctionFactory.Create(GetGlobalOptionSetDetailsAsync, GetGlobalOptionSetDetails);

        public AIFunction CreateTableToolAsync =>
             AIFunctionFactory.Create(CreateTableAsync, CreateTable);

        public AIFunction CreateAttributeToolAsync => 
             AIFunctionFactory.Create(CreateAttributeAsync, CreateAttribute);

        public AIFunction CreateGlobalOptionSetToolAsync => 
             AIFunctionFactory.Create(CreateGlobalOptionSetAsync, CreateGlobalOptionSet);

        public AIFunction UpdateOptionSetToolAsync =>
             AIFunctionFactory.Create(UpdateOptionSetAsync, UpdateOptionSet);

        public AIFunction CreateOneToManyRelationshipToolAsync =>
             AIFunctionFactory.Create(CreateOneToManyRelationshipAsync, CreateOneToManyRelationship);

        public AIFunction CreateManyToManyRelationshipToolAsync =>
            AIFunctionFactory.Create(CreateManyToManyRelationshipAsync, CreateManyToManyRelationship);

        // Grouped tools for easy access
        /// <summary>
        /// All read-only tools for querying data model information
        /// </summary>
        public AIFunction[] ReadOnlyTools => 
        [
            GetTablesToolAsync,
            GetTableMetadataToolAsync,
            GetAttributesToolAsync,
            GetRelationshipsToolAsync,
            GetGlobalOptionSetsToolAsync,
            GetGlobalOptionSetDetailsToolAsync
        ];

        /// <summary>
        /// All write tools for creating/modifying data model
        /// </summary>
        public AIFunction[] WriteTools => 
        [
            CreateTableToolAsync,
            CreateAttributeToolAsync,
            CreateGlobalOptionSetToolAsync,
            UpdateOptionSetToolAsync,
            CreateOneToManyRelationshipToolAsync,
            CreateManyToManyRelationshipToolAsync
        ];

        /// <summary>
        /// All tools (read + write)
        /// </summary>
        public AIFunction[] AllTools => 
        [
            .. ReadOnlyTools,
            .. WriteTools
        ];

        [Description("Retrieves all tables (entities) from the Dataverse environment.")]
        public async Task<object> GetTablesAsync()
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = false
                    };

                    var response = (RetrieveAllEntitiesResponse)ServiceClient.Execute(request);

                    var tables = response.EntityMetadata
                        .Where(e => e.IsCustomizable?.Value == true || e.IsManaged == false)
                        .Select(e => new TableMetadata
                        {
                            LogicalName = e.LogicalName,
                            SchemaName = e.SchemaName,
                            Description = e.Description?.UserLocalizedLabel?.Label,
                            DisplayName = e.DisplayName?.UserLocalizedLabel?.Label,
                            PrimaryIdAttribute = e.PrimaryIdAttribute,
                            PrimaryNameAttribute = e.PrimaryNameAttribute
                        })
                        .OrderBy(t => t.LogicalName)
                        .ToList();

                    return new
                    {
                        Success = true,
                        Data = tables,
                        Count = tables.Count
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message
                    };
                }
            });
        }

        [Description("Gets detailed metadata for a specific table including all its attributes and relationships.")]
        public async Task<object> GetTableMetadataAsync(
            [Description("The logical name of the table")] string logicalName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = logicalName,
                        EntityFilters = EntityFilters.All,
                        RetrieveAsIfPublished = false
                    };

                    var response = (RetrieveEntityResponse)ServiceClient.Execute(request);
                    var entity = response.EntityMetadata;

                    return new
                    {
                        Success = true,
                        LogicalName = entity.LogicalName,
                        SchemaName = entity.SchemaName,
                        DisplayName = entity.DisplayName?.UserLocalizedLabel?.Label,
                        Description = entity.Description?.UserLocalizedLabel?.Label,
                        PrimaryIdAttribute = entity.PrimaryIdAttribute,
                        PrimaryNameAttribute = entity.PrimaryNameAttribute,
                        Attributes = entity.Attributes.Select(a => new
                        {
                            LogicalName = a.LogicalName,
                            SchemaName = a.SchemaName,
                            DisplayName = a.DisplayName?.UserLocalizedLabel?.Label,
                            AttributeType = a.AttributeType?.ToString(),
                            IsRequired = a.RequiredLevel?.Value.ToString()
                        }).ToList()
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        TableName = logicalName
                    };
                }
            });
        }

        [Description("Gets all attributes (columns) for a specific table.")]
        public async Task<object> GetAttributesAsync(
            [Description("The logical name of the table")] string entityLogicalName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityLogicalName,
                        EntityFilters = EntityFilters.Attributes,
                        RetrieveAsIfPublished = false
                    };

                    var response = (RetrieveEntityResponse)ServiceClient.Execute(request);

                    var attributes = response.EntityMetadata.Attributes.Select(a =>
                    {
                        var attr = new TestAgentFramework.Model.AttributeMetadata
                        {
                            LogicalName = a.LogicalName,
                            SchemaName = a.SchemaName,
                            DisplayName = a.DisplayName?.UserLocalizedLabel?.Label,
                            Description = a.Description?.UserLocalizedLabel?.Label,
                            AttributeType = a.AttributeType?.ToString() ?? "Unknown",
                            IsRequired = a.RequiredLevel?.Value == Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired,
                            IsPrimaryId = a.IsPrimaryId ?? false,
                            IsPrimaryName = a.IsPrimaryName ?? false
                        };

                        if (a is StringAttributeMetadata stringAttr)
                        {
                            attr.MaxLength = stringAttr.MaxLength;
                        }
                        else if (a is IntegerAttributeMetadata intAttr)
                        {
                            attr.MinValue = intAttr.MinValue;
                            attr.MaxValue = intAttr.MaxValue;
                        }
                        else if (a is DecimalAttributeMetadata decimalAttr)
                        {
                            attr.MinValue = decimalAttr.MinValue;
                            attr.MaxValue = (decimal?)decimalAttr.MaxValue;
                        }
                        else if (a is PicklistAttributeMetadata picklistAttr && picklistAttr.OptionSet != null)
                        {
                            attr.OptionSet = picklistAttr.OptionSet.Options.Select(o => new TestAgentFramework.Model.OptionMetadata
                            {
                                Value = o.Value ?? 0,
                                Label = o.Label?.UserLocalizedLabel?.Label ?? string.Empty,
                                Description = o.Description?.UserLocalizedLabel?.Label
                            }).ToList();
                        }

                        return attr;
                    }).ToList();

                    return new
                    {
                        Success = true,
                        Data = attributes,
                        Count = attributes.Count,
                        TableName = entityLogicalName
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        TableName = entityLogicalName
                    };
                }
            });
        }

        [Description("Gets all relationships for a specific table.")]
        public async Task<object> GetRelationshipsAsync(
            [Description("The logical name of the table")] string entityLogicalName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityLogicalName,
                        EntityFilters = EntityFilters.Relationships,
                        RetrieveAsIfPublished = false
                    };

                    var response = (RetrieveEntityResponse)ServiceClient.Execute(request);
                    var relationships = new List<RelationshipMetadata>();

                    // One-to-Many relationships
                    foreach (var rel in response.EntityMetadata.OneToManyRelationships)
                    {
                        relationships.Add(new RelationshipMetadata
                        {
                            SchemaName = rel.SchemaName,
                            RelationshipType = "OneToMany",
                            ReferencedEntity = rel.ReferencedEntity,
                            ReferencedAttribute = rel.ReferencedAttribute,
                            ReferencingEntity = rel.ReferencingEntity,
                            ReferencingAttribute = rel.ReferencingAttribute
                        });
                    }

                    // Many-to-One relationships
                    foreach (var rel in response.EntityMetadata.ManyToOneRelationships)
                    {
                        relationships.Add(new RelationshipMetadata
                        {
                            SchemaName = rel.SchemaName,
                            RelationshipType = "ManyToOne",
                            ReferencedEntity = rel.ReferencedEntity,
                            ReferencedAttribute = rel.ReferencedAttribute,
                            ReferencingEntity = rel.ReferencingEntity,
                            ReferencingAttribute = rel.ReferencingAttribute
                        });
                    }

                    // Many-to-Many relationships
                    foreach (var rel in response.EntityMetadata.ManyToManyRelationships)
                    {
                        relationships.Add(new RelationshipMetadata
                        {
                            SchemaName = rel.SchemaName,
                            RelationshipType = "ManyToMany",
                            ReferencedEntity = rel.Entity1LogicalName,
                            ReferencedAttribute = rel.Entity1IntersectAttribute,
                            ReferencingEntity = rel.Entity2LogicalName,
                            ReferencingAttribute = rel.Entity2IntersectAttribute
                        });
                    }

                    return new
                    {
                        Success = true,
                        Data = relationships,
                        Count = relationships.Count,
                        TableName = entityLogicalName
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        TableName = entityLogicalName
                    };
                }
            });
        }

        [Description("Retrieves all global option sets (choices) from the Dataverse environment.")]
        public async Task<object> GetGlobalOptionSetsAsync()
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveAllOptionSetsRequest();
                    var response = (RetrieveAllOptionSetsResponse)ServiceClient.Execute(request);

                    var globalOptionSets = response.OptionSetMetadata
                        .Where(o => o.IsGlobal == true && o is OptionSetMetadata)
                        .Cast<OptionSetMetadata>()
                        .Select(o => new
                        {
                            Name = o.Name,
                            DisplayName = o.DisplayName?.UserLocalizedLabel?.Label,
                            Description = o.Description?.UserLocalizedLabel?.Label,
                            IsCustomizable = o.IsCustomizable?.Value ?? false,
                            OptionCount = o.Options?.Count ?? 0
                        })
                        .OrderBy(o => o.Name)
                        .ToList();

                    return new
                    {
                        Success = true,
                        Data = globalOptionSets,
                        Count = globalOptionSets.Count
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message
                    };
                }
            });
        }

        [Description("Gets detailed information about a specific global option set including all its values.")]
        public async Task<object> GetGlobalOptionSetDetailsAsync(
            [Description("The logical name of the global option set")] string optionSetName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var request = new RetrieveOptionSetRequest
                    {
                        Name = optionSetName
                    };

                    var response = (RetrieveOptionSetResponse)ServiceClient.Execute(request);
                    
                    if (response.OptionSetMetadata is not OptionSetMetadata optionSet)
                    {
                        return new
                        {
                            Success = false,
                            Error = "Option set not found or is not a standard option set",
                            OptionSetName = optionSetName
                        };
                    }

                    var options = optionSet.Options.Select(o => new
                    {
                        Value = o.Value ?? 0,
                        Label = o.Label?.UserLocalizedLabel?.Label ?? string.Empty,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        ExternalValue = o.ExternalValue
                    }).ToList();

                    return new
                    {
                        Success = true,
                        Name = optionSet.Name,
                        DisplayName = optionSet.DisplayName?.UserLocalizedLabel?.Label,
                        Description = optionSet.Description?.UserLocalizedLabel?.Label,
                        IsGlobal = optionSet.IsGlobal,
                        IsCustomizable = optionSet.IsCustomizable?.Value ?? false,
                        Options = options,
                        OptionCount = options.Count
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        OptionSetName = optionSetName
                    };
                }
            });
        }

        [Description("Creates a new custom table in Dataverse.")]
        public async Task<object> CreateTableAsync(
            [Description("The logical name (lowercase with underscores, e.g., 'agent_myproduct')")] string logicalName,
            [Description("The schema name (PascalCase, e.g., 'agent_MyProduct')")] string schemaName,
            [Description("The display name (user-friendly, e.g., 'My Product')")] string displayName,
            [Description("Optional description of the table")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var createRequest = new CreateEntityRequest
                    {
                        Entity = new EntityMetadata
                        {
                            SchemaName = schemaName,
                            LogicalName = logicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            DisplayCollectionName = new Microsoft.Xrm.Sdk.Label(displayName + "s", BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            OwnershipType = OwnershipTypes.UserOwned,
                            IsActivity = false,
                            HasActivities = true,
                            HasNotes = true
                        },
                        PrimaryAttribute = new StringAttributeMetadata
                        {
                            SchemaName = schemaName + "Name",
                            LogicalName = logicalName + "name",
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            MaxLength = 100,
                            DisplayName = new Microsoft.Xrm.Sdk.Label("Name", BaseLcid),
                            Description = new Microsoft.Xrm.Sdk.Label("The primary name attribute", BaseLcid)
                        },
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the entity
                    EnsureSolutionExists();

                    var response = (CreateEntityResponse)ServiceClient.Execute(createRequest);

                    // Add entity to solution
                    AddComponentToSolution(response.EntityId, SolutionComponentType.Entity);

                    return new
                    {
                        Success = true,
                        EntityId = response.EntityId,
                        Message = $"Table '{displayName}' created successfully with logical name '{logicalName}' in solution '{SolutionDisplayName}'"
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        LogicalName = logicalName,
                        SchemaName = schemaName,
                        DisplayName = displayName
                    };
                }
            });
        }

        [Description("Creates a new attribute (column) in a table. Supported types: string, integer, decimal, boolean, datetime, money, picklist.")]
        public async Task<object> CreateAttributeAsync(
            [Description("The logical name of the table")] string entityLogicalName,
            [Description("The logical name of the attribute (lowercase with underscores)")] string attributeLogicalName,
            [Description("The schema name of the attribute")] string attributeSchemaName,
            [Description("Type of attribute: 'string', 'integer', 'decimal', 'boolean', 'datetime', 'money', or 'picklist'")] string attributeType,
            [Description("The display name of the attribute")] string displayName,
            [Description("Optional description")] string? description = null,
            [Description("Whether the attribute is required")] bool isRequired = false,
            [Description("Maximum length (for string attributes only)")] int? maxLength = null,
            [Description("The logical name of the global option set (required for 'picklist' type)")] string? globalOptionSetName = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    Microsoft.Xrm.Sdk.Metadata.AttributeMetadata attribute = attributeType.ToLower() switch
                    {
                        "string" => new StringAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            MaxLength = maxLength ?? 100
                        },
                        "integer" => new IntegerAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            Format = IntegerFormat.None
                        },
                        "decimal" => new DecimalAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            Precision = 2
                        },
                        "boolean" => new BooleanAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            OptionSet = new BooleanOptionSetMetadata(
                                new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(new Microsoft.Xrm.Sdk.Label("Yes", BaseLcid), 1),
                                new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(new Microsoft.Xrm.Sdk.Label("No", BaseLcid), 0))
                        },
                        "datetime" => new DateTimeAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            Format = DateTimeFormat.DateAndTime
                        },
                        "money" => new MoneyAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            Precision = 2,
                            PrecisionSource = 2
                        },
                        "picklist" or "optionset" => string.IsNullOrWhiteSpace(globalOptionSetName)
                            ? throw new ArgumentException("globalOptionSetName is required when creating a picklist attribute")
                            : new PicklistAttributeMetadata
                            {
                                SchemaName = attributeSchemaName,
                                LogicalName = attributeLogicalName,
                                DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                                Description = string.IsNullOrWhiteSpace(description) 
                                    ? null 
                                    : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                                RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                    isRequired 
                                        ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                        : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                                OptionSet = new OptionSetMetadata
                                {
                                    IsGlobal = true,
                                    OptionSetType = OptionSetType.Picklist,
                                    Name = globalOptionSetName
                                }
                            },
                        _ => throw new ArgumentException($"Unsupported attribute type: {attributeType}")
                    };

                    var createRequest = new CreateAttributeRequest
                    {
                        EntityName = entityLogicalName,
                        Attribute = attribute,
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the attribute
                    EnsureSolutionExists();

                    var response = (CreateAttributeResponse)ServiceClient.Execute(createRequest);

                    // Add attribute to solution
                    AddComponentToSolution(response.AttributeId, SolutionComponentType.Attribute);

                    return new
                    {
                        Success = true,
                        AttributeId = response.AttributeId,
                        Message = $"Attribute '{displayName}' created successfully in table '{entityLogicalName}' in solution '{SolutionDisplayName}'"
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        EntityLogicalName = entityLogicalName,
                        AttributeLogicalName = attributeLogicalName,
                        AttributeType = attributeType,
                        DisplayName = displayName,
                        GlobalOptionSetName = globalOptionSetName
                    };
                }
            });
        }

        [Description("Creates a new global option set (choice) that can be reused across multiple tables.")]
        public async Task<object> CreateGlobalOptionSetAsync(
            [Description("The logical name of the option set")] string optionSetName,
            [Description("The display name of the option set")] string displayName,
            [Description("JSON string of option values (e.g., '{\"Active\":1,\"Inactive\":2,\"Pending\":3}')")] string optionsJson,
            [Description("Optional description")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var options = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(optionsJson);
                    if (options == null || !options.Any())
                    {
                        throw new ArgumentException("Options cannot be empty");
                    }

                    var optionSetMetadata = new OptionSetMetadata
                    {
                        Name = optionSetName,
                        DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, BaseLcid),
                        Description = string.IsNullOrWhiteSpace(description) 
                            ? null 
                            : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                        IsGlobal = true,
                        OptionSetType = OptionSetType.Picklist,
                        Options = { }
                    };

                    foreach (var option in options)
                    {
                        optionSetMetadata.Options.Add(new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(
                            new Microsoft.Xrm.Sdk.Label(option.Key, BaseLcid),
                            option.Value));
                    }

                    var request = new CreateOptionSetRequest
                    {
                        OptionSet = optionSetMetadata,
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the option set
                    EnsureSolutionExists();

                    var response = (CreateOptionSetResponse)ServiceClient.Execute(request);

                    // Add option set to solution
                    AddComponentToSolution(response.OptionSetId, SolutionComponentType.OptionSet);

                    return new
                    {
                        Success = true,
                        OptionSetId = response.OptionSetId,
                        Message = $"Global option set '{displayName}' created successfully with {options.Count} options in solution '{SolutionDisplayName}'"
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        OptionSetName = optionSetName,
                        DisplayName = displayName
                    };
                }
            });
        }

        [Description("Updates an existing option set by adding, removing, or modifying options.")]
        public async Task<object> UpdateOptionSetAsync(
            [Description("The logical name of the option set")] string optionSetName,
            [Description("JSON string of new options to add (e.g., '{\"NewOption\":10}')")] string? addOptionsJson = null,
            [Description("Comma-separated list of option values to remove (e.g., '1,2,3')")] string? removeOptionValues = null,
            [Description("JSON string of options to update (e.g., '{\"5\":\"UpdatedLabel\"}')")] string? updateOptionsJson = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var messages = new List<string>();

                    // Add new options
                    if (!string.IsNullOrWhiteSpace(addOptionsJson))
                    {
                        var optionsToAdd = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(addOptionsJson);
                        if (optionsToAdd != null)
                        {
                            foreach (var option in optionsToAdd)
                            {
                                var insertRequest = new InsertOptionValueRequest
                                {
                                    OptionSetName = optionSetName,
                                    Label = new Microsoft.Xrm.Sdk.Label(option.Key, BaseLcid),
                                    Value = option.Value
                                };
                                ServiceClient.Execute(insertRequest);
                                messages.Add($"Added option '{option.Key}' with value {option.Value}");
                            }
                        }
                    }

                    // Remove options
                    if (!string.IsNullOrWhiteSpace(removeOptionValues))
                    {
                        var valuesToRemove = removeOptionValues.Split(',').Select(v => int.Parse(v.Trim()));
                        foreach (var value in valuesToRemove)
                        {
                            var deleteRequest = new DeleteOptionValueRequest
                            {
                                OptionSetName = optionSetName,
                                Value = value
                            };
                            ServiceClient.Execute(deleteRequest);
                            messages.Add($"Removed option with value {value}");
                        }
                    }

                    // Update option labels
                    if (!string.IsNullOrWhiteSpace(updateOptionsJson))
                    {
                        var optionsToUpdate = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(updateOptionsJson);
                        if (optionsToUpdate != null)
                        {
                            foreach (var option in optionsToUpdate)
                            {
                                var updateRequest = new UpdateOptionValueRequest
                                {
                                    OptionSetName = optionSetName,
                                    Value = int.Parse(option.Key),
                                    Label = new Microsoft.Xrm.Sdk.Label(option.Value, BaseLcid)
                                };
                                ServiceClient.Execute(updateRequest);
                                messages.Add($"Updated option {option.Key} to '{option.Value}'");
                            }
                        }
                    }

                    return new
                    {
                        Success = true,
                        Message = $"Option set '{optionSetName}' updated successfully",
                        Changes = messages
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        OptionSetName = optionSetName
                    };
                }
            });
        }

        [Description("Creates a one-to-many (1:N) relationship between two tables. This creates a lookup field on the referencing (child) table.")]
        public async Task<object> CreateOneToManyRelationshipAsync(
            [Description("The logical name of the primary (parent/referenced) table")] string primaryTableLogicalName,
            [Description("The logical name of the related (child/referencing) table")] string relatedTableLogicalName,
            [Description("The schema name for the relationship (e.g., 'agent_account_product')")] string schemaName,
            [Description("The schema name for the lookup attribute (e.g., 'agent_AccountId')")] string lookupAttributeSchemaName,
            [Description("The logical name for the lookup attribute (e.g., 'agent_accountid')")] string lookupAttributeLogicalName,
            [Description("The display name for the lookup attribute (e.g., 'Account')")] string lookupDisplayName,
            [Description("Optional description for the lookup attribute")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var lookup = new LookupAttributeMetadata
                    {
                        SchemaName = lookupAttributeSchemaName,
                        LogicalName = lookupAttributeLogicalName,
                        DisplayName = new Microsoft.Xrm.Sdk.Label(lookupDisplayName, BaseLcid),
                        Description = string.IsNullOrWhiteSpace(description) 
                            ? null 
                            : new Microsoft.Xrm.Sdk.Label(description, BaseLcid),
                        RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                            Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None)
                    };

                    var relationship = new OneToManyRelationshipMetadata
                    {
                        SchemaName = schemaName,
                        ReferencedEntity = primaryTableLogicalName,
                        ReferencingEntity = relatedTableLogicalName,
                        AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                        {
                            Behavior = AssociatedMenuBehavior.UseCollectionName,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Microsoft.Xrm.Sdk.Label(lookupDisplayName, BaseLcid),
                            Order = 10000
                        },
                        CascadeConfiguration = new CascadeConfiguration
                        {
                            Assign = CascadeType.NoCascade,
                            Delete = CascadeType.RemoveLink,
                            Merge = CascadeType.NoCascade,
                            Reparent = CascadeType.NoCascade,
                            Share = CascadeType.NoCascade,
                            Unshare = CascadeType.NoCascade
                        }
                    };

                    var request = new CreateOneToManyRequest
                    {
                        Lookup = lookup,
                        OneToManyRelationship = relationship,
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the relationship
                    EnsureSolutionExists();

                    var response = (CreateOneToManyResponse)ServiceClient.Execute(request);

                    // Add relationship and attribute to solution
                    AddComponentToSolution(response.RelationshipId, SolutionComponentType.EntityRelationship);
                    AddComponentToSolution(response.AttributeId, SolutionComponentType.Attribute);

                    return new
                    {
                        Success = true,
                        RelationshipId = response.RelationshipId,
                        AttributeId = response.AttributeId,
                        Message = $"One-to-many relationship '{schemaName}' created successfully. Lookup field '{lookupDisplayName}' added to '{relatedTableLogicalName}' table in solution '{SolutionDisplayName}'"
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        PrimaryTable = primaryTableLogicalName,
                        RelatedTable = relatedTableLogicalName,
                        SchemaName = schemaName,
                        LookupLogicalName = lookupAttributeLogicalName
                    };
                }
            });
        }

        [Description("Creates a many-to-many (N:N) relationship between two tables. This creates an intersection table.")]
        public async Task<object> CreateManyToManyRelationshipAsync(
            [Description("The logical name of the first table")] string entity1LogicalName,
            [Description("The logical name of the second table")] string entity2LogicalName,
            [Description("The schema name for the relationship (e.g., 'agent_account_contact')")] string schemaName,
            [Description("The schema name for the intersection table (e.g., 'agent_account_contact')")] string intersectTableName,
            [Description("Optional description")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var relationship = new ManyToManyRelationshipMetadata
                    {
                        SchemaName = schemaName,
                        IntersectEntityName = intersectTableName,
                        Entity1LogicalName = entity1LogicalName,
                        Entity2LogicalName = entity2LogicalName,
                        Entity1AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                        {
                            Behavior = AssociatedMenuBehavior.UseLabel,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Microsoft.Xrm.Sdk.Label($"Related {entity2LogicalName}", BaseLcid),
                            Order = 10000
                        },
                        Entity2AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                        {
                            Behavior = AssociatedMenuBehavior.UseLabel,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Microsoft.Xrm.Sdk.Label($"Related {entity1LogicalName}", BaseLcid),
                            Order = 10000
                        }
                    };

                    var request = new CreateManyToManyRequest
                    {
                        ManyToManyRelationship = relationship,
                        IntersectEntitySchemaName = intersectTableName,
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the relationship
                    EnsureSolutionExists();

                    var response = (CreateManyToManyResponse)ServiceClient.Execute(request);

                    // Add relationship to solution
                    AddComponentToSolution(response.ManyToManyRelationshipId, SolutionComponentType.EntityRelationship);

                    return new
                    {
                        Success = true,
                        RelationshipId = response.ManyToManyRelationshipId,
                        Message = $"Many-to-many relationship '{schemaName}' created successfully between '{entity1LogicalName}' and '{entity2LogicalName}' with intersection table '{intersectTableName}' in solution '{SolutionDisplayName}'"
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = ex.Message,
                        ErrorType = ex.GetType().Name,
                        Details = ex.InnerException?.Message,
                        Entity1 = entity1LogicalName,
                        Entity2 = entity2LogicalName,
                        SchemaName = schemaName,
                        IntersectTableName = intersectTableName
                    };
                }
            });
        }


    }
}




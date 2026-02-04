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

namespace TestAgentFramework.Agents.Tools.Tools
{
    public class DataverseAgentTools
    {
        public const string GetTables = "dataverse_get_tables";
        public const string GetTableMetadata = "dataverse_get_table_metadata";
        public const string GetAttributes = "dataverse_get_attributes";
        public const string GetRelationships = "dataverse_get_relationships";
        public const string CreateTable = "dataverse_create_table";
        public const string CreateAttribute = "dataverse_create_attribute";
        public const string CreateGlobalOptionSet = "dataverse_create_global_optionset";
        public const string UpdateOptionSet = "dataverse_update_optionset";
        public const string CreateOneToManyRelationship = "dataverse_create_onetomany_relationship";
        public const string CreateManyToManyRelationship = "dataverse_create_manytomany_relationship";

        private const string SolutionUniqueName = "AgentCustomizations";
        private const string SolutionDisplayName = "Agent Customizations";
        private const string PublisherPrefix = "agent";

        private readonly ServiceClient _serviceClient;
        private Guid? _solutionId;

        public DataverseAgentTools(DataverseSettings configuration)
        {
            _serviceClient = new ServiceClient(configuration.ConnectionString);

            if (!_serviceClient.IsReady)
            {
                throw new InvalidOperationException($"Failed to connect to Dataverse: {_serviceClient.LastError}");
            }
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

        public AIFunction CreateTableToolAsync =>
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(CreateTableAsync, CreateTable));

        public AIFunction CreateAttributeToolAsync => 
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(CreateAttributeAsync, CreateAttribute));

        public AIFunction CreateGlobalOptionSetToolAsync => 
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(CreateGlobalOptionSetAsync, CreateGlobalOptionSet));

        public AIFunction UpdateOptionSetToolAsync =>
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(UpdateOptionSetAsync, UpdateOptionSet));

        public AIFunction CreateOneToManyRelationshipToolAsync =>
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(CreateOneToManyRelationshipAsync, CreateOneToManyRelationship));

        public AIFunction CreateManyToManyRelationshipToolAsync =>
           new ApprovalRequiredAIFunction(AIFunctionFactory.Create(CreateManyToManyRelationshipAsync, CreateManyToManyRelationship));


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

                    var response = (RetrieveAllEntitiesResponse)_serviceClient.Execute(request);

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

                    var response = (RetrieveEntityResponse)_serviceClient.Execute(request);
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

                    var response = (RetrieveEntityResponse)_serviceClient.Execute(request);

                    var attributes = response.EntityMetadata.Attributes.Select(a =>
                    {
                        var attr = new Model.AttributeMetadata
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
                            attr.OptionSet = picklistAttr.OptionSet.Options.Select(o => new Model.OptionMetadata
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

                    var response = (RetrieveEntityResponse)_serviceClient.Execute(request);
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            DisplayCollectionName = new Microsoft.Xrm.Sdk.Label(displayName + "s", 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label("Name", 1033),
                            Description = new Microsoft.Xrm.Sdk.Label("The primary name attribute", 1033)
                        },
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the entity
                    EnsureSolutionExists();

                    var response = (CreateEntityResponse)_serviceClient.Execute(createRequest);

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

        [Description("Creates a new attribute (column) in a table. Supported types: string, integer, decimal, boolean, datetime, money.")]
        public async Task<object> CreateAttributeAsync(
            [Description("The logical name of the table")] string entityLogicalName,
            [Description("The logical name of the attribute (lowercase with underscores)")] string attributeLogicalName,
            [Description("The schema name of the attribute")] string attributeSchemaName,
            [Description("Type of attribute: 'string', 'integer', 'decimal', 'boolean', 'datetime', or 'money'")] string attributeType,
            [Description("The display name of the attribute")] string displayName,
            [Description("Optional description")] string? description = null,
            [Description("Whether the attribute is required")] bool isRequired = false,
            [Description("Maximum length (for string attributes only)")] int? maxLength = null)
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            OptionSet = new BooleanOptionSetMetadata(
                                new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(new Microsoft.Xrm.Sdk.Label("Yes", 1033), 1),
                                new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(new Microsoft.Xrm.Sdk.Label("No", 1033), 0))
                        },
                        "datetime" => new DateTimeAttributeMetadata
                        {
                            SchemaName = attributeSchemaName,
                            LogicalName = attributeLogicalName,
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                            Description = string.IsNullOrWhiteSpace(description) 
                                ? null 
                                : new Microsoft.Xrm.Sdk.Label(description, 1033),
                            RequiredLevel = new Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty(
                                isRequired 
                                    ? Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.ApplicationRequired 
                                    : Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel.None),
                            Precision = 2,
                            PrecisionSource = 2
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

                    var response = (CreateAttributeResponse)_serviceClient.Execute(createRequest);

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
                        DisplayName = displayName
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
                        DisplayName = new Microsoft.Xrm.Sdk.Label(displayName, 1033),
                        Description = string.IsNullOrWhiteSpace(description) 
                            ? null 
                            : new Microsoft.Xrm.Sdk.Label(description, 1033),
                        IsGlobal = true,
                        OptionSetType = OptionSetType.Picklist,
                        Options = { }
                    };

                    foreach (var option in options)
                    {
                        optionSetMetadata.Options.Add(new Microsoft.Xrm.Sdk.Metadata.OptionMetadata(
                            new Microsoft.Xrm.Sdk.Label(option.Key, 1033),
                            option.Value));
                    }

                    var request = new CreateOptionSetRequest
                    {
                        OptionSet = optionSetMetadata,
                        SolutionUniqueName = SolutionUniqueName
                    };

                    // Ensure solution exists before creating the option set
                    EnsureSolutionExists();

                    var response = (CreateOptionSetResponse)_serviceClient.Execute(request);

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
                                    Label = new Microsoft.Xrm.Sdk.Label(option.Key, 1033),
                                    Value = option.Value
                                };
                                _serviceClient.Execute(insertRequest);
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
                            _serviceClient.Execute(deleteRequest);
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
                                    Label = new Microsoft.Xrm.Sdk.Label(option.Value, 1033)
                                };
                                _serviceClient.Execute(updateRequest);
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
                        DisplayName = new Microsoft.Xrm.Sdk.Label(lookupDisplayName, 1033),
                        Description = string.IsNullOrWhiteSpace(description) 
                            ? null 
                            : new Microsoft.Xrm.Sdk.Label(description, 1033),
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
                            Label = new Microsoft.Xrm.Sdk.Label(lookupDisplayName, 1033),
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

                    var response = (CreateOneToManyResponse)_serviceClient.Execute(request);

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
                            Label = new Microsoft.Xrm.Sdk.Label($"Related {entity2LogicalName}", 1033),
                            Order = 10000
                        },
                        Entity2AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                        {
                            Behavior = AssociatedMenuBehavior.UseLabel,
                            Group = AssociatedMenuGroup.Details,
                            Label = new Microsoft.Xrm.Sdk.Label($"Related {entity1LogicalName}", 1033),
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

                    var response = (CreateManyToManyResponse)_serviceClient.Execute(request);

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

        private Guid EnsureSolutionExists()
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

            var results = _serviceClient.RetrieveMultiple(query);
            
            if (results.Entities.Count > 0)
            {
                _solutionId = results.Entities[0].Id;
                return _solutionId.Value;
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

            _solutionId = _serviceClient.Create(solution);
            return _solutionId.Value;
        }

        private Guid EnsurePublisherExists()
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

            var results = _serviceClient.RetrieveMultiple(query);
            
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

            return _serviceClient.Create(publisher);
        }

        private void AddComponentToSolution(Guid componentId, SolutionComponentType componentType)
        {
            var solutionId = EnsureSolutionExists();

            var request = new AddSolutionComponentRequest
            {
                ComponentId = componentId,
                ComponentType = (int)componentType,
                SolutionUniqueName = SolutionUniqueName,
                AddRequiredComponents = true
            };

            _serviceClient.Execute(request);
        }


    }
}

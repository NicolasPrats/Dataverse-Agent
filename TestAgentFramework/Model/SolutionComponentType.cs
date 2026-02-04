namespace TestAgentFramework.Model;

/// <summary>
/// Represents the types of components that can be added to a Dataverse solution.
/// These values correspond to the ComponentType enumeration in Dynamics 365.
/// </summary>
public enum SolutionComponentType
{
    /// <summary>
    /// Entity (Table)
    /// </summary>
    Entity = 1,

    /// <summary>
    /// Attribute (Column)
    /// </summary>
    Attribute = 2,

    /// <summary>
    /// Relationship
    /// </summary>
    Relationship = 3,

    /// <summary>
    /// Attribute Picklist Value
    /// </summary>
    AttributePicklistValue = 4,

    /// <summary>
    /// Attribute Lookup Value
    /// </summary>
    AttributeLookupValue = 5,

    /// <summary>
    /// View Attribute
    /// </summary>
    ViewAttribute = 6,

    /// <summary>
    /// Localized Label
    /// </summary>
    LocalizedLabel = 7,

    /// <summary>
    /// Relationship Extra Condition
    /// </summary>
    RelationshipExtraCondition = 8,

    /// <summary>
    /// Option Set (Global Choice)
    /// </summary>
    OptionSet = 9,

    /// <summary>
    /// Entity Relationship
    /// </summary>
    EntityRelationship = 10,

    /// <summary>
    /// Entity Relationship Role
    /// </summary>
    EntityRelationshipRole = 11,

    /// <summary>
    /// Entity Relationship Relationships
    /// </summary>
    EntityRelationshipRelationships = 12,

    /// <summary>
    /// Managed Property
    /// </summary>
    ManagedProperty = 13,

    /// <summary>
    /// Entity Key
    /// </summary>
    EntityKey = 14,

    /// <summary>
    /// Role
    /// </summary>
    Role = 20,

    /// <summary>
    /// Role Privilege
    /// </summary>
    RolePrivilege = 21,

    /// <summary>
    /// Display String
    /// </summary>
    DisplayString = 22,

    /// <summary>
    /// Display String Map
    /// </summary>
    DisplayStringMap = 23,

    /// <summary>
    /// Form
    /// </summary>
    Form = 24,

    /// <summary>
    /// Organization
    /// </summary>
    Organization = 25,

    /// <summary>
    /// Saved Query (View)
    /// </summary>
    SavedQuery = 26,

    /// <summary>
    /// Workflow (Process)
    /// </summary>
    Workflow = 29,

    /// <summary>
    /// Report
    /// </summary>
    Report = 31,

    /// <summary>
    /// Report Entity
    /// </summary>
    ReportEntity = 32,

    /// <summary>
    /// Report Category
    /// </summary>
    ReportCategory = 33,

    /// <summary>
    /// Report Visibility
    /// </summary>
    ReportVisibility = 34,

    /// <summary>
    /// Attachment
    /// </summary>
    Attachment = 35,

    /// <summary>
    /// Email Template
    /// </summary>
    EmailTemplate = 36,

    /// <summary>
    /// Contract Template
    /// </summary>
    ContractTemplate = 37,

    /// <summary>
    /// KB Article Template
    /// </summary>
    KBArticleTemplate = 38,

    /// <summary>
    /// Mail Merge Template
    /// </summary>
    MailMergeTemplate = 39,

    /// <summary>
    /// Duplicate Rule
    /// </summary>
    DuplicateRule = 44,

    /// <summary>
    /// Duplicate Rule Condition
    /// </summary>
    DuplicateRuleCondition = 45,

    /// <summary>
    /// Entity Map
    /// </summary>
    EntityMap = 46,

    /// <summary>
    /// Attribute Map
    /// </summary>
    AttributeMap = 47,

    /// <summary>
    /// Ribbon Command
    /// </summary>
    RibbonCommand = 48,

    /// <summary>
    /// Ribbon Context Group
    /// </summary>
    RibbonContextGroup = 49,

    /// <summary>
    /// Ribbon Customization
    /// </summary>
    RibbonCustomization = 50,

    /// <summary>
    /// Ribbon Rule
    /// </summary>
    RibbonRule = 52,

    /// <summary>
    /// Ribbon Tab To Command Map
    /// </summary>
    RibbonTabToCommandMap = 53,

    /// <summary>
    /// Ribbon Diff
    /// </summary>
    RibbonDiff = 55,

    /// <summary>
    /// Saved Query Visualization (Chart)
    /// </summary>
    SavedQueryVisualization = 59,

    /// <summary>
    /// System Form
    /// </summary>
    SystemForm = 60,

    /// <summary>
    /// Web Resource
    /// </summary>
    WebResource = 61,

    /// <summary>
    /// Site Map
    /// </summary>
    SiteMap = 62,

    /// <summary>
    /// Connection Role
    /// </summary>
    ConnectionRole = 63,

    /// <summary>
    /// Complex Control
    /// </summary>
    ComplexControl = 64,

    /// <summary>
    /// Hierarchy Rule
    /// </summary>
    HierarchyRule = 65,

    /// <summary>
    /// Custom Control
    /// </summary>
    CustomControl = 66,

    /// <summary>
    /// Custom Control Default Config
    /// </summary>
    CustomControlDefaultConfig = 68,

    /// <summary>
    /// Field Security Profile
    /// </summary>
    FieldSecurityProfile = 70,

    /// <summary>
    /// Field Permission
    /// </summary>
    FieldPermission = 71,

    /// <summary>
    /// Model-Driven App
    /// </summary>
    ModelDrivenApp = 80,

    /// <summary>
    /// Plugin Type
    /// </summary>
    PluginType = 90,

    /// <summary>
    /// Plugin Assembly
    /// </summary>
    PluginAssembly = 91,

    /// <summary>
    /// SDK Message Processing Step
    /// </summary>
    SDKMessageProcessingStep = 92,

    /// <summary>
    /// SDK Message Processing Step Image
    /// </summary>
    SDKMessageProcessingStepImage = 93,

    /// <summary>
    /// Service Endpoint
    /// </summary>
    ServiceEndpoint = 95,

    /// <summary>
    /// Routing Rule
    /// </summary>
    RoutingRule = 150,

    /// <summary>
    /// Routing Rule Item
    /// </summary>
    RoutingRuleItem = 151,

    /// <summary>
    /// SLA
    /// </summary>
    SLA = 152,

    /// <summary>
    /// SLA Item
    /// </summary>
    SLAItem = 153,

    /// <summary>
    /// Convert Rule
    /// </summary>
    ConvertRule = 154,

    /// <summary>
    /// Convert Rule Item
    /// </summary>
    ConvertRuleItem = 155,

    /// <summary>
    /// Mobile Offline Profile
    /// </summary>
    MobileOfflineProfile = 161,

    /// <summary>
    /// Mobile Offline Profile Item
    /// </summary>
    MobileOfflineProfileItem = 162,

    /// <summary>
    /// Similarity Rule
    /// </summary>
    SimilarityRule = 165,

    /// <summary>
    /// Data Source Mapping
    /// </summary>
    DataSourceMapping = 166,

    /// <summary>
    /// SDKMessage
    /// </summary>
    SDKMessage = 201,

    /// <summary>
    /// SDKMessageFilter
    /// </summary>
    SDKMessageFilter = 202,

    /// <summary>
    /// SdkMessagePair
    /// </summary>
    SdkMessagePair = 203,

    /// <summary>
    /// SdkMessageRequest
    /// </summary>
    SdkMessageRequest = 204,

    /// <summary>
    /// SdkMessageRequestField
    /// </summary>
    SdkMessageRequestField = 205,

    /// <summary>
    /// SdkMessageResponse
    /// </summary>
    SdkMessageResponse = 206,

    /// <summary>
    /// SdkMessageResponseField
    /// </summary>
    SdkMessageResponseField = 207,

    /// <summary>
    /// Index
    /// </summary>
    Index = 208,

    /// <summary>
    /// Import Map
    /// </summary>
    ImportMap = 210,

    /// <summary>
    /// Canvas App
    /// </summary>
    CanvasApp = 300,

    /// <summary>
    /// Connector
    /// </summary>
    Connector = 371,

    /// <summary>
    /// Connector
    /// </summary>
    Connector2 = 372,

    /// <summary>
    /// Environment Variable Definition
    /// </summary>
    EnvironmentVariableDefinition = 380,

    /// <summary>
    /// Environment Variable Value
    /// </summary>
    EnvironmentVariableValue = 381,

    /// <summary>
    /// AI Project Type
    /// </summary>
    AIProjectType = 400,

    /// <summary>
    /// AI Project
    /// </summary>
    AIProject = 401,

    /// <summary>
    /// AI Configuration
    /// </summary>
    AIConfiguration = 402,

    /// <summary>
    /// Entity Analytics Config
    /// </summary>
    EntityAnalyticsConfig = 430,

    /// <summary>
    /// Attribute Image Config
    /// </summary>
    AttributeImageConfig = 431,

    /// <summary>
    /// Entity Image Config
    /// </summary>
    EntityImageConfig = 432
}

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
using Dataverse_AG_UI_Server.Model;
using Dataverse_AG_UI_Server.Services;
using Dataverse_AG_UI_Server.Agents.Tools.Tools;
using Dataverse_AG_UI_Server.Agents.Base;

namespace Dataverse_AG_UI_Server.Agents.Tools
{
    public class DataverseUITools : DataverseToolsBase
    {
        public const string GetViews = "dataverse_get_views";
        public const string CreateView = "dataverse_create_view";
        public const string UpdateView = "dataverse_update_view";
        public const string GetForms = "dataverse_get_forms";
        public const string CreateForm = "dataverse_create_form";
        public const string UpdateForm = "dataverse_update_form";
        public const string ValidateFormXml = "dataverse_validate_formxml";
        public const string ValidateFetchXml = "dataverse_validate_fetchxml";
        public const string ValidateLayoutXml = "dataverse_validate_layoutxml";

        private readonly DataverseXmlValidator _xmlValidator = new();

        public DataverseUITools(DataverseServiceClientFactory serviceClientFactory)
            : base(serviceClientFactory)
        {
        }

        // Individual tool properties - allows selective tool usage
        public Tool GetViewsToolAsync =>
            new(GetViews, GetViewsAsync);

        public Tool CreateViewToolAsync =>
            new(CreateView, CreateViewAsync);

        public Tool UpdateViewToolAsync =>
            new(UpdateView, UpdateViewAsync);

        public Tool GetFormsToolAsync =>
            new(GetForms, GetFormsAsync);

        public Tool CreateFormToolAsync =>
            new(CreateForm, CreateFormAsync);

        public Tool UpdateFormToolAsync =>
            new(UpdateForm, UpdateFormAsync);

        public Tool ValidateFormXmlToolAsync =>
            new(ValidateFormXml, ValidateFormXmlAsync);

        public Tool ValidateFetchXmlToolAsync =>
            new(ValidateFetchXml, ValidateFetchXmlAsync);

        public Tool ValidateLayoutXmlToolAsync =>
            new(ValidateLayoutXml, ValidateLayoutXmlAsync);

        // Grouped tools for easy access
        /// <summary>
        /// All read-only tools for querying UI components and validating XML
        /// </summary>
        public Tool[] ReadOnlyTools =>
        [
            GetViewsToolAsync,
            GetFormsToolAsync
        ];

        /// <summary>
        /// All write tools for creating/modifying UI components
        /// </summary>
        public Tool[] WriteTools =>
        [
            CreateViewToolAsync,
            UpdateViewToolAsync,
            CreateFormToolAsync,
            UpdateFormToolAsync,
            ValidateFormXmlToolAsync,
            ValidateFetchXmlToolAsync,
            ValidateLayoutXmlToolAsync
        ];

        /// <summary>
        /// All tools (read + write + validation)
        /// </summary>
        public Tool[] AllTools =>
        [
            .. ReadOnlyTools,
            .. WriteTools
        ];

        #region Views (SavedQuery)

        [Description("Retrieves all views for a specific table.")]
        public async Task<object> GetViewsAsync(
            [Description("The logical name of the table")] string entityLogicalName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var query = new QueryExpression("savedquery")
                    {
                        ColumnSet = new ColumnSet(
                            "savedqueryid",
                            "name",
                            "returnedtypecode",
                            "querytype",
                            "isdefault",
                            "iscustomizable",
                            "description",
                            "fetchxml",
                            "layoutxml"
                        ),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("returnedtypecode", ConditionOperator.Equal, entityLogicalName)
                            }
                        }
                    };

                    var results = ServiceClient.RetrieveMultiple(query);

                    var views = results.Entities.Select(e => new
                    {
                        ViewId = e.Id,
                        Name = e.GetAttributeValue<string>("name"),
                        EntityName = e.GetAttributeValue<string>("returnedtypecode"),
                        QueryType = e.GetAttributeValue<int>("querytype"),
                        IsDefault = e.GetAttributeValue<bool>("isdefault"),
                        IsCustomizable = e.GetAttributeValue<BooleanManagedProperty>("iscustomizable")?.Value ?? false,
                        Description = e.GetAttributeValue<string>("description"),
                        FetchXml = e.GetAttributeValue<string>("fetchxml"),
                        LayoutXml = e.GetAttributeValue<string>("layoutxml")
                    }).ToList();

                    return new
                    {
                        Success = true,
                        Data = views,
                        Count = views.Count,
                        EntityName = entityLogicalName
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
                        EntityName = entityLogicalName
                    };
                }
            });
        }

        [Description("Creates a new view for a table.")]
        public async Task<object> CreateViewAsync(
            [Description("The logical name of the table")] string entityLogicalName,
            [Description("The name of the view")] string viewName,
            [Description("The FetchXML query for the view")] string fetchXml,
            [Description("The LayoutXML for the view columns")] string layoutXml,
            [Description("The view type (0=Public, 1=Advanced Find, 2=Associated, 4=Quick Find, 8=Lookup)")] int queryType = 0,
            [Description("Optional description")] string? description = null,
            [Description("Set as default view")] bool isDefault = false)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var view = new Entity("savedquery")
                    {
                        ["name"] = viewName,
                        ["returnedtypecode"] = entityLogicalName,
                        ["querytype"] = queryType,
                        ["fetchxml"] = fetchXml,
                        ["layoutxml"] = layoutXml,
                        ["isdefault"] = isDefault
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        view["description"] = description;
                    }

                    var viewId = ServiceClient.Create(view);

                    // Add to solution
                    EnsureSolutionExists(createIfNotExists: false);
                    AddComponentToSolution(viewId, SolutionComponentType.SavedQuery);

                    return new
                    {
                        Success = true,
                        ViewId = viewId,
                        Message = $"View '{viewName}' created successfully for table '{entityLogicalName}' in solution '{SolutionDisplayName}'"
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
                        EntityName = entityLogicalName,
                        ViewName = viewName
                    };
                }
            });
        }

        [Description("Updates an existing view.")]
        public async Task<object> UpdateViewAsync(
            [Description("The GUID of the view to update")] Guid viewId,
            [Description("The new name (optional)")] string? viewName = null,
            [Description("The new FetchXML query (optional)")] string? fetchXml = null,
            [Description("The new LayoutXML (optional)")] string? layoutXml = null,
            [Description("The new description (optional)")] string? description = null,
            [Description("Set as default view (optional)")] bool? isDefault = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var view = new Entity("savedquery", viewId);
                    var updates = new List<string>();

                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        view["name"] = viewName;
                        updates.Add("name");
                    }

                    if (!string.IsNullOrWhiteSpace(fetchXml))
                    {
                        view["fetchxml"] = fetchXml;
                        updates.Add("fetchxml");
                    }

                    if (!string.IsNullOrWhiteSpace(layoutXml))
                    {
                        view["layoutxml"] = layoutXml;
                        updates.Add("layoutxml");
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        view["description"] = description;
                        updates.Add("description");
                    }

                    if (isDefault.HasValue)
                    {
                        view["isdefault"] = isDefault.Value;
                        updates.Add("isdefault");
                    }

                    if (updates.Count == 0)
                    {
                        return new
                        {
                            Success = false,
                            Error = "No updates provided",
                            ViewId = viewId
                        };
                    }

                    ServiceClient.Update(view);

                    return new
                    {
                        Success = true,
                        ViewId = viewId,
                        UpdatedFields = updates,
                        Message = $"View updated successfully. Modified fields: {string.Join(", ", updates)}"
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
                        ViewId = viewId
                    };
                }
            });
        }

        #endregion

        #region Forms (SystemForm)

        [Description("Retrieves all forms for a specific table.")]
        public async Task<object> GetFormsAsync(
            [Description("The logical name of the table")] string entityLogicalName)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var query = new QueryExpression("systemform")
                    {
                        ColumnSet = new ColumnSet(
                            "formid",
                            "name",
                            "objecttypecode",
                            "type",
                            "description",
                            "iscustomizable",
                            "formxml"
                        ),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("objecttypecode", ConditionOperator.Equal, entityLogicalName)
                            }
                        }
                    };

                    var results = ServiceClient.RetrieveMultiple(query);

                    var forms = results.Entities.Select(e => new
                    {
                        FormId = e.Id,
                        Name = e.GetAttributeValue<string>("name"),
                        EntityName = e.GetAttributeValue<string>("objecttypecode"),
                        FormType = e.GetAttributeValue<OptionSetValue>("type")?.Value,
                        FormTypeName = GetFormTypeName(e.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0),
                        Description = e.GetAttributeValue<string>("description"),
                        IsCustomizable = e.GetAttributeValue<BooleanManagedProperty>("iscustomizable")?.Value ?? false,
                        FormXml = e.GetAttributeValue<string>("formxml")
                    }).ToList();

                    return new
                    {
                        Success = true,
                        Data = forms,
                        Count = forms.Count,
                        EntityName = entityLogicalName
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
                        EntityName = entityLogicalName
                    };
                }
            });
        }

        [Description("Creates a new form for a table.")]
        public async Task<object> CreateFormAsync(
            [Description("The logical name of the table")] string entityLogicalName,
            [Description("The name of the form")] string formName,
            [Description("The FormXML definition")] string formXml,
            [Description("The form type (2=Main, 5=Mobile, 6=Quick View Form, 7=Quick Create, 8=Dialog, 9=Task Flow Form, 10=InteractionCentricDashboard, 11=Card, 12=Main - Interactive experience)")] int formType = 2,
            [Description("Optional description")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var form = new Entity("systemform")
                    {
                        ["name"] = formName,
                        ["objecttypecode"] = entityLogicalName,
                        ["type"] = new OptionSetValue(formType),
                        ["formxml"] = formXml
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        form["description"] = description;
                    }

                    var formId = ServiceClient.Create(form);

                    // Add to solution
                    EnsureSolutionExists(createIfNotExists: false);
                    AddComponentToSolution(formId, SolutionComponentType.SystemForm);

                    return new
                    {
                        Success = true,
                        FormId = formId,
                        Message = $"Form '{formName}' created successfully for table '{entityLogicalName}' in solution '{SolutionDisplayName}'"
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
                        EntityName = entityLogicalName,
                        FormName = formName
                    };
                }
            });
        }

        [Description("Updates an existing form.")]
        public async Task<object> UpdateFormAsync(
            [Description("The GUID of the form to update")] Guid formId,
            [Description("The new name (optional)")] string? formName = null,
            [Description("The new FormXML (optional)")] string? formXml = null,
            [Description("The new description (optional)")] string? description = null)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var form = new Entity("systemform", formId);
                    var updates = new List<string>();

                    if (!string.IsNullOrWhiteSpace(formName))
                    {
                        form["name"] = formName;
                        updates.Add("name");
                    }

                    if (!string.IsNullOrWhiteSpace(formXml))
                    {
                        form["formxml"] = formXml;
                        updates.Add("formxml");
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        form["description"] = description;
                        updates.Add("description");
                    }

                    if (updates.Count == 0)
                    {
                        return new
                        {
                            Success = false,
                            Error = "No updates provided",
                            FormId = formId
                        };
                    }

                    ServiceClient.Update(form);

                    return new
                    {
                        Success = true,
                        FormId = formId,
                        UpdatedFields = updates,
                        Message = $"Form updated successfully. Modified fields: {string.Join(", ", updates)}"
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
                        FormId = formId
                    };
                }
            });
        }

        #endregion

        #region Helper Methods

        private static string GetFormTypeName(int formType)
        {
            return formType switch
            {
                0 => "Dashboard",
                1 => "AppointmentBook",
                2 => "Main",
                3 => "MiniCampaignBO",
                4 => "Preview",
                5 => "Mobile - Express",
                6 => "Quick View Form",
                7 => "Quick Create",
                8 => "Dialog",
                9 => "Task Flow Form",
                10 => "InteractionCentricDashboard",
                11 => "Card",
                12 => "Main - Interactive experience",
                100 => "Other",
                101 => "MainBackup",
                102 => "AppointmentBookBackup",
                103 => "Power BI Dashboard",
                _ => "Unknown"
            };
        }

        #endregion

        #region XML Validation

        [Description("Validates FormXML against the Dataverse FormXml.xsd schema. Use this before creating or updating forms to ensure XML is valid.")]
        public async Task<object> ValidateFormXmlAsync(
            [Description("The FormXML string to validate")] string formXml)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var result = _xmlValidator.ValidateFormXml(formXml);

                    return new
                    {
                        Success = result.IsValid,
                        IsValid = result.IsValid,
                        Errors = result.Errors,
                        Warnings = result.Warnings,
                        Message = result.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = $"Validation failed: {ex.Message}",
                        Details = ex.ToString()
                    };
                }
            });
        }

        [Description("Validates FetchXML against the Dataverse Fetch.xsd schema. Use this before creating views to ensure the query XML is valid.")]
        public async Task<object> ValidateFetchXmlAsync(
            [Description("The FetchXML string to validate")] string fetchXml)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var result = _xmlValidator.ValidateFetchXml(fetchXml);

                    return new
                    {
                        Success = result.IsValid,
                        IsValid = result.IsValid,
                        Errors = result.Errors,
                        Warnings = result.Warnings,
                        Message = result.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = $"Validation failed: {ex.Message}",
                        Details = ex.ToString()
                    };
                }
            });
        }

        [Description("Validates LayoutXML against the Dataverse CustomizationsSolution.xsd schema. Use this before creating views to ensure the column layout XML is valid.")]
        public async Task<object> ValidateLayoutXmlAsync(
            [Description("The LayoutXML string to validate")] string layoutXml)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    var result = _xmlValidator.ValidateLayoutXml(layoutXml);

                    return new
                    {
                        Success = result.IsValid,
                        IsValid = result.IsValid,
                        Errors = result.Errors,
                        Warnings = result.Warnings,
                        Message = result.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Error = $"Validation failed: {ex.Message}",
                        Details = ex.ToString()
                    };
                }
            });
        }

        #endregion
    }
}






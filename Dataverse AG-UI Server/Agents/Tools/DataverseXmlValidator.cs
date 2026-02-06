using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Dataverse_AG_UI_Server.Agents.Tools.Tools;

/// <summary>
/// Validates Dataverse XML schemas (FormXML, FetchXML, LayoutXML)
/// </summary>
public class DataverseXmlValidator
{
    private const string SchemaNamespace = "Dataverse_AG_UI_Server.Agents.Tools.Schemas._9._0._0._2090";
    private readonly Dictionary<string, XmlSchemaSet> _schemaCache = new();
    private readonly object _cacheLock = new();

    public enum XmlType
    {
        FormXml,
        FetchXml,
        LayoutXml
    }

    /// <summary>
    /// Validates XML against the appropriate Dataverse schema
    /// </summary>
    public ValidationResult ValidateXml(string xml, XmlType xmlType)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = ["XML content is null or empty"]
            };
        }

        try
        {
            var schemaSet = GetSchemaSet(xmlType);
            var errors = new List<string>();
            var warnings = new List<string>();

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemaSet,
                ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema |
                                 XmlSchemaValidationFlags.ProcessSchemaLocation |
                                 XmlSchemaValidationFlags.ReportValidationWarnings
            };

            settings.ValidationEventHandler += (sender, args) =>
            {
                if (args.Severity == XmlSeverityType.Error)
                {
                    errors.Add($"Line {args.Exception?.LineNumber}, Position {args.Exception?.LinePosition}: {args.Message}");
                }
                else
                {
                    warnings.Add($"Line {args.Exception?.LineNumber}, Position {args.Exception?.LinePosition}: {args.Message}");
                }
            };

            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader, settings);

            while (xmlReader.Read()) { }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (XmlException ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"XML Parsing Error at Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}"]
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Validation Error: {ex.Message}"]
            };
        }
    }

    /// <summary>
    /// Validates FormXML structure
    /// </summary>
    public ValidationResult ValidateFormXml(string formXml)
    {
        return ValidateXml(formXml, XmlType.FormXml);
    }

    /// <summary>
    /// Validates FetchXML query
    /// </summary>
    public ValidationResult ValidateFetchXml(string fetchXml)
    {
        return ValidateXml(fetchXml, XmlType.FetchXml);
    }

    /// <summary>
    /// Validates LayoutXML for views
    /// </summary>
    public ValidationResult ValidateLayoutXml(string layoutXml)
    {
        return ValidateXml(layoutXml, XmlType.LayoutXml);
    }

    private XmlSchemaSet GetSchemaSet(XmlType xmlType)
    {
        var cacheKey = xmlType.ToString();

        lock (_cacheLock)
        {
            if (_schemaCache.TryGetValue(cacheKey, out var cachedSchema))
            {
                return cachedSchema;
            }

            var schemaSet = new XmlSchemaSet();
            var assembly = Assembly.GetExecutingAssembly();

            switch (xmlType)
            {
                case XmlType.FormXml:
                    LoadSchemaFromResource(schemaSet, assembly, "FormXml.xsd");
                    break;

                case XmlType.FetchXml:
                    LoadSchemaFromResource(schemaSet, assembly, "Fetch.xsd");
                    break;

                case XmlType.LayoutXml:
                    // LayoutXML is part of CustomizationsSolution.xsd
                    LoadSchemaFromResource(schemaSet, assembly, "CustomizationsSolution.xsd");
                    LoadSchemaFromResource(schemaSet, assembly, "FormXml.xsd"); // Included dependency
                    LoadSchemaFromResource(schemaSet, assembly, "Fetch.xsd"); // Included dependency
                    LoadSchemaFromResource(schemaSet, assembly, "isv.config.xsd"); // Included dependency
                    LoadSchemaFromResource(schemaSet, assembly, "SiteMapType.xsd"); // Included dependency
                    break;
            }

            schemaSet.Compile();
            _schemaCache[cacheKey] = schemaSet;
            return schemaSet;
        }
    }

    private void LoadSchemaFromResource(XmlSchemaSet schemaSet, Assembly assembly, string schemaFileName)
    {
        var resourceName = $"{SchemaNamespace}.{schemaFileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
        }

        using var reader = XmlReader.Create(stream);
        var schema = XmlSchema.Read(reader, (sender, args) =>
        {
            // Log schema loading warnings/errors if needed
        });

        if (schema != null)
        {
            schemaSet.Add(schema);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = [];
        public List<string> Warnings { get; set; } = [];

        public override string ToString()
        {
            if (IsValid && Warnings.Count == 0)
            {
                return "? XML is valid";
            }

            var result = new System.Text.StringBuilder();
            
            if (!IsValid)
            {
                result.AppendLine("? XML is invalid");
                result.AppendLine();
                result.AppendLine("Errors:");
                foreach (var error in Errors)
                {
                    result.AppendLine($"  • {error}");
                }
            }
            else
            {
                result.AppendLine("? XML is valid");
            }

            if (Warnings.Count > 0)
            {
                result.AppendLine();
                result.AppendLine("Warnings:");
                foreach (var warning in Warnings)
                {
                    result.AppendLine($"  ?? {warning}");
                }
            }

            return result.ToString();
        }
    }
}

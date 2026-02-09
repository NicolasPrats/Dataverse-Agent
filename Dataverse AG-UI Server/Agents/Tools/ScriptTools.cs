using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Dataverse_AG_UI_Server.Agents.Base;
using Dataverse_AG_UI_Server.Services;

namespace Dataverse_AG_UI_Server.Agents.Tools
{
    /// <summary>
    /// Provides dynamic script compilation and execution for Dataverse operations.
    /// 
    /// SECURITY NOTES:
    /// - Uses static code analysis to block dangerous APIs (IO, Process, Reflection)
    /// - Executes in the same AppDomain (no full sandbox)
    /// - Network access is allowed (required for Dataverse)
    /// - For production: consider process isolation or stricter validation
    /// </summary>
    public class ScriptTools : DataverseToolsBase
    {
        public const string RunScript = "dataverse_runScript";
        private readonly DataverseServiceClientFactory _serviceClientFactory;

        private static readonly string[] ForbiddenNamespaces =
        [
            "System.IO",
            "System.Diagnostics",
            "System.Reflection",
            "System.Runtime.InteropServices",
            "System.Security",
            "System.Threading.Thread", // Allow Task but not Thread manipulation
            "System.Environment",
            "Microsoft.Win32"
        ];

        public ScriptTools(DataverseServiceClientFactory serviceClientFactory)
            : base(serviceClientFactory)
        {
            _serviceClientFactory = serviceClientFactory;
        }

        public Tool RunScriptToolAsync => new(RunScript, RunScriptAsync);

        public Tool[] AllTools => [RunScriptToolAsync];

        [Description("Executes a Dataverse script with security restrictions. The scriptBody must contain only the method body (including brackets) without signature. The script receives an IOrganizationService instance to interact with Dataverse. Forbidden: file access, process execution, reflection, environment variables.")]
        public async Task<object> RunScriptAsync(
            [Description("The body of the script including brackets. Example: { var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest()); return $\"User: {response.UserId}\"; }")] 
            string scriptBody)
        {
            return await Task.Run<object>(() =>
            {
                try
                {
                    // Step 1: Generate full class code
                    var fullCode = GenerateFullClass(scriptBody);
                    
                    // Step 2: Security validation
                    ValidateCodeSecurity(fullCode);
                    
                    // Step 3: Compile
                    var assembly = CompileCode(fullCode);
                    
                    if (assembly == null)
                    {
                        return new
                        {
                            Success = false,
                            Error = "Failed to compile the script",
                            ScriptBody = scriptBody
                        };
                    }

                    // Step 4: Execute with pre-authenticated service
                    var result = ExecuteScript(assembly);
                    
                    return new
                    {
                        Success = true,
                        Result = result,
                        Message = "Script executed successfully"
                    };
                }
                catch (SecurityException secEx)
                {
                    return new
                    {
                        Success = false,
                        Error = "Security validation failed: " + secEx.Message,
                        ErrorType = "SecurityException",
                        Hint = "The script attempted to use forbidden APIs. Allowed: Dataverse SDK only.",
                        ScriptBody = scriptBody
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
                        StackTrace = ex.StackTrace,
                        ScriptBody = scriptBody
                    };
                }
            });
        }

        private string GenerateFullClass(string scriptBody)
        {
            return $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace DynamicScript
{{
    public class ScriptRunner
    {{
        public string DoJob(IOrganizationService service)
        {scriptBody}
    }}
}}";
        }

        /// <summary>
        /// Validates code security by analyzing the syntax tree for forbidden API usage.
        /// This is a basic blacklist approach - not foolproof but provides a first line of defense.
        /// </summary>
        private void ValidateCodeSecurity(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Check for forbidden namespaces in member access
            var memberAccesses = root.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>();

            foreach (var access in memberAccesses)
            {
                var fullText = access.ToString();
                
                foreach (var forbidden in ForbiddenNamespaces)
                {
                    if (fullText.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new SecurityException(
                            $"Forbidden API usage detected: '{fullText}'. " +
                            $"Access to '{forbidden}' is not allowed.");
                    }
                }
            }

            // Check for forbidden type names (catch 'new FileStream()', etc.)
            var objectCreations = root.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>();

            foreach (var creation in objectCreations)
            {
                var typeName = creation.Type.ToString();
                
                var forbiddenTypes = new[]
                {
                    "FileStream", "File", "Directory", "Process",
                    "RegistryKey", "Thread", "AppDomain"
                };

                if (forbiddenTypes.Any(f => typeName.Contains(f)))
                {
                    throw new SecurityException(
                        $"Forbidden type instantiation: '{typeName}'. " +
                        "File system, process, and system manipulation are not allowed.");
                }
            }

            // Warn about 'dynamic' usage (harder to validate)
            var dynamicUsages = root.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(i => i.Identifier.ValueText == "dynamic");

            if (dynamicUsages.Any())
            {
                throw new SecurityException(
                    "Use of 'dynamic' keyword is forbidden as it bypasses security checks.");
            }
        }

        private Assembly? CompileCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IOrganizationService).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Entity).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OrganizationRequest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(EntityMetadata).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(QueryExpression).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(WhoAmIRequest).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
            };

            var compilation = CSharpCompilation.Create(
                "DynamicScript",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(OptimizationLevel.Release));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics
                    .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                var errorMessages = string.Join("\n", failures.Select(f => $"{f.Id}: {f.GetMessage()}"));
                throw new InvalidOperationException($"Compilation failed:\n{errorMessages}");
            }

            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        /// <summary>
        /// Executes the compiled script with a pre-authenticated Dataverse service.
        /// This avoids re-authentication and is compatible with all auth methods including OAuth interactive.
        /// </summary>
        private string ExecuteScript(Assembly assembly)
        {
            var type = assembly.GetType("DynamicScript.ScriptRunner");
            if (type == null)
            {
                throw new InvalidOperationException("ScriptRunner class not found in compiled assembly");
            }

            var instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException("Failed to create instance of ScriptRunner");
            }

            var method = type.GetMethod("DoJob");
            if (method == null)
            {
                throw new InvalidOperationException("DoJob method not found in ScriptRunner class");
            }

            // Pass the pre-authenticated service instead of connection string
            // This works with all auth methods including OAuth interactive
            var service = ServiceClient as IOrganizationService;
            var result = method.Invoke(instance, [service]);
            
            return result?.ToString() ?? "Script executed successfully with no return value";
        }
    }
}

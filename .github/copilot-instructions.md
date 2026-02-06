# Code Generation Guidelines

## General Principles

When generating or modifying code in this project, follow these modern C# best practices:

### 1. Use Primary Constructors

**DO** use primary constructors (C# 12+) when appropriate to reduce boilerplate code:

```csharp
// ✅ GOOD - Primary constructor
public class MyService(ILogger<MyService> logger, IConfiguration config)
{
    private readonly ILogger<MyService> _logger = logger;
    private readonly IConfiguration _config = config;
}

// ❌ AVOID - Traditional constructor when primary constructor is cleaner
public class MyService
{
    private readonly ILogger<MyService> _logger;
    private readonly IConfiguration _config;
    
    public MyService(ILogger<MyService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }
}
```

### 2. Prefer Non-Nullable Types

**DO** use non-nullable reference types by default and only use nullable types when null is a valid value:

```csharp
// ✅ GOOD - Non-nullable by default
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address? OptionalAddress { get; set; } // Nullable only when needed
}

// ❌ AVOID - Unnecessary nullable types
public class User
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}
```

**DO** initialize non-nullable properties with default values:
- `string.Empty` for strings
- `new()` or `[]` for collections
- `default` or constructor for value types

### 3. Use Modern Initialization Patterns

**DO** use modern collection and object initialization syntax:

```csharp
// ✅ GOOD - Collection expressions (C# 12+)
public List<string> Tags { get; set; } = [];
public string[] Items { get; set; } = [];

// ✅ GOOD - Target-typed new
public MyClass Instance { get; set; } = new();

// ❌ AVOID - Verbose initialization
public List<string> Tags { get; set; } = new List<string>();
public MyClass Instance { get; set; } = new MyClass();
```

**DO** use collection expressions for inline initialization:

```csharp
// ✅ GOOD
public AgentDefinition Agent { get; set; } = new()
{
    Name = "MyAgent",
    Tools = ["tool1", "tool2", "tool3"]
};

// ❌ AVOID
public AgentDefinition Agent { get; set; } = new()
{
    Name = "MyAgent",
    Tools = new List<string> { "tool1", "tool2", "tool3" }
};
```

### 4. Use Modern Connection and Client Initialization

**DO** use modern patterns for database and service connections:

```csharp
// ✅ GOOD - Modern async initialization
public class DataService(string connectionString)
{
    private readonly string _connectionString = connectionString;
    
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new Connection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

// ✅ GOOD - Dependency injection with options pattern
public class ApiClient(HttpClient httpClient, IOptions<ApiSettings> options)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ApiSettings _settings = options.Value;
}
```

### 5. Constructor Parameter Validation

**DO** validate constructor parameters at the start of the constructor or when first used:

```csharp
// ✅ GOOD
public class MyService(string apiKey, ILogger logger)
{
    private readonly string _apiKey = !string.IsNullOrWhiteSpace(apiKey) 
        ? apiKey 
        : throw new ArgumentException("API key cannot be empty", nameof(apiKey));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

### 6. Property Patterns

**DO** use property patterns consistently:

```csharp
// ✅ GOOD - Auto-properties with initializers
public string Name { get; set; } = string.Empty;
public bool IsActive { get; set; } = true;
public List<Item> Items { get; set; } = [];

// ✅ GOOD - Read-only properties for immutable data
public string Id { get; init; } = Guid.NewGuid().ToString();

// ✅ GOOD - Required properties (C# 11+)
public required string UserId { get; set; }
```

### 7. Avoid Magic Numbers - Use Constants and Enums

**DO NOT** use magic numbers or hardcoded values. Always use named constants or enums for better readability and maintainability:

```csharp
// ✅ GOOD - Using enum
public enum SolutionComponentType
{
    Entity = 1,
    Attribute = 2,
    OptionSet = 9
}

AddComponentToSolution(componentId, SolutionComponentType.Entity);

// ✅ GOOD - Using constants
private const int DefaultTimeout = 30;
private const int MaxRetries = 3;
private const string DefaultCulture = "en-US";

public async Task<Result> ProcessAsync()
{
    await RetryAsync(MaxRetries);
}

// ❌ AVOID - Magic numbers
AddComponentToSolution(componentId, 1); // What does 1 mean?
await Task.Delay(30000); // What is this timeout for?
if (statusCode == 200) // Use HttpStatusCode.OK instead
```

**DO** create enums for related sets of values:

```csharp
// ✅ GOOD
public enum AttributeType
{
    String,
    Integer,
    Decimal,
    Boolean,
    DateTime
}

// ❌ AVOID - String literals everywhere
if (type == "string") { /* ... */ }
if (type == "integer") { /* ... */ }
```

### 8. DRY Principle - Don't Repeat Yourself

**DO NOT** duplicate code. Always factor out common logic into reusable methods, classes, or utilities:

```csharp
// ✅ GOOD - Factored common logic
public class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && email.Contains('@');
    }
}

public class UserService
{
    public bool ValidateUser(User user)
    {
        return ValidationHelper.IsValidEmail(user.Email);
    }
}

public class ContactService
{
    public bool ValidateContact(Contact contact)
    {
        return ValidationHelper.IsValidEmail(contact.Email);
    }
}

// ❌ AVOID - Duplicated validation logic
public class UserService
{
    public bool ValidateUser(User user)
    {
        return !string.IsNullOrWhiteSpace(user.Email) && user.Email.Contains('@');
    }
}

public class ContactService
{
    public bool ValidateContact(Contact contact)
    {
        return !string.IsNullOrWhiteSpace(contact.Email) && contact.Email.Contains('@');
    }
}
```

**DO** extract repeated patterns into base classes or extension methods:

```csharp
// ✅ GOOD - Base class for common behavior
public abstract class RepositoryBase<T>
{
    protected readonly DbContext _context;
    
    protected RepositoryBase(DbContext context)
    {
        _context = context;
    }
    
    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    public virtual async Task SaveAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
}

// ❌ AVOID - Duplicating repository logic in each class
public class UserRepository
{
    private readonly DbContext _context;
    
    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Set<User>().FindAsync(id);
    }
    
    public async Task SaveAsync(User entity)
    {
        _context.Set<User>().Update(entity);
        await _context.SaveChangesAsync();
    }
}

public class ProductRepository
{
    private readonly DbContext _context;
    
    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _context.Set<Product>().FindAsync(id);
    }
    
    public async Task SaveAsync(Product entity)
    {
        _context.Set<Product>().Update(entity);
        await _context.SaveChangesAsync();
    }
}
```

**When to Factor Out Code:**
- When the same logic appears 2+ times
- When patterns are similar but not identical (consider parameterization)
- When code blocks serve the same conceptual purpose
- When factoring improves testability and maintainability

**When NOT to Factor Out:**
- When it reduces code clarity significantly
- When the duplication is coincidental (looks similar but serves different purposes)
- When factoring creates tight coupling between unrelated modules

### 9. NuGet Package Versions

**ALWAYS** use the latest stable version of NuGet packages unless there is a specific reason not to:

```xml
<!-- ✅ GOOD - Latest stable version -->
<PackageReference Include="Microsoft.Extensions.AI" Version="1.0.0" />
<PackageReference Include="Azure.AI.OpenAI" Version="2.1.0" />

<!-- ⚠️ AVOID - Old versions without justification -->
<PackageReference Include="Microsoft.Extensions.AI" Version="0.5.0" />

<!-- ❌ NEVER - Wildcards in production -->
<PackageReference Include="Microsoft.Extensions.AI" Version="*" />
```

**DO** check for latest versions when:
- Adding a new package to the project
- Updating dependencies for security patches
- Upgrading to access new features

**DO** document if using an older version:
```xml
<!-- Using version 1.5.0 due to compatibility with legacy API -->
<PackageReference Include="Legacy.Library" Version="1.5.0" />
```

**EXCEPTIONS** - Use specific older versions only when:
- Required for compatibility with other dependencies
- A newer version has breaking changes you cannot accommodate
- Project requirements mandate a specific version
- Security or stability issues with latest version

### 10. Documentation and Comments

**DO NOT** generate documentation files (`.md`, `.txt`, etc.) unless explicitly requested by the user.

**DO NOT** write comments that simply paraphrase the code:

```csharp
// ❌ AVOID - Useless paraphrasing comments
public class UserService
{
    // Gets the user by id
    public async Task<User?> GetUserByIdAsync(string id)
    {
        // Return the user from the repository
        return await _repository.GetByIdAsync(id);
    }
    
    // Saves the user
    public async Task SaveUserAsync(User user)
    {
        // Validate the user
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        // Save to repository
        await _repository.SaveAsync(user);
    }
}
```

**DO** write comments only when they add value:
- Explain **WHY**, not **WHAT**
- Document complex algorithms or business logic
- Clarify non-obvious decisions or workarounds
- Provide context that isn't obvious from the code

```csharp
// ✅ GOOD - Comments that add value
public class UserService
{
    public async Task<User?> GetUserByIdAsync(string id)
    {
        // No comment needed - method name is self-explanatory
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task SaveUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        
        // Clear cache before save to prevent stale data issues with eventual consistency
        // See bug #1234 for context
        await _cache.InvalidateUserCacheAsync(user.Id);
        
        await _repository.SaveAsync(user);
    }
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        // Using a 90-day threshold per business requirement from PROD-567
        // Users are considered active if they logged in within the last 90 days
        var threshold = DateTime.UtcNow.AddDays(-90);
        return await _repository.GetUsersLoggedInAfterAsync(threshold);
    }
}
```

**DO** prefer self-documenting code over comments:

```csharp
// ✅ GOOD - Self-documenting code
private const int ActiveUserThresholdDays = 90;

public async Task<List<User>> GetActiveUsersAsync()
{
    var threshold = DateTime.UtcNow.AddDays(-ActiveUserThresholdDays);
    return await _repository.GetUsersLoggedInAfterAsync(threshold);
}

// ❌ AVOID - Needs comment because code isn't clear
public async Task<List<User>> GetActiveUsersAsync()
{
    // 90 days = active user threshold
    var threshold = DateTime.UtcNow.AddDays(-90);
    return await _repository.GetUsersLoggedInAfterAsync(threshold);
}
```

**Summary on Documentation:**
- ✅ Write comments that explain **WHY** and provide context
- ✅ Use self-documenting code (clear names, constants, etc.)
- ✅ Document complex algorithms, workarounds, and business rules
- ❌ Don't generate documentation files unless explicitly asked
- ❌ Don't write comments that just restate what the code does
- ❌ When in doubt, prefer no comment over a useless one

## Specific Patterns for This Project

### Agent Definitions

When creating agent classes:

```csharp
// ✅ GOOD
public abstract class AgentBase(string name, string instructions) : IAgent
{
    public string Name { get; protected set; } = name;
    public string Instructions { get; protected set; } = instructions;
    public AIAgent Agent { get; protected set; } = null!; // Will be initialized later
}
```

### Configuration Classes

```csharp
// ✅ GOOD
public class ServiceSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    public List<string> AllowedHosts { get; set; } = [];
}
```

### Service Classes with Dependencies

```csharp
// ✅ GOOD
public class AgentOrchestrator(
    ILogger<AgentOrchestrator> logger,
    IAgentFactory agentFactory,
    IToolRegistry toolRegistry)
{
    private readonly ILogger<AgentOrchestrator> _logger = logger;
    private readonly IAgentFactory _agentFactory = agentFactory;
    private readonly IToolRegistry _toolRegistry = toolRegistry;
}
```

## Anti-Patterns to Avoid

### ❌ Don't use nullable when not needed

```csharp
// ❌ AVOID
public class Config
{
    public string? Name { get; set; } // Should be non-nullable with = string.Empty
    public List<string>? Items { get; set; } // Should be non-nullable with = []
}
```

### ❌ Don't use old collection initialization

```csharp
// ❌ AVOID
public List<string> Items { get; set; } = new List<string>();
public Dictionary<string, int> Map { get; set; } = new Dictionary<string, int>();

// ✅ GOOD
public List<string> Items { get; set; } = [];
public Dictionary<string, int> Map { get; set; } = [];
```

### ❌ Don't use verbose constructors for simple dependency injection

```csharp
// ❌ AVOID - When primary constructor is clearer
public class MyService
{
    private readonly ILogger _logger;
    private readonly IConfig _config;
    
    public MyService(ILogger logger, IConfig config)
    {
        _logger = logger;
        _config = config;
    }
}

// ✅ GOOD
public class MyService(ILogger logger, IConfig config)
{
    private readonly ILogger _logger = logger;
    private readonly IConfig _config = config;
}
```

### ❌ Don't use magic numbers or hardcoded values

```csharp
// ❌ AVOID
public void ProcessData()
{
    if (status == 1) { /* ... */ } // What is 1?
    await Task.Delay(5000); // Why 5000?
    var result = Calculate(3.14159); // Use Math.PI
}

// ✅ GOOD
private const int StatusActive = 1;
private const int DefaultDelayMs = 5000;

public void ProcessData()
{
    if (status == StatusActive) { /* ... */ }
    await Task.Delay(DefaultDelayMs);
    var result = Calculate(Math.PI);
}
```

### ❌ Don't duplicate code

```csharp
// ❌ AVOID - Code duplication
public class OrderService
{
    public decimal CalculateOrderTotal(Order order)
    {
        decimal total = 0;
        foreach (var item in order.Items)
        {
            total += item.Price * item.Quantity;
        }
        if (order.DiscountPercent > 0)
        {
            total -= total * (order.DiscountPercent / 100);
        }
        return total;
    }
    
    public decimal CalculateQuoteTotal(Quote quote)
    {
        decimal total = 0;
        foreach (var item in quote.Items)
        {
            total += item.Price * item.Quantity;
        }
        if (quote.DiscountPercent > 0)
        {
            total -= total * (quote.DiscountPercent / 100);
        }
        return total;
    }
}

// ✅ GOOD - Factored common logic
public class PricingService
{
    public decimal CalculateTotal<T>(IEnumerable<T> items, decimal discountPercent) 
        where T : ILineItem
    {
        var total = items.Sum(item => item.Price * item.Quantity);
        
        if (discountPercent > 0)
        {
            total -= total * (discountPercent / 100);
        }
        
        return total;
    }
}

public class OrderService(PricingService pricingService)
{
    public decimal CalculateOrderTotal(Order order) 
        => pricingService.CalculateTotal(order.Items, order.DiscountPercent);
}

public class QuoteService(PricingService pricingService)
{
    public decimal CalculateQuoteTotal(Quote quote) 
        => pricingService.CalculateTotal(quote.Items, quote.DiscountPercent);
}
```

## When to Use Traditional Constructors

Use traditional constructors when:
- You need complex initialization logic
- You have validation that spans multiple parameters
- The class has multiple constructors
- You need to call base constructors with transformed parameters

```csharp
// ✅ GOOD - Complex logic warrants traditional constructor
public class ComplexService : BaseService
{
    private readonly string _processedValue;
    
    public ComplexService(string rawValue, IValidator validator) : base(validator)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            throw new ArgumentException("Value required", nameof(rawValue));
            
        _processedValue = ProcessValue(rawValue);
        InitializeResources();
    }
}
```

## SOLID Principles Verification

**CRITICAL**: After every code generation or modification, verify that the code adheres to SOLID principles:

### S - Single Responsibility Principle (SRP)
- ✅ Each class should have only one reason to change
- ✅ A class should do one thing and do it well
- ❌ Avoid "god classes" that handle multiple unrelated responsibilities

```csharp
// ✅ GOOD - Single responsibility
public class UserValidator
{
    public ValidationResult Validate(User user) { /* ... */ }
}

public class UserRepository
{
    public Task SaveAsync(User user) { /* ... */ }
}

// ❌ AVOID - Multiple responsibilities
public class UserManager
{
    public ValidationResult Validate(User user) { /* ... */ }
    public Task SaveAsync(User user) { /* ... */ }
    public void SendEmail(User user) { /* ... */ }
    public void LogActivity(User user) { /* ... */ }
}
```

### O - Open/Closed Principle (OCP)
- ✅ Classes should be open for extension, closed for modification
- ✅ Use abstractions (interfaces/abstract classes) to allow extensibility
- ✅ Prefer composition and inheritance over modifying existing code

```csharp
// ✅ GOOD - Open for extension
public interface ITool
{
    Task<string> ExecuteAsync(string input);
}

public class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = [];
    
    public void RegisterTool(string name, ITool tool)
    {
        _tools[name] = tool;
    }
}

// ❌ AVOID - Requires modification for each new tool
public class ToolExecutor
{
    public Task<string> ExecuteAsync(string toolName, string input)
    {
        if (toolName == "tool1") return ExecuteTool1(input);
        if (toolName == "tool2") return ExecuteTool2(input);
        // Need to modify this method for each new tool
    }
}
```

### L - Liskov Substitution Principle (LSP)
- ✅ Derived classes must be substitutable for their base classes
- ✅ Overridden methods should maintain the contract of the base class
- ❌ Avoid breaking the expected behavior of base class methods

```csharp
// ✅ GOOD - Derived class maintains contract
public abstract class AgentBase
{
    public virtual async Task<string> RunAsync(string input)
    {
        return await RunAsync(input, CancellationToken.None);
    }
}

public class JokeAgent : AgentBase
{
    public override async Task<string> RunAsync(string input)
    {
        // Maintains expected behavior
        return await base.RunAsync(input);
    }
}

// ❌ AVOID - Breaking base class contract
public class BadAgent : AgentBase
{
    public override async Task<string> RunAsync(string input)
    {
        throw new NotImplementedException(); // Breaks LSP!
    }
}
```

### I - Interface Segregation Principle (ISP)
- ✅ Clients should not depend on interfaces they don't use
- ✅ Create specific, focused interfaces rather than large, general ones
- ✅ Split large interfaces into smaller, cohesive ones

```csharp
// ✅ GOOD - Segregated interfaces
public interface IReadOnlyRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task SaveAsync(T entity);
    Task DeleteAsync(string id);
}

// ❌ AVOID - Fat interface forcing unnecessary implementations
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task SaveAsync(T entity);
    Task DeleteAsync(string id);
    Task BulkInsertAsync(IEnumerable<T> entities);
    Task ExecuteStoredProcedure(string name, params object[] args);
    // Too many methods - not all implementations need all of these
}
```

### D - Dependency Inversion Principle (DIP)
- ✅ Depend on abstractions, not concretions
- ✅ High-level modules should not depend on low-level modules
- ✅ Use dependency injection with interfaces

```csharp
// ✅ GOOD - Depends on abstractions
public class AgentOrchestrator(
    ILogger<AgentOrchestrator> logger,
    IAgentFactory agentFactory,
    IToolRegistry toolRegistry)
{
    private readonly ILogger<AgentOrchestrator> _logger = logger;
    private readonly IAgentFactory _agentFactory = agentFactory;
    private readonly IToolRegistry _toolRegistry = toolRegistry;
}

// ❌ AVOID - Depends on concrete implementations
public class AgentOrchestrator
{
    private readonly ConsoleLogger _logger = new();
    private readonly AgentFactory _agentFactory = new();
    private readonly ToolRegistry _toolRegistry = new();
}
```

### SOLID Verification Checklist

After generating or modifying code, ask yourself:

1. **SRP**: Does each class have only one reason to change?
2. **OCP**: Can I add new functionality without modifying existing code?
3. **LSP**: Can I substitute derived classes without breaking functionality?
4. **ISP**: Are my interfaces focused and cohesive?
5. **DIP**: Am I depending on abstractions rather than concrete implementations?

If the answer to any question is "No", refactor the code before proceeding.

## Summary

- ✅ Use primary constructors for simple dependency injection
- ✅ Prefer non-nullable types with proper initialization
- ✅ Use modern syntax: `[]` for collections, `new()` for objects
- ✅ Initialize all non-nullable properties
- ✅ Use nullable types (`?`) only when null is a valid semantic value
- ✅ Follow async/await patterns for I/O operations
- ✅ Leverage C# 12+ features when they improve code clarity
- ✅ **Never use magic numbers - always use named constants or enums**
- ✅ **Apply DRY principle - factor out duplicated code when it makes sense**
- ✅ **Always use the latest stable version of NuGet packages by default**
- ✅ **Do not generate documentation files unless explicitly requested**
- ✅ **Avoid useless comments that paraphrase code - prefer self-documenting code**
- ✅ **Write comments only when they explain WHY or provide valuable context**
- ✅ **ALWAYS verify SOLID principles after code generation or modification**

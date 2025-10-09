# 🚀 Min.ApiManager
[![NuGet Version](https://img.shields.io/nuget/v/Min.ApiManager.svg)](https://www.nuget.org/packages/Min.ApiManager/)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Min.ApiManager-blue.svg)](https://github.com/mindev99/Min.ApiManager)

💡A lightweight .NET class library for managing API addresses, supporting multi-environment configuration, dynamic route management, and intelligent endpoint construction.


## 📝 Features
- **🌍 Multi-environment Support** - Compatible with environments like Development, Testing, UAT, Staging, and Production
- **🔧 Flexible Configuration** - Supports three configuration formats: JSON, XML, and INI
- **🛡️ Thread Safety** - Built-in locking mechanism for high-concurrency scenarios
- **📝 Templated Routing** - Supports parameterized route templates (e.g., `/api/users/{userId}`)
- **🔗 Intelligent Endpoint Construction** - Automatically concatenates domain names, routes, and query parameters
- **📡 Event Notifications** - Listens for events such as environment switching and domain name changes
- **💾 Weak Reference Events** - Prevents memory leaks, ideal for long-running applications
- **🎯 Chained Calls** - Smooth API design with method chaining support
- **📦 Lightweight** - No external dependencies; only relies on .NET standard libraries


## 📦 Installation
### NuGet Package Manager
```bash
Install-Package Min.ApiManager
```

### .NET CLI
```bash
dotnet add package Min.ApiManager
```

### PackageReference
```xml
<PackageReference Include="Min.ApiManager" Version="1.0.0" />
```


## 🏗️ Project Structure
```
Min.ApiManager/
├── ApiManager.cs                           # Core manager class
├── ApiEnvironment.cs                       # Environment enumeration definition
├── APIDoc.cs                               # Configuration document model
├── Event/                                  # Event-related components
│   ├── WeakEvent.cs                        # Weak reference event implementation
│   └── EnvironmentChangedEventArgs.cs      # Environment change event model
└── Interface/                              # Interface definitions
    ├── IApiManager.cs                      # Default main interface
    ├── IConfigurableManager.cs             # Configuration management interface
    ├── IDomainManager.cs                   # Domain name management interface
    ├── IRouteManager.cs                    # Route management interface
    ├── IEndpointBuilder.cs                 # Endpoint construction interface
    ├── ITemplateManager.cs                 # Templated routing interface
    └── IValidationManager.cs               # Address validation interface
```


## 🎯 Core Concepts
### 🌳 Environment Management
The following environment types are supported:

| 🌱 Environment Type | 📝 Description |
| ------------- | ---------- |
| `Development` | Local development environment |
| `Testing`     | Testing/QA environment |
| `UAT`         | User Acceptance Testing environment |
| `Staging`     | Pre-production/simulation production environment |
| `Sandbox`     | Sandbox environment |
| `Production`  | Production environment |


### 🔧 Configuration Formats
Three configuration formats are supported:

| 🛠 Format | 📝 Description |
| ------ | -------------------------- |
| JSON   | Structured configuration, easy to read and edit |
| XML    | Traditional configuration format |
| INI    | Simple key-value pair format |


#### 🟨 JSON Configuration
```json
{
  "CurrentEnvironment": "Development",
  "Domains": {
    "Development": "https://development.mindev.cn",
    "Staging": "https://staging.mindev.cn",
    "Production": "https://production.mindev.cn",
    "Sandbox": "https://sandbox.mindev.cn/",
    "Testing": "https://testing.mindev.cn/",
    "UAT": "https://uat.mindev.cn/"
  },
  "Routes": {
    "GetUser": "/user/get",
    "UpdateUser": "/user/update",
    "DeleUser": "/uid={uid}"
  },
  "DefaultQueryParams": {
    "apiKey": "your-api-key",
    "version": "v1",
    "format": "json"
  }
}
```


#### 🟥 XML Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<ApiConfig>
  <!-- Current environment -->
  <CurrentEnvironment>Development</CurrentEnvironment>

  <!-- Domain names for each environment -->
  <Domains>
    <Domain Environment="Development">https://development.mindev.cn</Domain>
    <Domain Environment="Staging">https://staging.mindev.cn</Domain>
    <Domain Environment="Production">https://production.mindev.cn</Domain>
    <Domain Environment="Sandbox">https://sandbox.mindev.cn/</Domain>
    <Domain Environment="Testing">https://testing.mindev.cn/</Domain>
    <Domain Environment="UAT">https://uat.mindev.cn/</Domain>
  </Domains>

  <!-- API route addresses -->
  <Routes>
    <Route Key="GetUser">/user/get</Route>
    <Route Key="UpdateUser">/user/update</Route>
    <Route Key="DeleUser">/uid={uid}</Route>
  </Routes>

  <!-- Default query parameters -->
  <DefaultQueryParams>
    <Param Key="apiKey">your-api-key</Param>
    <Param Key="version">v1</Param>
    <Param Key="format">json</Param>
  </DefaultQueryParams>
</ApiConfig>
```


#### 🟩 INI Configuration
```ini
; Current environment configuration
[Environment]
; Optional values: Development / Staging / Production / Sandbox / Testing / UAT
CurrentEnvironment=Development

; Domain names for each environment
[Domains]
Development=https://development.mindev.cn
Staging=https://staging.mindev.cn
Production=https://production.mindev.cn
Sandbox=https://sandbox.mindev.cn/
Testing=https://testing.mindev.cn/
UAT=https://uat.mindev.cn/

; API route addresses
[Routes]
GetUser=/user/get
UpdateUser=/user/update
DeleUser=/uid={uid}

; Default query parameters
[DefaultQueryParams]
apiKey=your-api-key
version=v1
format=json
```


## 📖 Quick Start
### 1. Basic Initialization
> 📝 Note: This example uses a project-wide global shared instance. If not needed, you can create a separate manager via `new`.

#### 📌 Using the Singleton Pattern
```csharp
// Import namespace
using Min.ApiManager;

// Create a static global API manager instance for the project
public static class APIExamples
{
    private static ApiManager? _instance;
    private static readonly object _lock = new();
    
    public static ApiManager ApiManager
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ApiManager();
                    }
                }
            }
            return _instance;
        }
    }
}
```

#### 💻 Initialization via Code
```csharp
// Program entry point
internal class Program
{
    // Main method
    internal static void Main(string[] args)
    {
        // Initialize configuration (must be called at the program entry)
        APIExamples.ApiManager.Initialize(SetUrlAddress);
    }

    // Callback method
    private void SetUrlAddress(ApiManager manager)
    {
        // Set domain names (supports batch addition via dictionary)
        manager.SetDomain(ApiEnvironment.Development, "https://dev.mindev.cn");
        manager.SetDomain(ApiEnvironment.Production, "https://api.mindev.com");

        // Define route dictionary
        var routes = new Dictionary<string, string>
        {
            { "login", "/api/login" },
            { "register", "/api/register" }
        };

        // Batch set routes
        manager.SetRoutes(routes);
    }
}
```

#### 📄 Initialization via File
```csharp
// Program entry point
internal class Program
{
    // Main method
    internal static void Main(string[] args)
    {
        // Path to the JSON configuration file
        string jsonFilePath = @"C:\Configs\api_config.json";
        try
        {
            // Read file content as a string
            string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

            // Load configuration from file content
            APIExamples.ApiManager.LoadConfig(jsonContent, LoadFormat.JSON);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to read file: {ex.Message}");
        }
    }
}
```


### 2. Building API Endpoints
```csharp
// Get a simple endpoint
string endpoint = APIExamples.ApiManager.GetEndpoint("GetUser");
// Result: https://development.mindev.cn/user/get

// Get an endpoint with template parameters
var templateParams = new { uid = 123 };
string userEndpoint = APIExamples.ApiManager.GetEndpoint("DeleUser", templateParams);
// Result: https://development.mindev.cn/uid=123

// Get an endpoint with additional query parameters
var queryParams = new { page = 1, limit = 10 };
string usersWithPaging = APIExamples.ApiManager.GetEndpoint("GetUsers", null, queryParams);
// Result: https://development.mindev.cn/user/get?page=1&limit=10

// Get an endpoint with both template and query parameters
string userWithQuery = APIExamples.ApiManager.GetEndpoint("DeleUser", templateParams, queryParams);
// Result: https://development.mindev.cn/uid=123?page=1&limit=10
```


### 3. Environment Switching
```csharp
// Switch to Production environment
APIExamples.ApiManager.SwitchEnvironment(ApiEnvironment.Production);

// Safe switching (no exceptions thrown)
if (APIExamples.ApiManager.TrySwitchEnvironment(ApiEnvironment.Staging, out string? error))
{
    Debug.WriteLine("Environment switched successfully");
}
else
{
    Debug.WriteLine($"Environment switching failed: {error}");
}

// Get current environment
ApiEnvironment currentEnv = APIExamples.ApiManager.CurrentEnvironment;
```


## ⚙️ Advanced Features
### 📡 Event Listening
```csharp
// Listen for environment switching events
APIExamples.ApiManager.OnEnvironmentChanged += (sender, args) =>
{
    Debug.WriteLine($"Environment switched from {args.OldEnvironment} to {args.NewEnvironment}");
    // Perform cleanup or initialization after environment switching here
};

// Listen for domain name change events
APIExamples.ApiManager.OnDomainChanged += (env, newDomain, oldDomain) =>
{
    Debug.WriteLine($"Domain for environment {env} updated from {oldDomain} to {newDomain}");
};

// Listen for route change events
APIExamples.ApiManager.OnRouteChanged += (key, newRoute, oldRoute) =>
{
    Debug.WriteLine($"Route {key} updated from {oldRoute} to {newRoute}");
};
```


### ♻️ Dynamic Configuration Update
```csharp
// Update configuration at runtime
APIExamples.ApiManager.UpdateConfig(manager =>
{
    // Add a new route
    manager.SetRoute("NewFeature", "/api/new-feature");
    
    // Update a domain name
    manager.SetDomain(ApiEnvironment.Development, "https://new-dev-api.example.com");
    
    // Add a new default query parameter
    manager.AddDefaultQueryParam("debug", "true");
});

// Batch update routes
var newRoutes = new Dictionary<string, string>
{
    ["Feature1"] = "/api/feature1",
    ["Feature2"] = "/api/feature2",
    ["Feature3"] = "/api/feature3"
};

// Batch set new routes
APIExamples.ApiManager.SetRoutes(newRoutes);
```


## 🐞 Troubleshooting
### 🤔 Common Issues
1. **Error: "Please call Initialize() to set the domain name first"**
   
```csharp
// ❌ Incorrect usage
var apiManager = new ApiManager();
string url = apiManager.GetEndpoint("GetUser"); // Throws an exception (route "GetUser" not found; initialization required)

// ✔️ Correct usage
var apiManager = new ApiManager();
apiManager.Initialize(manager => {
   manager.SetDomain(ApiEnvironment.Development, "https://api.example.com");
   manager.SetRoute("GetUser", "/api/users/{userId}");
});
string url = apiManager.GetEndpoint("GetUser"); // Works correctly
```

2. **Template parameters not fully replaced**
```csharp
// Error: Missing required template parameter
string url = apiManager.GetEndpoint("GetUser", new { id = 123 }); // "userId" parameter is missing

// Correct: Provide all required template parameters
string url = apiManager.GetEndpoint("GetUser", new { userId = 123 });
```

3. **Failed to switch environment**
```csharp
// Check if the target environment has a configured domain
if (!apiManager.HasDomain(ApiEnvironment.Production))
{
   Console.WriteLine("Domain for Production environment not configured");
}

// Use safe switching method
if (!apiManager.TrySwitchEnvironment(ApiEnvironment.Production, out string error))
{
   Console.WriteLine($"Environment switching failed: {error}");
}
```


## 📋 Detailed Usage Example
### 🛠️ Example: Complete E-commerce System API Management
```csharp
/// <summary>
/// E-commerce System API Manager - Complete Example
/// Demonstrates how to use Min.ApiManager in real-world projects
/// </summary>
public class ECommerceApiManager
{
    private readonly ApiManager _apiManager;

    public ECommerceApiManager()
    {
        _apiManager = new ApiManager();
        InitializeApiManager();
    }

    private void InitializeApiManager()
    {
        _apiManager.Initialize(manager =>
        {
            // Configure multi-environment domains as needed
            manager.SetDomains(new Dictionary<ApiEnvironment, string>
            {
                [ApiEnvironment.Development] = "https://dev-api.shop.com",
                [ApiEnvironment.Staging] = "https://staging-api.shop.com",
                [ApiEnvironment.Production] = "https://api.shop.com",
                [ApiEnvironment.Testing] = "https://test-api.shop.com"
            });

            // Configure product-related routes as needed
            manager.SetRoutes(new Dictionary<string, string>
            {
                // Product Management
                ["GetProducts"] = "/api/products",
                ["GetProduct"] = "/api/products/{productId}",
                ["CreateProduct"] = "/api/products",
                ["SearchProducts"] = "/api/products/search",

                // Order Management
                ["GetOrders"] = "/api/orders",
                ["GetOrder"] = "/api/orders/{orderId}",
                ["CreateOrder"] = "/api/orders",
                ["UpdateOrderStatus"] = "/api/orders/{orderId}/status",

                // User Management
                ["GetUsers"] = "/api/users",
                ["GetUser"] = "/api/users/{userId}",
                ["CreateUser"] = "/api/users",
                ["UpdateUser"] = "/api/users/{userId}",

                // Payment Management
                ["ProcessPayment"] = "/api/payments/process",
                ["GetPaymentStatus"] = "/api/payments/{paymentId}/status",

                // Inventory Management
                ["GetInventory"] = "/api/inventory",
                ["UpdateInventory"] = "/api/inventory/{productId}",
                ["CheckStock"] = "/api/inventory/{productId}/stock"
            });
        });

        // Set default parameters as needed
        _apiManager.AddDefaultQueryParams(new Dictionary<string, string>
        {
            ["apiKey"] = Environment.GetEnvironmentVariable("SHOP_API_KEY") ?? "dev-key-123",
            ["version"] = "v2.1",
            ["format"] = "json",
            ["locale"] = "zh-CN"
        });

        // Set current environment based on environment variable
        var envVar = Environment.GetEnvironmentVariable("SHOP_ENVIRONMENT") ?? "Development";
        if (Enum.TryParse<ApiEnvironment>(envVar, true, out var apiEnv))
        {
            _apiManager.SwitchEnvironment(apiEnv);
        }

        // Listen for environment switching events
        _apiManager.OnEnvironmentChanged += (sender, args) =>
        {
            Console.WriteLine($"🔄 E-commerce system environment switched from {args.OldEnvironment} to {args.NewEnvironment}");
        };
    }

    // Product-related methods
    public string GetProductsUrl(int page = 1, int limit = 20, string category = "")
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = limit.ToString()
        };

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        return _apiManager.GetEndpoint("GetProducts", null, queryParams);
    }

    public string GetProductUrl(int productId, bool includeReviews = false)
    {
        var templateParams = new { productId };
        var queryParams = includeReviews ? new { includeReviews = "true" } : null;

        return _apiManager.GetEndpoint("GetProduct", templateParams, queryParams);
    }

    public string SearchProductsUrl(string keyword, string category = "")
    {
        var queryParams = new Dictionary<string, string> { ["q"] = keyword };

        if (!string.IsNullOrEmpty(category))
            queryParams["category"] = category;

        return _apiManager.GetEndpoint("SearchProducts", null, queryParams);
    }

    // Order-related methods
    public string GetOrdersUrl(int userId, string status = "")
    {
        var queryParams = new Dictionary<string, string> { ["userId"] = userId.ToString() };

        if (!string.IsNullOrEmpty(status))
            queryParams["status"] = status;

        return _apiManager.GetEndpoint("GetOrders", null, queryParams);
    }

    public string GetOrderUrl(int orderId)
    {
        var templateParams = new { orderId };
        return _apiManager.GetEndpoint("GetOrder", templateParams);
    }

    public string CreateOrderUrl() => _apiManager.GetEndpoint("CreateOrder");

    // Environment management
    public void SwitchToProduction()
    {
        _apiManager.SwitchEnvironment(ApiEnvironment.Production);
    }

    public string GetCurrentBaseUrl()
    {
        return _apiManager.GetCurrentBaseUrl();
    }

    public ApiEnvironment GetCurrentEnvironment()
    {
        return _apiManager.CurrentEnvironment;
    }
}

// Usage Example
internal class Program
{
    internal static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("== E-commerce System API Manager - Complete Example ==");
        // Create e-commerce API manager instance
        var ecommerceApi = new ECommerceApiManager();

        Console.WriteLine($"Current Environment: {ecommerceApi.GetCurrentEnvironment()}");
        Console.WriteLine($"Current Base URL: {ecommerceApi.GetCurrentBaseUrl()}");

        // Get product list URL
        string productsUrl = ecommerceApi.GetProductsUrl(page: 1, limit: 10, category: "electronics");
        Console.WriteLine($"Product List URL: {productsUrl}");

        // Get specific product URL
        string productUrl = ecommerceApi.GetProductUrl(123, includeReviews: true);
        Console.WriteLine($"Product Detail URL: {productUrl}");

        // Get product search URL
        string searchUrl = ecommerceApi.SearchProductsUrl("mobile phone", "electronics");
        Console.WriteLine($"Product Search URL: {searchUrl}");

        // Get user orders URL
        string ordersUrl = ecommerceApi.GetOrdersUrl(456, "completed");
        Console.WriteLine($"User Orders URL: {ordersUrl}");

        // Switch to Production environment
        Console.WriteLine("\nSwitching to Production environment...");
        ecommerceApi.SwitchToProduction();
        Console.WriteLine($"Production Environment Product URL: {ecommerceApi.GetProductUrl(123)}");
        Console.ReadKey();
    }
}
```


## 🤝 Support and Contribution
- 📜 **GitHub License**: This project is licensed under the [MIT License](LICENSE).
- 👤 **Author**: Yu Sheng
- 🌐 **Website**: [https://mindev.cn](https://mindev.cn)
- 💡 **Contribution**: Contributions are welcome! Submit [Issues](https://github.com/mindev99/Min.ApiManager/issues) or [Pull Requests](https://github.com/mindev99/Min.ApiManager/pulls) to help improve the project.

---

**Min.ApiManager** - Making API address management simple and powerful! 🚀这条消息已经在编辑器中准备就绪。你想如何调整这篇文档?请随时告诉我。


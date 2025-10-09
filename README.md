# 🚀 Min.ApiManager

[![NuGet Version](https://img.shields.io/nuget/v/Min.ApiManager.svg)](https://www.nuget.org/packages/Min.ApiManager/)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Min.ApiManager-blue.svg)](https://github.com/mindev99/Min.ApiManager)

💡 一个用于管理 API 地址的轻量级 .NET 类库，支持多环境配置、动态路由管理和智能端点构建。

## 📝 特性

- **🌍 多环境支持** - 支持 Development、Testing、UAT、Staging、Production 等多种环境
- **🔧 灵活配置** - 支持 JSON、XML、INI 三种配置格式
- **🛡️ 线程安全** - 内置锁机制，支持高并发场景
- **📝 模板化路由** - 支持参数化路由模板，如 `/api/users/{userId}`
- **🔗 智能端点构建** - 自动拼接域名、路由和查询参数
- **📡 事件通知** - 支持环境切换、域名变更等事件监听
- **💾 弱引用事件** - 避免内存泄漏，适合长期运行的应用
- **🎯 链式调用** - 流畅的 API 设计，支持方法链
- **📦 轻量级** - 无外部依赖，仅依赖 .NET 标准库

## 📦 安装

### NuGet 包管理器
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

## 🏗️ 项目架构

```
Min.ApiManager/
├── ApiManager.cs                           # 核心管理器类
├── ApiEnvironment.cs                       # 环境枚举定义
├── APIDoc.cs                               # 配置文档模型
├── Event/                                  # 事件相关
│   ├── WeakEvent.cs                        # 弱引用事件实现
│   └── EnvironmentChangedEventArgs.cs      # 环境变化事件
└── Interface/                              # 接口定义
    ├── IApiManager.cs                      # 默认主接口
    ├── IConfigurableManager.cs             # 配置管理接口
    ├── IDomainManager.cs                   # 域名管理接口
    ├── IRouteManager.cs                    # 路由管理接口
    ├── IEndpointBuilder.cs                 # 端点构建接口
    ├── ITemplateManager.cs                 # 模板路由接口
    └── IValidationManager.cs               # 地址验证接口
```

## 🎯 核心概念

### 🌳 环境管理
支持以下环境类型：
* 🧩 **Development**：本地开发环境
* 🧪 **Testing**：测试 / QA 环境
* 🧭 **UAT**：用户验收测试环境
* 🚀 **Staging**：预发布 / 模拟生产环境
* 🧱 **Sandbox**：沙箱环境（隔离实验用途）
* 🏭 **Production**：正式生产环境

### 🔧 配置格式
支持三种配置格式：
* 📄 **JSON**：结构化配置，易于阅读与编辑
* 🗂️ **XML**：传统格式，兼容性强，适合企业级项目
* 🧾 **INI**：轻量级键值对配置，简单直接

#### 🟨 JSON 配置

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

#### 🟥 XML 配置

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApiConfig>
  <!-- 当前环境 -->
  <CurrentEnvironment>Development</CurrentEnvironment>

  <!-- 各环境对应的域名 -->
  <Domains>
    <Domain Environment="Development">https://development.mindev.cn</Domain>
    <Domain Environment="Staging">https://staging.mindev.cn</Domain>
    <Domain Environment="Production">https://production.mindev.cn</Domain>
    <Domain Environment="Sandbox">https://sandbox.mindev.cn/</Domain>
    <Domain Environment="Testing">https://testing.mindev.cn/</Domain>
    <Domain Environment="UAT">https://uat.mindev.cn/</Domain>
  </Domains>

  <!-- API 路由地址 -->
  <Routes>
    <Route Key="GetUser">/user/get</Route>
    <Route Key="UpdateUser">/user/update</Route>
    <Route Key="DeleUser">/uid={uid}</Route>
  </Routes>

  <!-- 默认查询参数 -->
  <DefaultQueryParams>
    <Param Key="apiKey">your-api-key</Param>
    <Param Key="version">v1</Param>
    <Param Key="format">json</Param>
  </DefaultQueryParams>
</ApiConfig>
```

#### 🟩 INI 配置

```ini
; 当前环境配置
[Environment]
; 可选值: Development / Staging / Production / Sandbox / Testing / UAT
CurrentEnvironment=Development

; 各环境对应的域名
[Domains]
Development=https://development.mindev.cn
Staging=https://staging.mindev.cn
Production=https://production.mindev.cn
Sandbox=https://sandbox.mindev.cn/
Testing=https://testing.mindev.cn/
UAT=https://uat.mindev.cn/

; API 路由地址
[Routes]
GetUser=/user/get
UpdateUser=/user/update
DeleUser=/uid={uid}

; 默认查询参数
[DefaultQueryParams]
apiKey=your-api-key
version=v1
format=json
```


## 📖 快速开始

### 1. 基本初始化

> 📝 备注：这里用整体项目全局共享的方式、如果不需要也可以单独创建管理器  ”即 **new**“

#### 📌 单例模式使用

```csharp
// 引入命名空间
using Min.ApiManager;

// 创建静态全局项目 API 管理器实例
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

#### 💻 代码方式初始化

```csharp
// 程序入口点
internal class Program
{
	// Main 方法
	internal static void Main(string[] args)
	{
		// 初始化配置（需要程序入口中）
		APIExamples.ApiManager.Initialize(SetUrlAddress);
	}

	// 回调方法
	private void SetUrlAddress(ApiManager manager)
	{
		// 设置域名（可使用字典进行批量添加）
		manager.SetDomain(ApiEnvironment.Development, "https://dev.mindev.cn");
		manager.SetDomain(ApiEnvironment.Production, "https://api.mindev.com");

		// 设置路由字典
		var routes = new Dictionary<string, string>
		{
			{ "login", "/api/login" },
			{ "register", "/api/register" }
		};

		// 批量设置路由
		manager.SetRoutes(routes);
	}
}
```

#### 📄 文件方式初始化

```csharp
// 程序入口点
internal class Program
{
	// Main 方法
	internal static void Main(string[] args)
	{
	  // JSON 文件路径
        string jsonFilePath = @"C:\Configs\api_config.json";
        try
        {
            // 读取文件内容为字符串
            string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

			// 读取配置文件内容
			APIExamples.ApiManager.LoadConfig(jsonContent, LoadFormat.JSON);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"读取文件失败: {ex.Message}");
        }
	}
}

```

### 2. 构建 API 端点

```csharp
// 获取简单的端点
string endpoint = APIExamples.ApiManager.GetEndpoint("GetUser");
// 结果: https://development.mindev.cn/user/get

// 带模板参数的端点
var templateParams = new { uid = 123 };
string userEndpoint = APIExamples.ApiManager.GetEndpoint("DeleUser", templateParams);
// 结果: https://development.mindev.cn/uid=123

// 带额外查询参数的端点
var queryParams = new { page = 1, limit = 10 };
string usersWithPaging = APIExamples.ApiManager.GetEndpoint("GetUsers", null, queryParams);
// 结果: https://development.mindev.cn/user/get?page=1&limit=10

// 同时使用模板参数和查询参数
string userWithQuery = APIExamples.ApiManager.GetEndpoint("DeleUser", templateParams, queryParams);
// 结果: https://development.mindev.cn/uid=123?page=1&limit=10

```

### 3. 环境切换

```csharp
// 切换到生产环境
APIExamples.ApiManager.SwitchEnvironment(ApiEnvironment.Production);

// 安全切换（不抛异常）
if (APIExamples.ApiManager.TrySwitchEnvironment(ApiEnvironment.Staging, out string? error))
{
    Debug.WriteLine("环境切换成功");
}
else
{
    Debug.WriteLine($"环境切换失败: {error}");
}

// 获取当前环境
ApiEnvironment current = APIExamples.ApiManager.CurrentEnvironment;
```


## ⚙️ 高级功能

### 📡 事件监听

```csharp
// 监听环境切换事件
APIExamples.ApiManager.OnEnvironmentChanged += (sender, args) =>
{
    Debug.WriteLine($"环境从 {args.OldEnvironment} 切换到 {args.NewEnvironment}");
    // 可以在这里执行环境切换后的清理或初始化工作
};

// 监听域名变更事件
APIExamples.ApiManager.OnDomainChanged += (env, newDomain, oldDomain) =>
{
    Debug.WriteLine($"环境 {env} 的域名从 {oldDomain} 更新为 {newDomain}");
};

// 监听路由变更事件
APIExamples.ApiManager.OnRouteChanged += (key, newRoute, oldRoute) =>
{
    Debug.WriteLine($"路由 {key} 从 {oldRoute} 更新为 {newRoute}");
};
```

### ♻️ 动态配置更新

```csharp
// 运行时更新配置
APIExamples.ApiManager.UpdateConfig(manager =>
{
    // 添加新的路由
    manager.SetRoute("NewFeature", "/api/new-feature");
    
    // 更新域名
    manager.SetDomain(ApiEnvironment.Development, "https://new-dev-api.example.com");
    
    // 添加新的查询参数
    manager.AddDefaultQueryParam("debug", "true");
});

// 批量更新路由
var newRoutes = new Dictionary<string, string>
{
    ["Feature1"] = "/api/feature1",
    ["Feature2"] = "/api/feature2",
    ["Feature3"] = "/api/feature3"
};

// 批量设置新路由
APIExamples.ApiManager.SetRoutes(newRoutes);
```

## 🐞 故障排除

### 🤔 常见问题

1. **"请先调用 Initialize() 设置域名" 错误**
   
```csharp
// ❌ 错误用法
var apiManager = new ApiManager();
string url = apiManager.GetEndpoint("GetUser"); // 会抛出异常，因为没有查找到名为”GetUser“的路由，需要在Initialize中进行初始化

// ✔️ 正确用法
var apiManager = new ApiManager();
apiManager.Initialize(manager => {
   manager.SetDomain(ApiEnvironment.Development, "https://api.example.com");
   manager.SetRoute("GetUser", "/api/users/{userId}");
});
string url = apiManager.GetEndpoint("GetUser"); // 正常工作
```

2. **模板参数未完全替换**

```csharp
// 错误：缺少必需的模板参数
string url = apiManager.GetEndpoint("GetUser", new { id = 123 }); // userId 参数缺失

// 正确：提供所有必需的模板参数
string url = apiManager.GetEndpoint("GetUser", new { userId = 123 });
```

3. **环境切换失败**

```csharp
// 检查目标环境是否已配置域名
if (!apiManager.HasDomain(ApiEnvironment.Production))
{
   Console.WriteLine("生产环境域名未配置");
}

// 使用安全切换方法
if (!apiManager.TrySwitchEnvironment(ApiEnvironment.Production, out string error))
{
   Console.WriteLine($"环境切换失败: {error}");
}
```

## 📋 详细使用案例

### 🛠️ 示例 : 完整的电商系统 API 管理

```csharp
/// <summary>
/// 电商系统 API 管理器 - 完整示例
/// 展示如何在实际项目中使用 Min.ApiManager
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
			// 按需配置多环境域名
			manager.SetDomains(new Dictionary<ApiEnvironment, string>
			{
				[ApiEnvironment.Development] = "https://dev-api.shop.com",
				[ApiEnvironment.Staging] = "https://staging-api.shop.com",
				[ApiEnvironment.Production] = "https://api.shop.com",
				[ApiEnvironment.Testing] = "https://test-api.shop.com"
			});

			// 按需配置商品相关路由
			manager.SetRoutes(new Dictionary<string, string>
			{
				// 商品管理
				["GetProducts"] = "/api/products",
				["GetProduct"] = "/api/products/{productId}",
				["CreateProduct"] = "/api/products",
				["SearchProducts"] = "/api/products/search",

				// 订单管理
				["GetOrders"] = "/api/orders",
				["GetOrder"] = "/api/orders/{orderId}",
				["CreateOrder"] = "/api/orders",
				["UpdateOrderStatus"] = "/api/orders/{orderId}/status",

				// 用户管理
				["GetUsers"] = "/api/users",
				["GetUser"] = "/api/users/{userId}",
				["CreateUser"] = "/api/users",
				["UpdateUser"] = "/api/users/{userId}",

				// 支付管理
				["ProcessPayment"] = "/api/payments/process",
				["GetPaymentStatus"] = "/api/payments/{paymentId}/status",

				// 库存管理
				["GetInventory"] = "/api/inventory",
				["UpdateInventory"] = "/api/inventory/{productId}",
				["CheckStock"] = "/api/inventory/{productId}/stock"
			});

		});

		// 按需设置默认参数
		_apiManager.AddDefaultQueryParams(new Dictionary<string, string>
		{
			["apiKey"] = Environment.GetEnvironmentVariable("SHOP_API_KEY") ?? "dev-key-123",
			["version"] = "v2.1",
			["format"] = "json",
			["locale"] = "zh-CN"
		});

		// 根据环境变量设置当前环境
		var env = Environment.GetEnvironmentVariable("SHOP_ENVIRONMENT") ?? "Development";
		if (Enum.TryParse<ApiEnvironment>(env, true, out var apiEnv))
		{
			_apiManager.SwitchEnvironment(apiEnv);
		}


		// 监听环境切换事件
		_apiManager.OnEnvironmentChanged += (sender, args) =>
		{
			Console.WriteLine($"🔄 电商系统环境已从 {args.OldEnvironment} 切换到 {args.NewEnvironment}");
		};
	}

	// 商品相关方法
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

	// 订单相关方法
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

	// 环境管理
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


// 使用示例
internal class Program
{
	internal static void Main(string[] args)
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		Console.WriteLine("== 电商系统 API 管理器 - 完整示例 ==");
		// 创建电商 API 管理器
		var ecommerceApi = new ECommerceApiManager();

		Console.WriteLine($"当前环境: {ecommerceApi.GetCurrentEnvironment()}");
		Console.WriteLine($"当前基础URL: {ecommerceApi.GetCurrentBaseUrl()}");

		// 获取商品列表
		string productsUrl = ecommerceApi.GetProductsUrl(page: 1, limit: 10, category: "electronics");
		Console.WriteLine($"商品列表URL: {productsUrl}");

		// 获取特定商品
		string productUrl = ecommerceApi.GetProductUrl(123, includeReviews: true);
		Console.WriteLine($"商品详情URL: {productUrl}");

		// 搜索商品
		string searchUrl = ecommerceApi.SearchProductsUrl("手机", "electronics");
		Console.WriteLine($"商品搜索URL: {searchUrl}");

		// 获取用户订单
		string ordersUrl = ecommerceApi.GetOrdersUrl(456, "completed");
		Console.WriteLine($"用户订单URL: {ordersUrl}");

		// 切换到生产环境
		Console.WriteLine("\n切换到生产环境...");
		ecommerceApi.SwitchToProduction();
		Console.WriteLine($"生产环境商品URL: {ecommerceApi.GetProductUrl(123)}");
		Console.ReadKey();
	}
}
```


## 🤝 支持与贡献

- 📜 **许可证**：本项目采用 [MIT 许可证](LICENSE)。
- 👤 **作者**：于晟
- 🌐 **网站**：[https://mindev.cn](https://mindev.cn)
- 💡 **贡献**：欢迎提交 [Lssue](https://github.com/mindev99/Min.ApiManager/issues) 和 [Pull Request](https://github.com/mindev99/Min.ApiManager/pulls)，共同改进项目。

---

**Min.ApiManager** - 让 API 地址管理变得简单而强大！ 🚀
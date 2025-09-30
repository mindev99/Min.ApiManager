namespace Min.ApiManager;

/// <summary>
/// 默认的 API 管理器实现。
/// <para>
/// 提供统一的域名管理、路由注册、端点构建和环境切换功能，
/// 适合在项目中作为全局 API 地址管理工具使用。
/// </para>
/// <list type="bullet">
/// <item>环境切换（Development、Staging、Production 等）</item>
/// <item>域名动态管理（添加、更新、删除、批量设置）</item>
/// <item>路由管理（添加、更新、删除、批量设置）</item>
/// <item>接口地址构建（支持模板参数与查询参数）</item>
/// <item>事件通知（环境、域名、路由变更）</item>
/// </list>
/// <para>
/// 该类采用 **全局单例模式**（通过 <see cref="Instance"/> 属性访问），开发者只需在项目初始化时配置一次域名和路由，即可在应用程序全局调用。
/// </para>
/// </summary>
public class ApiManager : Interface.IApiManager<ApiManager>
{
    #region ==== 构造 ====

    /// <summary>
    /// 私有构造函数，禁止外部创建实例。
    /// </summary>
    public ApiManager() { }

    #endregion

    #region ==== 线程安全锁 ==== 

    /// <summary>
    /// 域名字典操作锁，用于保证在多线程下对 <see cref="_domains"/> 的读写安全。
    /// </summary>
    private readonly object _domainLock = new();

    /// <summary>
    /// 路由字典操作锁，用于保证在多线程下对 <see cref="_routes"/> 的读写安全。
    /// </summary>
    private readonly object _routeLock = new();

    /// <summary>
    /// 默认查询参数字典操作锁，用于保证在多线程下对 <see cref="_defaultQueryParams"/> 的读写安全。
    /// </summary>
    private readonly object _queryLock = new();

    /// <summary>
    /// 当前环境切换锁，用于保证在多线程下对 <see cref="_currentEnvironment"/> 的读写和切换安全。
    /// </summary>
    private readonly object _envLock = new();

    #endregion

    #region ==== 字段 ====

    /// <summary>
    /// 标记 <see cref="ApiManager"/> 是否已经完成初始化。
    /// 用于确保在调用依赖域名或路由的方法之前，先执行 <see cref="Initialize(Action{ApiManager})"/>。
    /// </summary>
    private bool _initialized = false;

    /// <summary>
    /// 当前运行环境
    /// </summary>
    private ApiEnvironment _currentEnvironment;

    /// <summary>
    /// 环境到域名映射
    /// </summary>
    private readonly Dictionary<ApiEnvironment, string> _domains = new();

    /// <summary>
    /// 路由 Key 到路径映射
    /// </summary>
    private readonly Dictionary<string, string> _routes = new();

    /// <summary>
    /// 默认查询参数
    /// </summary>
    private readonly Dictionary<string, string> _defaultQueryParams = new();

    #endregion

    #region ==== 属性 ====

    /// <inheritdoc />    
    public ApiEnvironment CurrentEnvironment
    {
        get
        {
            lock (_envLock)
            {
                return _currentEnvironment;
            }
        }
    }

    #endregion

    #region ==== 事件 ====

    // ==== 弱事件字段 ====
    private readonly Event.WeakEvent<EventHandler<Event.EnvironmentChangedEventArgs>> _onEnvironmentChanged = new();
    private readonly Event.WeakEvent<Action<ApiEnvironment, string?, string?>> _onDomainChanged = new();
    private readonly Event.WeakEvent<Action<string, string?, string?>> _onRouteChanged = new();
    private static readonly string[] separator = new[] { "\r\n", "\n" };

    /// <inheritdoc />    
    public event EventHandler<Event.EnvironmentChangedEventArgs>? OnEnvironmentChanged
    {
        add => _onEnvironmentChanged.AddHandler(value);
        remove => _onEnvironmentChanged.RemoveHandler(value);
    }

    /// <inheritdoc />    
    public event Action<ApiEnvironment, string?, string?>? OnDomainChanged
    {
        add => _onDomainChanged.AddHandler(value);
        remove => _onDomainChanged.RemoveHandler(value);
    }

    /// <inheritdoc />    
    public event Action<string, string?, string?>? OnRouteChanged
    {
        add => _onRouteChanged.AddHandler(value);
        remove => _onRouteChanged.RemoveHandler(value);
    }

    #endregion

    #region ==== 初始化与配置 ====

    /// <inheritdoc />    
    public virtual ApiManager Initialize(Action<ApiManager> initializer)
    {
        if (_initialized) return this;

        // 用户自定义初始化回调,字典已在构造函数初始化，这里只做回调和标记
        initializer?.Invoke(this);
        _initialized = true;
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager UpdateConfig(Action<ApiManager> updater)
    {
        updater?.Invoke(this);
        return this;
    }

    /// <inheritdoc />    
    public ApiManager LoadConfig(string config, LoadFormat format)
    {
        if (string.IsNullOrWhiteSpace(config))
            throw new ArgumentNullException(nameof(config), "配置内容不能为空");

        APIDoc doc = format switch
        {
            LoadFormat.JSON => ParseJson(config),
            LoadFormat.XML => ParseXml(config),
            LoadFormat.INI => ParseIni(config),
            _ => throw new NotSupportedException($"不支持的配置格式: {format}")
        };

        ApplyConfig(doc);

        _initialized = true;
        return this;
    }

    /// <inheritdoc />    
    public virtual string DumpConfig()
    {
        return $"Environment: {_currentEnvironment}, Domains: [{string.Join(", ", _domains.Select(kv => $"{kv.Key}:{kv.Value}"))}], Routes: [{string.Join(", ", _routes.Select(kv => $"{kv.Key}:{kv.Value}"))}]";
    }

    #endregion

    #region ==== 环境切换 ====

    /// <inheritdoc />    
    public virtual ApiManager SwitchEnvironment(ApiEnvironment environment)
    {
        this.EnsureDomainsInitialized(nameof(SwitchEnvironment));

        ApiEnvironment old;
        lock (_envLock)
        {
            if (environment == _currentEnvironment) return this;
            if (!_domains.ContainsKey(environment))
                throw new InvalidOperationException($"目标环境 {environment} 未配置有效域名");
            old = _currentEnvironment;
            _currentEnvironment = environment;
        }
        RaiseEnvironmentChanged(new Event.EnvironmentChangedEventArgs(old, environment));
        return this;
    }

    /// <inheritdoc />    
    public virtual bool TrySwitchEnvironment(ApiEnvironment environment, out string? errorMessage)
    {
        errorMessage = null;
        try
        {
            SwitchEnvironment(environment);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    #endregion

    #region ==== 域名管理 ====

    /// <inheritdoc />    
    public virtual ApiManager SetDomain(ApiEnvironment environment, string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentNullException(nameof(domain), "域名不能为空");
        string? old;
        lock (_domainLock)
        {
            _domains.TryGetValue(environment, out old);
            _domains[environment] = domain;
        }
        // 事件放在锁外触发，避免死锁
        RaiseDomainChanged(environment, domain, old);
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager SetDomains(Dictionary<ApiEnvironment, string> domains)
    {
        if (domains == null || domains.Count == 0) return this;

        // 保存变化信息，事件用
        var changes = new List<(ApiEnvironment Env, string New, string? Old)>();

        lock (_domainLock)
        {
            foreach (var kv in domains)
            {
                _domains.TryGetValue(kv.Key, out var old);
                _domains[kv.Key] = kv.Value;
                changes.Add((kv.Key, kv.Value, old));
            }
        }

        // 事件放在锁外触发
        var handler = RaiseDomainChanged;
        if (handler != null)
        {
            foreach (var (Env, New, Old) in changes)
            {
                handler.Invoke(Env, New, Old);
            }
        }

        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager RemoveDomain(ApiEnvironment environment)
    {
        string? old;
        lock (_domainLock)
        {
            if (!_domains.TryGetValue(environment, out old)) return this;
            _domains.Remove(environment);
        }
        RaiseDomainChanged(environment, null, old);
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager ClearDomains()
    {
        List<(ApiEnvironment Env, string Old)> removed;

        lock (_domainLock)
        {
            removed = _domains.Select(kv => (kv.Key, kv.Value)).ToList();
            _domains.Clear();
        }

        foreach (var (Env, Old) in removed)
        {
            RaiseDomainChanged(Env, null, Old);
        }

        return this;
    }

    /// <inheritdoc />    
    public virtual string GetBaseUrl(ApiEnvironment environment)
    {
        this.EnsureDomainsInitialized(nameof(GetBaseUrl));

        lock (_domainLock)
        {
            if (!_domains.TryGetValue(environment, out var url))
                throw new KeyNotFoundException($"环境 {environment} 未配置域名");
            return url;
        }
    }

    /// <inheritdoc />    
    public virtual string GetCurrentBaseUrl()
    {
        this.EnsureDomainsInitialized(nameof(GetCurrentBaseUrl));

        ApiEnvironment env;
        lock (_envLock)
        {
            env = _currentEnvironment;
        }

        lock (_domainLock)
        {
            if (!_domains.TryGetValue(env, out var url))
                throw new KeyNotFoundException($"环境 {env} 未配置域名");

            return url;
        }
    }

    /// <inheritdoc />    
    public virtual bool HasDomain(ApiEnvironment environment)
    {
        lock (_domainLock)
        {
            return _domains.ContainsKey(environment);
        }
    }

    /// <summary>
    /// <inheritdoc />    
    /// </summary>
    /// <returns>
    /// 环境到域名映射字典
    /// </returns>
    public virtual IReadOnlyDictionary<ApiEnvironment, string> GetAllDomains()
    {
        lock (_domainLock)
        {
            return new Dictionary<ApiEnvironment, string>(_domains);
        }
    }

    #endregion

    #region ==== 路由管理 ====

    /// <inheritdoc />    
    public virtual ApiManager SetRoute(string key, string route)
    {
        string? old;
        lock (_routeLock)
        {
            _routes.TryGetValue(key, out old);
            _routes[key] = route;
        }
        RaiseRouteChanged(key, route, old);
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager SetRoutes(Dictionary<string, string> routes)
    {
        if (routes == null || routes.Count == 0) return this;

        var changes = new List<(string Key, string New, string? Old)>();

        lock (_routeLock)
        {
            foreach (var kv in routes)
            {
                _routes.TryGetValue(kv.Key, out var old);
                _routes[kv.Key] = kv.Value;
                changes.Add((kv.Key, kv.Value, old));
            }
        }

        var handler = RaiseRouteChanged;

        if (handler != null)
        {
            foreach (var (Key, New, Old) in changes)
                handler.Invoke(Key, New, Old);
        }

        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager RemoveRoute(string key)
    {
        string? old;
        lock (_routeLock)
        {
            if (!_routes.TryGetValue(key, out old)) return this;
            _routes.Remove(key);
        }
        RaiseRouteChanged(key, null, old);
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager ClearRoutes()
    {
        List<(string Key, string Old)> removed;

        lock (_routeLock)
        {
            removed = _routes.Select(kv => (kv.Key, kv.Value)).ToList();
            _routes.Clear();
        }

        foreach (var (Key, Old) in removed)
        {
            RaiseRouteChanged(Key, null!, Old);
        }

        return this;
    }

    /// <inheritdoc />    
    public virtual string? GetRoute(string routeKey)
    {
        this.EnsureRoutesInitialized(nameof(GetRoute));

        lock (_routeLock)
        {
            if (!_routes.TryGetValue(routeKey, out var route))
                throw new KeyNotFoundException("路由 {routeKey} 不存在");
            return route;
        }
    }

    /// <inheritdoc />    
    public virtual bool HasRoute(string routeKey)
    {
        lock (_routeLock)
        {
            return _routes.ContainsKey(routeKey);
        }
    }

    /// <summary>
    /// <inheritdoc />    
    /// </summary>
    /// <returns>
    /// 路由 Key 到路径映射字典 
    /// </returns>
    public virtual IReadOnlyDictionary<string, string> GetAllRoutes()
    {
        lock (_routeLock)
        {
            return new Dictionary<string, string>(_routes);
        }
    }

    #endregion

    #region ==== 接口构建 ====

    /// <inheritdoc />    
    public virtual string GetEndpoint(string routeKey)
    {
        if (string.IsNullOrEmpty(routeKey))
            throw new ArgumentException("路由 Key 不能为空", nameof(routeKey));

        this.EnsureRoutesInitialized(nameof(GetEndpoint));
        this.EnsureDomainsInitialized(nameof(GetEndpoint));

        lock (_routeLock)
        {
            if (!_routes.TryGetValue(routeKey, out var route))
                throw new KeyNotFoundException($"路由 {routeKey} 不存在");

            return $"{GetCurrentBaseUrl().TrimEnd('/')}/{route.TrimStart('/')}";
        }
    }

    /// <inheritdoc />    
    public virtual string GetEndpoint(string routeKey, object? templateParams = null, object? queryParams = null)
    {
        var url = GetEndpoint(routeKey);

        // 模板参数替换,大小写安全
        if (templateParams != null)
        {
            if (templateParams is Dictionary<string, object?> dictOjb)
            {
                foreach (var kv in dictOjb)
                {
                    var pattern = @"\{" + System.Text.RegularExpressions.Regex.Escape(kv.Key) + @"\}";
                    url = System.Text.RegularExpressions.Regex.Replace(url, pattern, Uri.EscapeDataString(kv.Value?.ToString() ?? ""), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            else if (templateParams is Dictionary<string, string> dict)
            {
                foreach (var kv in dict)
                {
                    var pattern = @"\{" + System.Text.RegularExpressions.Regex.Escape(kv.Key) + @"\}";
                    url = System.Text.RegularExpressions.Regex.Replace(url, pattern, Uri.EscapeDataString(kv.Value?.ToString() ?? ""), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            else
            {
                // 普通对象
                var props = templateParams.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var pattern = @"\{" + System.Text.RegularExpressions.Regex.Escape(prop.Name) + @"\}";
                    var raw = prop.GetValue(templateParams)?.ToString() ?? string.Empty;
                    url = System.Text.RegularExpressions.Regex.Replace(url, pattern, Uri.EscapeDataString(raw), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
        }

        // 模板未完全替换检测，检查是否有未替换的占位符
        var leftover = System.Text.RegularExpressions.Regex.Matches(url, @"\{([^{}]+)\}");
        if (leftover.Count > 0)
        {
            var missing = leftover.Cast<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).Distinct();
            throw new InvalidOperationException($"[{nameof(GetEndpoint)}] URL 模板未完全替换，缺失参数: {string.Join(", ", missing)}，模板结果: {url}");
        }

        // 合并默认查询参数和传入查询参数
        Dictionary<string, string> allQuery;
        lock (_queryLock)
        {
            allQuery = new Dictionary<string, string>(_defaultQueryParams);
        }
        if (queryParams != null)
        {
            // 🚀 支持字典和匿名对象
            if (queryParams is Dictionary<string, object?> qdictObj)
            {
                foreach (var kv in qdictObj)
                {
                    allQuery[kv.Key] = kv.Value?.ToString() ?? "";
                }
            }
            else if (queryParams is Dictionary<string, string?> qdict)
            {
                foreach (var kv in qdict)
                {
                    allQuery[kv.Key] = kv.Value?.ToString() ?? "";
                }
            }
            else
            {
                foreach (var prop in queryParams.GetType().GetProperties())
                {
                    var value = prop.GetValue(queryParams)?.ToString() ?? "";
                    allQuery[prop.Name] = value;
                }
            }
        }

        if (allQuery.Count > 0)
        {
            url += "?" + string.Join("&", allQuery.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        }

        return Uri.UnescapeDataString(url);
    }

    /// <inheritdoc />    
    public virtual string GetEndpointTemplate(string routeKey, object templateParams)
    {
        return GetEndpoint(routeKey, templateParams, null);
    }

    /// <inheritdoc />    
    public virtual ApiManager AddDefaultQueryParam(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("参数 Key 不能为空", nameof(key));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("参数 Key 不能为空", nameof(value));

        lock (_queryLock)
        {
            _defaultQueryParams[key] = value;
        }
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager AddDefaultQueryParams(Dictionary<string, string> queryParams)
    {
        if (queryParams == null || queryParams.Count == 0) return this;

        lock (_queryLock)
        {
            foreach (var kv in queryParams)
            {
                _defaultQueryParams[kv.Key] = kv.Value;
            }
        }
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager RemoveDefaultQueryParam(string key)
    {
        lock (_queryLock)
        {
            _defaultQueryParams.Remove(key);
        }
        return this;
    }

    /// <inheritdoc />    
    public virtual ApiManager ClearDefaultQueryParams()
    {
        lock (_queryLock)
        {
            _defaultQueryParams.Clear();
        }
        return this;
    }

    /// <inheritdoc />    
    public virtual IReadOnlyDictionary<string, string> GetDefaultQueryParams()
    {
        lock (_queryLock)
        {
            return new Dictionary<string, string>(_defaultQueryParams);
        }
    }

    #endregion

    #region ==== 安全检查 ====

    /// <summary>
    /// 确保域名初始化，若为空则抛出异常提示。
    /// </summary>
    private void EnsureDomainsInitialized(string method)
    {
        if (!_initialized || _domains.Count == 0)
            throw new InvalidOperationException($"[{method}] 请先调用 Initialize() / LoadConfig() 设置域名");
    }

    /// <summary>
    /// 确保路由字典已初始化，若为空则抛出异常提示。
    /// </summary>
    private void EnsureRoutesInitialized(string method)
    {
        if (!_initialized || _routes.Count == 0)
            throw new InvalidOperationException($"[{method}] 请先调用 Initialize() / LoadConfig() 并设置路由");
    }

    #endregion

    #region ==== 事件触发 ====

    /// <summary>
    /// 触发环境变更事件 <see cref="_onEnvironmentChanged"/>，通知所有订阅者当前环境已改变。
    /// </summary>
    /// <param name="args">包含旧环境和新环境信息的事件参数。</param>
    private void RaiseEnvironmentChanged(Event.EnvironmentChangedEventArgs args)
    {
        _onEnvironmentChanged.Invoke(h => h.Invoke(this, args));
    }

    /// <summary>
    /// 触发域名变更事件 <see cref="_onDomainChanged"/>，通知所有订阅者指定环境的域名已更新。
    /// </summary>
    /// <param name="env">发生变更的环境。</param>
    /// <param name="newDomain">新域名。</param>
    /// <param name="oldDomain">旧域名，如果没有旧域名则为 <c>null</c>。</param>
    private void RaiseDomainChanged(ApiEnvironment env, string? newDomain, string? oldDomain)
    {
        _onDomainChanged.Invoke(h => h.Invoke(env, newDomain, oldDomain));
    }

    /// <summary>
    /// 触发路由变更事件 <see cref="_onRouteChanged"/>，通知所有订阅者指定路由键对应的路径已更新。
    /// </summary>
    /// <param name="key">发生变更的路由键。</param>
    /// <param name="newRoute">新的路由路径。</param>
    /// <param name="oldRoute">旧的路由路径，如果没有旧路径则为 <c>null</c>。</param>
    private void RaiseRouteChanged(string key, string? newRoute, string? oldRoute)
    {
        _onRouteChanged.Invoke(h => h.Invoke(key, newRoute, oldRoute));
    }

    #endregion

    #region ==== 处理文本 ====

    /// <summary>
    /// 解析 JSON 配置字符串到 <see cref="APIDoc"/> 对象。
    /// </summary>
    private APIDoc ParseJson(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<APIDoc>(json) ?? throw new InvalidOperationException("JSON 配置解析失败");
    }

    /// <summary>
    /// 解析 XML 配置字符串到 <see cref="APIDoc"/> 对象。
    /// </summary>
    private APIDoc ParseXml(string xml)
    {

        var doc = System.Xml.Linq.XDocument.Parse(xml);
        var apiDoc = new APIDoc
        {
            CurrentEnvironment = doc.Root?.Element("CurrentEnvironment")?.Value ?? string.Empty,
            Domains = doc.Root?
                .Element("Domains")?
                .Elements("Domain")
                .ToDictionary(
                    x => Enum.Parse<ApiEnvironment>(x.Attribute("Environment")!.Value),
                    x => x.Value
                ),
            Routes = doc.Root?
                .Element("Routes")?
                .Elements("Route")
                .ToDictionary(
                    x => x.Attribute("Key")!.Value,
                    x => x.Value
                ),
            DefaultQueryParams = doc.Root?
                .Element("DefaultQueryParams")?
                .Elements("Param")
                .ToDictionary(
                    x => x.Attribute("Key")!.Value,
                    x => x.Value
                )
        };

        return apiDoc;
    }

    /// <summary>
    /// 解析 INI 配置字符串到 <see cref="APIDoc"/> 对象。
    /// </summary>
    private APIDoc ParseIni(string ini)
    {
        var doc = new APIDoc
        {
            Domains = [],
            Routes = [],
            DefaultQueryParams = []
        };

        string? currentSection = null;
        foreach (var line in ini.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(';')) continue;

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed[1..^1].Trim();
                continue;
            }

            var kv = trimmed.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim();
            var value = kv[1].Trim();

            switch (currentSection)
            {
                case "Domains":
                    if (Enum.TryParse<ApiEnvironment>(key, true, out var env))
                        doc.Domains![env] = value;
                    break;
                case "Routes":
                    doc.Routes![key] = value;
                    break;
                case "DefaultQueryParams":
                    doc.DefaultQueryParams![key] = value;
                    break;
                case null:
                case "Environment":
                    if (key.Equals("CurrentEnvironment", StringComparison.OrdinalIgnoreCase))
                        doc.CurrentEnvironment = value;
                    break;
            }
        }

        return doc;
    }

    /// <summary>
    /// 将 <see cref="APIDoc"/> 应用到当前 ApiManager 实例。
    /// </summary>
    private void ApplyConfig(APIDoc doc)
    {
        if (!string.IsNullOrWhiteSpace(doc.CurrentEnvironment) && Enum.TryParse<ApiEnvironment>(doc.CurrentEnvironment, true, out var currentEnv))
        {
            _currentEnvironment = currentEnv;
        }

        if (doc.Domains != null)
        {
            lock (_domainLock)
            {
                _domains.Clear();
                foreach (var kv in doc.Domains)
                    _domains[kv.Key] = kv.Value;
            }
        }

        if (doc.Routes != null)
        {
            lock (_routeLock)
            {
                _routes.Clear();
                foreach (var kv in doc.Routes)
                    _routes[kv.Key] = kv.Value;
            }
        }

        if (doc.DefaultQueryParams != null)
        {
            lock (_queryLock)
            {
                _defaultQueryParams.Clear();
                foreach (var kv in doc.DefaultQueryParams)
                    _defaultQueryParams[kv.Key] = kv.Value;
            }
        }
    }

    #endregion

}












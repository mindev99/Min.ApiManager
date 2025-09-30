namespace Min.ApiManager;

/// <summary>
/// 配置文档类，用于统一存储从 JSON / XML / INI 配置文件解析后的结果。
/// 该类作为中间对象，便于将解析后的配置应用到 <see cref="ApiManager"/> 实例中。
/// </summary>
internal class APIDoc
{
    /// <summary>
    /// 当前运行环境的名称，用于初始化 <see cref="ApiManager.CurrentEnvironment"/>。
    /// 解析时会尝试将字符串转换为 <see cref="ApiEnvironment"/> 枚举值。
    /// </summary>
    public string CurrentEnvironment { get; set; } = string.Empty;

    /// <summary>
    /// 环境到域名的映射字典，用于初始化 <see cref="ApiManager"/> 中的 <see cref="_domains"/> 字段。
    /// 例如：
    /// <code>
    /// {
    ///     ApiEnvironment.Dev: "https://dev.example.com",
    ///     ApiEnvironment.Prod: "https://prod.example.com"
    /// }
    /// </code>
    /// </summary>
    public Dictionary<ApiEnvironment, string>? Domains { get; set; }

    /// <summary>
    /// 路由 Key 到路径的映射字典，用于初始化 <see cref="ApiManager"/> 中的 <see cref="_routes"/> 字段。
    /// 例如：
    /// <code>
    /// {
    ///     "GetUser": "/api/user/get",
    ///     "UpdateUser": "/api/user/update"
    /// }
    /// </code>
    /// </summary>
    public Dictionary<string, string>? Routes { get; set; }

    /// <summary>
    /// 默认查询参数字典，用于初始化 <see cref="ApiManager"/> 中的 <see cref="_defaultQueryParams"/> 字段。
    /// 这些参数会在调用 <see cref="ApiManager.GetEndpoint(string, object?, object?)"/> 时自动合并到 URL 查询字符串中。
    /// 例如：
    /// <code>
    /// {
    ///     "apiKey": "123456",
    ///     "locale": "zh-CN"
    /// }
    /// </code>
    /// </summary>
    public Dictionary<string, string>? DefaultQueryParams { get; set; }
}

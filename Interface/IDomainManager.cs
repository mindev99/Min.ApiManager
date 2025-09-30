namespace Min.ApiManager.Interface;

/// <summary>
/// 管理不同运行环境下的 API 域名配置（如开发环境、测试环境、生产环境等）。
/// <para>
/// 主要职责：
/// <list type="bullet">
///   <item><description>为不同的 <see cref="ApiEnvironment"/> 设置基础域名。</description></item>
///   <item><description>支持单个环境域名的添加、批量添加。</description></item>
///   <item><description>支持移除指定环境或清空所有域名配置。</description></item>
///   <item><description>根据环境枚举获取对应的域名。</description></item>
/// </list>
/// </para>
/// <para>
/// 注意：
/// <list type="bullet">
///   <item><description>接口只定义行为，不暴露内部数据结构（如字典）。</description></item>
///   <item><description>实际存储和并发处理由实现类负责。</description></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于支持链式调用（Fluent API）。</typeparam>
/// <typeparam name="TSelf">实现类自身类型，便于链式调用返回具体类型。</typeparam>
public interface IDomainManager<TSelf> where TSelf : IDomainManager<TSelf>
{
    /// <summary>
    /// 当域名变更时触发的事件。
    /// </summary>
    event Action<ApiEnvironment, string?, string?> OnDomainChanged;

    /// <summary>
    /// 设置或更新指定环境的域名。
    /// </summary>
    /// <param name="environment">环境枚举，例如 Dev、Test、Prod。</param>
    /// <param name="domain">域名字符串，例如 "https://api.example.com"。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf SetDomain(ApiEnvironment environment, string domain);

    /// <summary>
    /// 批量设置多个环境的域名。
    /// </summary>
    /// <param name="domains">键为环境枚举，值为对应的域名。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf SetDomains(Dictionary<ApiEnvironment, string> domains);

    /// <summary>
    /// 移除指定环境的域名配置。
    /// </summary>
    /// <param name="environment">要移除的环境枚举。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf RemoveDomain(ApiEnvironment environment);

    /// <summary>
    /// 清空所有已设置的域名配置。
    /// </summary>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf ClearDomains();

    /// <summary>
    /// 获取指定环境的基础域名。
    /// </summary>
    /// <param name="environment">环境枚举。</param>
    /// <returns>对应的域名字符串。</returns>
    /// <exception cref="KeyNotFoundException">当指定环境未配置域名时抛出。</exception>
    string GetBaseUrl(ApiEnvironment environment);

    /// <summary>
    /// 获取当前运行环境的域名。
    /// </summary>
    /// <returns>当前环境的域名字符串</returns>
    string GetCurrentBaseUrl();

    /// <summary>
    /// 判断指定环境是否存在域名配置。
    /// </summary>
    /// <returns>如果存在返回 true，否则 false</returns>
    bool HasDomain(ApiEnvironment environment);

    /// <summary>
    /// 获取所有环境及域名，只读访问，用于调试或日志。
    /// </summary>
    IReadOnlyDictionary<ApiEnvironment, string> GetAllDomains();
}
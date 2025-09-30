namespace Min.ApiManager.Interface;

/// <summary>
/// 管理 API 路由配置（Key → Path 模板），支持模板化占位符（如 <c>{userId}</c>）。
/// <para>
/// 主要职责：
/// <list type="bullet">
///   <item><description>根据 Key 管理路由字符串。</description></item>
///   <item><description>支持单个/批量设置路由。</description></item>
///   <item><description>支持移除指定路由或清空所有路由。</description></item>
///   <item><description>根据 Key 获取对应的路由字符串。</description></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于支持链式调用。</typeparam>
public interface IRouteManager<TSelf> where TSelf : IRouteManager<TSelf>
{
    /// <summary>
    /// 当路由变更时触发的事件。
    /// <para>
    /// 参数分别为：路由 key、新路由（可能为 null）、旧路由（可能为 null）
    /// </para>
    /// </summary>
    event Action<string, string?, string?> OnRouteChanged;

    /// <summary>
    /// 设置或更新指定 Key 的路由。
    /// </summary>
    /// <param name="key">路由 Key，例如 "GetUser"。</param>
    /// <param name="route">路由字符串，例如 "/api/user/{userId}"。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf SetRoute(string key, string route);

    /// <summary>
    /// 批量设置多个路由。
    /// </summary>
    /// <param name="routes">键为路由 Key，值为路由字符串。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf SetRoutes(Dictionary<string, string> routes);

    /// <summary>
    /// 移除指定 Key 的路由。
    /// </summary>
    /// <param name="key">要移除的路由 Key。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf RemoveRoute(string key);

    /// <summary>
    /// 清空所有路由配置。
    /// </summary>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf ClearRoutes();

    /// <summary>
    /// 获取指定 Key 对应的路由。
    /// </summary>
    /// <param name="routeKey">路由 Key。</param>
    /// <returns>对应的路由字符串。</returns>
    /// <exception cref="KeyNotFoundException">当指定 Key 未找到时抛出。</exception>
    string? GetRoute(string routeKey);

    /// <summary>
    /// 检查指定路由 key 是否已存在。
    /// </summary>
    /// <param name="routeKey">路由名称</param>
    /// <returns>如果存在返回 true，否则 false</returns>
    bool HasRoute(string routeKey);

    /// <summary>
    /// 获取所有路由，只读访问，用于调试或日志。
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllRoutes();
}




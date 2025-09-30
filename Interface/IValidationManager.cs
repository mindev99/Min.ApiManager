namespace Min.ApiManager.Interface;

/// <summary>
/// 提供路由和域名验证功能，确保配置合法，减少运行时错误。
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于支持链式调用</typeparam>
public interface IValidationManager<TSelf> where TSelf : IValidationManager<TSelf>
{
    /// <summary>
    /// 验证指定路由 key 是否存在且合法。
    /// </summary>
    /// <param name="routeKey">路由 Key</param>
    /// <returns>返回 true 表示有效，否则 false</returns>
    bool ValidateRoute(string routeKey);

    /// <summary>
    /// 验证指定环境的域名是否存在且有效。
    /// </summary>
    /// <param name="environment">目标环境</param>
    /// <returns>返回 true 表示有效，否则 false</returns>
    bool ValidateDomain(ApiEnvironment environment);
}

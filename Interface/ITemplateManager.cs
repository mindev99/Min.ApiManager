namespace Min.ApiManager.Interface;

/// <summary>
/// 提供模板化路由的增强功能，用于替换占位符并支持复杂 URL 构建。
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于支持链式调用</typeparam>
public interface ITemplateManager<TSelf> where TSelf : ITemplateManager<TSelf>
{
    /// <summary>
    /// 构建模板化路由地址，将路由中的占位符 {key} 替换为指定对象属性值。
    /// </summary>
    /// <param name="routeKey">路由 Key</param>
    /// <param name="templateParams">模板参数对象，属性名对应路由中的占位符</param>
    /// <returns>模板替换后的完整 URL</returns>
    string BuildTemplate(string routeKey, object templateParams);

    /// <summary>
    /// 构建模板化路由地址并附加查询参数。
    /// </summary>
    /// <param name="routeKey">路由 Key</param>
    /// <param name="templateParams">模板参数对象</param>
    /// <param name="queryParams">查询参数对象，用于生成 ?key=value</param>
    /// <returns>完整 URL</returns>
    string BuildTemplateWithQuery(string routeKey, object templateParams, object queryParams);
}

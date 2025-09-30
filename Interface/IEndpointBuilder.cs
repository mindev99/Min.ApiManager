namespace Min.ApiManager.Interface;

/// <summary>
/// 构建最终 API 地址（支持模板化参数替换、查询参数拼接、默认参数、签名等扩展）。
/// <para>
/// 使用场景：
/// <list type="bullet">
///   <item><description>拼接基础 URL（域名 + 路由）。</description></item>
///   <item><description>替换路由中的占位符参数，例如 <c>{userId}</c>。</description></item>
///   <item><description>生成带查询参数的完整地址，例如 <c>?page=1&amp;size=20</c>。</description></item>
///   <item><description>添加默认查询参数（如 API Key、版本号）。</description></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于支持链式调用。</typeparam>
public interface IEndpointBuilder<TSelf> where TSelf : IEndpointBuilder<TSelf>
{
    /// <summary>
    /// 获取指定 Key 的完整接口地址（仅拼接域名和路由，不带模板替换和查询参数）。
    /// </summary>
    /// <param name="routeKey">路由 Key。</param>
    /// <returns>完整接口地址。</returns>
    string GetEndpoint(string routeKey);

    /// <summary>
    /// 根据路由键生成完整的 API 地址，支持模板参数与查询参数替换。
    /// </summary>
    /// <param name="routeKey">路由键，例如 "GetUserById"。 </param>
    /// <param name="templateParams">
    /// 模板参数，用于替换 URL 中的占位符。
    /// 可以是以下类型：
    /// <list type="bullet">
    /// <item>匿名对象或普通对象：属性名对应模板中的占位符（例如 new { uid = 1234 }）</item>
    /// <item>字典：<see cref="Dictionary{String,String}"/>，键对应占位符名</item>
    /// </list>
    /// 如果 URL 中存在未匹配的占位符，则会抛出 <see cref="InvalidOperationException"/>。
    /// </param>
    /// <param name="queryParams">
    /// 查询参数，用于生成 URL 查询字符串。
    /// 可以是以下类型：
    /// <list type="bullet">
    /// <item>匿名对象或普通对象：属性名对应查询参数名</item>
    /// <item>字典：<see cref="Dictionary{String,String}"/>，键值对直接作为查询参数</item>
    /// </list>
    /// 默认查询参数会与传入参数合并，传入参数会覆盖默认参数。
    /// </param>
    /// <returns>生成的完整 URL，已进行模板替换和查询参数拼接。</returns>
    /// <exception cref="InvalidOperationException">
    /// 如果模板中存在未替换的占位符，抛出异常并提示缺失的参数名。
    /// </exception>
    /// <remarks>
    /// - 模板参数支持大小写不敏感匹配。
    /// - 查询参数会自动进行 URL 编码。
    /// - 如果模板参数是列表/数组，则 URL 中占位符必须为 {0}, {1}, … 格式，否则无法匹配。
    /// - 该方法线程安全，合并默认查询参数时会加锁。
    /// </remarks> 
    string GetEndpoint(string routeKey, object? templateParams = null, object? queryParams = null);

    /// <summary>
    /// （已弃用）仅支持模板化参数替换的接口构建方法。
    /// 建议使用 <see cref="GetEndpoint(string, object, object)"/> 替代。
    /// </summary>
    /// <param name="routeKey">路由 Key。</param>
    /// <param name="templateParams">模板参数。</param>
    /// <returns>完整接口地址。</returns>
    [Obsolete("请使用 GetEndpoint(routeKey, templateParams, queryParams) 替代。")]
    string GetEndpointTemplate(string routeKey, object templateParams);

    /// <summary>
    /// 添加一个默认查询参数，所有接口地址都会自动附带该参数。
    /// </summary>
    /// <param name="key">参数名。</param>
    /// <param name="value">参数值。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf AddDefaultQueryParam(string key, string value);

    /// <summary>
    /// 批量添加默认查询参数。
    /// 已存在的 Key 会被覆盖。
    /// </summary>
    /// <param name="parameters">查询参数集合</param>
    /// <returns>当前实例（支持链式调用）</returns>
    TSelf AddDefaultQueryParams(Dictionary<string, string> parameters);

    /// <summary>
    /// 移除一个默认查询参数。
    /// </summary>
    /// <param name="key">参数名。</param>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf RemoveDefaultQueryParam(string key);

    /// <summary>
    /// 清空所有默认查询参数。
    /// </summary>
    /// <returns>当前实例（支持链式调用）。</returns>
    TSelf ClearDefaultQueryParams();

    /// <summary>
    /// 获取所有默认查询参数（只读）。
    /// </summary>
    IReadOnlyDictionary<string, string> GetDefaultQueryParams();
}



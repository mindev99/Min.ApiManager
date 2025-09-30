namespace Min.ApiManager.Interface;

/// <summary>
/// 定义 API 管理器的统一接口，
/// 提供环境切换、域名管理、路由配置和端点构建等核心能力。
/// </summary>
/// <typeparam name="TSelf">
/// 实现该接口的具体类型，
/// 用于支持链式调用模式（Fluent API）。
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="IApiManager{TSelf}"/> 是整个 API 管理体系的核心接口，
/// 它将环境管理、域名绑定、路由配置等能力统一抽象，
/// 适用于需要支持多环境、多端点的应用程序或 SDK。
/// </para>
/// <para>
/// 常见应用场景包括：
/// <list type="bullet">
/// <item>支持开发 / 测试 / 生产等多环境的客户端 SDK。</item>
/// <item>在桌面应用或移动端中，根据配置文件动态切换环境。</item>
/// <item>自动化测试场景下，运行时指定目标环境。</item>
/// </list>
/// </para>
/// </remarks>
public interface IApiManager<TSelf> : IConfigurableManager<TSelf>, IDomainManager<TSelf>, IRouteManager<TSelf>, IEndpointBuilder<TSelf> where TSelf : IApiManager<TSelf>
{
    /// <summary>
    /// 当环境切换成功时触发的事件。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 订阅者可在该事件中执行额外逻辑，例如：
    /// <list type="bullet">
    /// <item>记录环境切换日志</item>
    /// <item>刷新缓存或重置依赖服务</item>
    /// <item>重新初始化与 API 环境相关的配置</item>
    /// </list>
    /// </para>
    /// <para>
    /// 事件仅在切换成功后触发，
    /// 若目标环境无效或切换失败，则不会触发。
    /// </para>
    /// </remarks>
    event EventHandler<Event.EnvironmentChangedEventArgs> OnEnvironmentChanged;

    /// <summary>
    /// 获取当前运行环境。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 当前环境通常为 <see cref="ApiEnvironment.Development"/>、
    /// <see cref="ApiEnvironment.Staging"/> 或
    /// <see cref="ApiEnvironment.Production"/>等。
    /// </para>
    /// <para>
    /// 在调用 <see cref="SwitchEnvironment(ApiEnvironment)"/>
    /// 或 <see cref="TrySwitchEnvironment(ApiEnvironment, out string?)"/> 方法时，
    /// 该属性会被更新。
    /// </para>
    /// </remarks>
    ApiEnvironment CurrentEnvironment { get; }

    /// <summary>
    /// 切换当前运行环境。
    /// </summary>
    /// <param name="environment">目标环境枚举值。</param>
    /// <returns>返回当前实例，支持链式调用。</returns>
    /// <exception cref="InvalidOperationException">
    /// 当目标环境未配置有效域名，或当前状态不允许切换时抛出。
    /// </exception>
    /// <remarks>
    /// <para>
    /// 调用此方法后，如果切换成功，将自动触发
    /// <see cref="OnEnvironmentChanged"/> 事件。
    /// </para>
    /// <para>
    /// 该方法适合在环境配置已明确，且切换失败应视为异常情况时使用。
    /// </para>
    /// </remarks>
    TSelf SwitchEnvironment(ApiEnvironment environment);

    /// <summary>
    /// 尝试切换当前运行环境。
    /// </summary>
    /// <param name="environment">
    /// 目标环境枚举值，例如 Development、Staging、Production。
    /// </param>
    /// <param name="errorMessage">
    /// 输出参数：切换失败时返回错误描述；如果切换成功，则为 <c>null</c>。
    /// </param>
    /// <returns>
    /// <c>true</c> 表示切换成功；<c>false</c> 表示切换失败。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 使用该方法可以在运行时安全地尝试切换环境，而不会抛出异常导致程序中断。
    /// </para>
    /// <para>
    /// 内部实现应在切换前执行必要的验证（例如检查目标环境是否配置了有效域名）。
    /// </para>
    /// <para>
    /// <see cref="OnEnvironmentChanged"/> 事件仅在切换成功时触发。
    /// </para>
    /// </remarks>
    bool TrySwitchEnvironment(ApiEnvironment environment, out string? errorMessage);
}


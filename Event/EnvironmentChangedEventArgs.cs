namespace Min.ApiManager.Event;

/// <summary>
/// 表示 API 管理器环境切换时的事件参数。
/// <para>
/// 该类继承自 <see cref="EventArgs"/>，用于提供环境切换过程中的上下文信息，
/// 包括切换前的环境、切换后的环境，以及事件发生的时间。
/// </para>
/// </summary>
public class EnvironmentChangedEventArgs : EventArgs
{
    /// <summary>
    /// 获取切换前的运行环境。
    /// <para>
    /// 例如：<c>Development</c> → <c>Staging</c> 时，该属性值为 <c>Development</c>。
    /// </para>
    /// </summary>
    public ApiEnvironment OldEnvironment { get; }

    /// <summary>
    /// 获取切换后的运行环境。
    /// <para>
    /// 例如：<c>Development</c> → <c>Staging</c> 时，该属性值为 <c>Staging</c>。
    /// </para>
    /// </summary>
    public ApiEnvironment NewEnvironment { get; }

    /// <summary>
    /// 获取环境切换发生的时间（UTC 时间）。
    /// <para>
    /// 默认使用 <see cref="DateTime.UtcNow"/> 捕获事件触发的时间点，
    /// 便于在日志记录或调试时进行时间追踪。
    /// </para>
    /// </summary>
    public DateTime ChangedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// 初始化 <see cref="EnvironmentChangedEventArgs"/> 实例。
    /// </summary>
    /// <param name="oldEnv">切换前的环境。</param>
    /// <param name="newEnv">切换后的环境。</param>
    public EnvironmentChangedEventArgs(ApiEnvironment oldEnv, ApiEnvironment newEnv)
    {
        OldEnvironment = oldEnv;
        NewEnvironment = newEnv;
    }
}

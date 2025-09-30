namespace Min.ApiManager.Interface;

/// <summary>
/// 配置管理接口，用于加载、更新、导出 API 管理器的配置信息。
/// 单一职责：只管理配置，不涉及路由拼接或域名切换。
/// </summary>
/// <typeparam name="TSelf">实现类自身类型，用于链式调用返回自定义类型</typeparam>
public interface IConfigurableManager<TSelf> where TSelf : IConfigurableManager<TSelf>
{
    /// <summary>
    /// 初始化 API 管理器配置。
    /// 可用于加载默认域名、路由、模板规则等。
    /// </summary>
    /// <param name="initializer">初始化回调，提供当前管理器实例</param>
    /// <returns>当前实例（支持链式调用）</returns>
    TSelf Initialize(Action<TSelf> initializer);

    /// <summary>
    /// 动态更新配置，可在运行时修改域名、路由等。
    /// </summary>
    /// <param name="updater">配置更新回调，提供当前管理器实例</param>
    /// <returns>当前实例（支持链式调用）</returns>
    TSelf UpdateConfig(Action<TSelf> updater);

    /// <summary>
    /// 从配置字符串加载配置，可用于持久化恢复。
    /// 支持 JSON / XML / INI 格式。
    /// </summary>
    /// <param name="config">配置字符串</param>
    /// <param name="format">指定配置格式（JSON 或 XML 或 INI格式）</param>
    /// <returns>当前实例（支持链式调用）</returns>
    TSelf LoadConfig(string config, LoadFormat format);

    /// <summary>
    /// 导出当前配置快照（包含所有环境域名和路由），用于调试或日志记录。
    /// 包括当前环境、域名列表、路由列表等。
    /// </summary>
    /// <returns>格式化后的配置字符串，例如 JSON 或自定义文本。</returns>
    string DumpConfig();
}



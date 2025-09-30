namespace Min.ApiManager;

/// <summary>
/// 应用运行环境枚举
/// </summary>
public enum ApiEnvironment
{
    /// <summary>
    /// 本地开发环境
    /// </summary>
    Development,

    /// <summary>
    /// 测试/QA环境
    /// </summary>
    Testing,

    /// <summary>
    /// 用户验收测试环境
    /// </summary>
    UAT,

    /// <summary>
    /// 预发布/模拟生产环境
    /// </summary>
    Staging,

    /// <summary>
    /// 沙箱环境（实验、隔离用）
    /// </summary>
    Sandbox,

    /// <summary>
    /// 生产环境
    /// </summary>
    Production
}


/// <summary>
/// 支持的配置格式
/// </summary>
public enum LoadFormat
{
    JSON,
    XML,
    INI,
}

namespace Min.ApiManager.Event;

/// <summary>
/// 提供一个弱引用事件封装类，用于安全地订阅和触发事件，避免事件导致的内存泄漏。
/// </summary>
/// <typeparam name="T">事件处理器的类型，一般为委托类型（例如 <see cref="EventHandler"/> 或自定义 Action 委托）。</typeparam>
public class WeakEvent<T> where T : class
{
    /// <summary>
    /// 内部锁对象，用于保护对 <see cref="_handlers"/> 的并发访问。
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 内部存储的弱引用处理器列表。
    /// </summary>
    private readonly List<WeakReference<T>> _handlers = [];

    /// <summary>
    /// 添加事件处理器。使用弱引用保存，不会阻止垃圾回收。
    /// </summary>
    /// <param name="handler">要添加的事件处理器，不能为 null。</param>
    public void AddHandler(T? handler)
    {
        if (handler == null) return;
        lock (_lock)
        {
            _handlers.Add(new WeakReference<T>(handler));
        }
    }

    /// <summary>
    /// 移除事件处理器。
    /// </summary>
    /// <param name="handler">要移除的事件处理器，不能为 null。</param>
    public void RemoveHandler(T? handler)
    {
        if (handler == null) return;
        lock (_lock)
        {
            _handlers.RemoveAll(wr => wr.TryGetTarget(out var h) && h == handler);
        }
    }

    /// <summary>
    /// 安全触发事件，自动跳过已被垃圾回收的处理器，并清理死引用。
    /// </summary>
    /// <param name="invocation">触发每个有效事件处理器的回调方法。</param>
    public void Invoke(Action<T> invocation)
    {
        if (invocation == null) return;

        List<WeakReference<T>> dead = [];
        List<T> liveHandlers = [];

        lock (_lock)
        {
            foreach (var wr in _handlers)
            {
                if (wr.TryGetTarget(out var h) && h != null)
                {
                    liveHandlers.Add(h);
                }
                else
                {
                    dead.Add(wr);
                }
            }

            // 移除已被 GC 回收的死引用
            foreach (var d in dead)
            {
                _handlers.Remove(d);
            }
        }

        // 调用处理器，锁外执行，避免回调中再次访问 WeakEvent 导致死锁
        foreach (var handler in liveHandlers)
        {
            invocation(handler);
        }
    }
}


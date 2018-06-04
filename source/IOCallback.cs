namespace Rrs
{
    /// <summary>
    /// IO操作完成回调
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="source">IO的源管道</param>
    /// <param name="packet">输入或输出关联的包</param>
    /// <param name="state">异步状态参数</param>
    public delegate void IOCallback<TState>(IPipeline source, IPacket packet, TState state);
}

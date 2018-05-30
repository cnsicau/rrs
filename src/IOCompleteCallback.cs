namespace rrs
{
    /// <summary>
    /// IO操作完成回调
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="sender"></param>
    /// <param name="packet"></param>
    /// <param name="state"></param>
    public delegate void IOCompleteCallback<TState>(IPipeline sender, IPacket packet, TState state);
}

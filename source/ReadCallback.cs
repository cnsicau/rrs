namespace Rrs
{
    /// <summary>
    /// 报文内容读取回调
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="data">报文内容</param>
    /// <param name="state">状态</param>
    public delegate void ReadCallback<TState>(PacketData data, TState state);
}

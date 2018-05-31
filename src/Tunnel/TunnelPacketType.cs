namespace rrs.Tunnel
{
    public enum TunnelPacketType : byte
    {
        /// <summary>数据包</summary>
        Data = 0,
        /// <summary>认证包</summary>
        Authenticate = 1,
        /// <summary>PING请求</summary>
        Ping = 2,
        /// <summary>对应 PING的响应</summary>
        Pong = 3,
        /// <summary>激活</summary>
        Active = 4,
        /// <summary>已激活 对应 Active的响应</summary>
        Actived = 5,
        /// <summary>终结连接</summary>
        Terminate = 6,
    }
}

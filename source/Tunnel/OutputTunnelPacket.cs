using System.Collections.Generic;

namespace Rrs.Tunnel
{
    /// <summary>
    /// 输出报文
    /// </summary>
    public class OutputTunnelPacket : TunnelPacket
    {
        static readonly Dictionary<TunnelPacketType, byte[]> headers = new Dictionary<TunnelPacketType, byte[]>(7)
            {
                { TunnelPacketType.Data, GenerateHeaderBytes(TunnelPacketType.Data) },
                { TunnelPacketType.Authenticate, GenerateHeaderBytes(TunnelPacketType.Authenticate) },
                { TunnelPacketType.Ping, GenerateHeaderBytes(TunnelPacketType.Ping) },
                { TunnelPacketType.Pong, GenerateHeaderBytes(TunnelPacketType.Pong) },
                { TunnelPacketType.Active, GenerateHeaderBytes(TunnelPacketType.Active) },
                { TunnelPacketType.Actived, GenerateHeaderBytes(TunnelPacketType.Actived) },
                { TunnelPacketType.Terminate, GenerateHeaderBytes(TunnelPacketType.Terminate) },
            };
        private readonly IPacket data;

        static byte[] GenerateHeaderBytes(TunnelPacketType type)
        {
            var bytes = new byte[HeaderSize];
            HeaderSerializer.Serialize((int)type, 0, bytes);
            return bytes;
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public OutputTunnelPacket(TunnelPipeline source, TunnelPacketType type, IPacket data) : base(source)
        {
            Type = type;
            this.data = data;
        }

        public override void Read<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            if (data == null) callback(new PacketData(this), state);
            else data.Read(callback, state);
        }

        public override void ReadHeader<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            callback(new PacketData(this, headers[Type], HeaderSize), state);
        }
    }
}

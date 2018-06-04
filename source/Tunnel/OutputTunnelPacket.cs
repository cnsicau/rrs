using System.Collections.Generic;

namespace Rrs.Tunnel
{
    /// <summary>
    /// 输出报文
    /// </summary>
    public class OutputTunnelPacket : TunnelPacket
    {
        private readonly PacketData data;
        private readonly byte[] header;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public OutputTunnelPacket(TunnelPipeline source, byte[] header, TunnelPacketType type, PacketData data) : base(source)
        {
            Type = type;
            this.header = header;
            this.Length = data?.Size ?? 0;
            this.data = data;
        }

        public override void Read<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            callback(data ?? new PacketData(this), state);
        }

        public override void ReadHeader<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {

            HeaderSerializer.Serialize((int)Type, Length, header);
            callback(new PacketData(this, header, HeaderSize), state);
        }
    }
}

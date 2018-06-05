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

            HeaderSerializer.Serialize((int)type, data?.Size ?? 0, header);
        }

        public override void Read<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            var data = new PacketData(this, this.data?.Buffer, this.data?.Size ?? 0);
            this.data.Dispose();
            callback(data, state);
        }

        public override PacketData HeaderData { get { return new PacketData(this, header, HeaderSize); } }
    }
}

using System;

namespace Rrs
{
    /// <summary>
    /// 包数据
    /// </summary>
    public class PacketData : IDisposable
    {
        /// <summary>
        /// 空包
        /// </summary>
        public static readonly PacketData Empty = new PacketData(null, null, 0);

        private readonly IPacket packet;
        private readonly byte[] buffer;
        private int size;
        private PacketData parent = null;

        /// <summary>
        /// 创建子集数据
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public PacketData CreateSubData(int size)
        {
            return new PacketData(packet, buffer, size) { parent = this };
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        public PacketData(IPacket packet, byte[] buffer, int size)
        {
            this.packet = packet;
            this.buffer = buffer;
            this.size = size;
        }

        /// <summary>
        /// 该数据对应的包
        /// </summary>
        public IPacket Packet { get { return packet; } }

        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public byte[] Buffer { get { return buffer; } }

        /// <summary>
        /// 大小
        /// </summary>
        public int Size { get { return size; } }

        /// <summary>
        /// 完结
        /// </summary>
        public bool Completed { get { return size <= 0; } }

        public void Dispose() { size = 0; }

        /// <summary>
        /// 丢弃前端数据
        /// </summary>
        /// <param name="count"></param>
        public void Discard(int count)
        {
            if (count <= 0) return;

            size -= count;
            if (parent != null) parent.Discard(count);
            else Array.Copy(buffer, count, buffer, 0, size);
        }

        /// <summary>
        /// 转缓冲区包
        /// </summary>
        /// <param name="data"></param>
        public static implicit operator BufferPacket(PacketData data)
        {
            var packet = new BufferPacket(data.packet.Source, data.buffer);
            packet.SetBufferSize(data.size);
            return packet;
        }
    }
}

using System;

namespace Rrs.Tunnel
{
    class InputTunnelPacket : TunnelPacket
    {
        private readonly IPipeline trans;   // 传输管道
        private readonly byte[] header = new byte[HeaderSize];   // 信息头内容
        private int headerSize;
        private IPacket packet;
        private byte[] source;
        private int sourceSize, sourceOffset;
        private int length = 0;

        public InputTunnelPacket(TunnelPipeline pipeline) : base(pipeline)
        {
            trans = pipeline.TransPipeline;
        }

        /// <summary>
        /// 创建包
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void Construct<TState>(IOCallback<TState> callback, TState state)
        {
            for (; headerSize < HeaderSize && sourceOffset < sourceSize; headerSize++)
            {
                header[headerSize] = source[sourceOffset++];
            }

            if (headerSize < HeaderSize)            // 信息头未解析完成
            {
                ReadMoreData<TState>(new object[] { callback, state });
                return;
            }

            HeaderSerializer.Deserialize(header, this);
            // 检查报文有效性
            if (Magic != MagicValue || Version != VersionValue)
                trans.Interrupte();

            length = Length;
            callback(Source, this, state);
        }

        void ReadMoreData<TState>(object[] args)
        {
            if (packet == null) trans.Input(OnHeaderInput<TState>, args);
            else packet.Read(OnHeaderPacketRead<TState>, args);
        }

        void OnHeaderInput<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            this.packet = packet;
            packet.Read(OnHeaderPacketRead<TState>, args);
        }

        void OnHeaderPacketRead<TState>(PacketData data, object[] args)
        {
            if (data.Completed) // 当前包已使用完
            {
                packet.Dispose();
                trans.Input(OnHeaderInput<TState>, args);
                return;
            }
            source = data.Buffer;
            sourceSize = data.Size;
            sourceOffset = 0;

            Construct((IOCallback<TState>)args[0], (TState)args[1]);
        }

        public override void Read<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            if (length == 0) // 无数据内容直接返回
            {
                callback(new PacketData(this), state);
                return;
            }
            if (sourceOffset == sourceSize) // 已读取完加载后续包
            {
                trans.Input(OnDataInput<TState>, new object[] { callback, state });
                return;
            }
            // 将可用缓冲区移至起始位置
            if (sourceOffset > 0)
            {
                sourceSize -= sourceOffset;
                Array.Copy(source, sourceOffset, source, 0, sourceSize);
            }
            // 获取数据块大小
            var dataSize = length > sourceSize ? sourceSize : length;
            length -= dataSize;
            sourceOffset = dataSize;
            callback(new PacketData(this, source, dataSize), state);
        }
        void OnDataInput<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            this.packet = packet;
            packet.Read(OnDataPacketRead<TState>, args);
        }

        void OnDataPacketRead<TState>(PacketData data, object[] args)
        {
            if (data.Completed) // 当前包已使用完
            {
                packet.Dispose();
                trans.Input(OnDataInput<TState>, args);
                return;
            }
            source = data.Buffer;
            sourceSize = data.Size;
            sourceOffset = 0;

            Read((ReadCallback<TState>)args[0], (TState)args[1]);   // 继续读取内容
        }

        public override void ReadHeader<TState>(ReadCallback<TState> callback, TState state = default(TState))
        {
            callback(new PacketData(this), state);
        }

        public override void Dispose()
        {
            headerSize = 0;     // 清空信息头，以便重新解析
            base.Dispose();
        }
    }
}

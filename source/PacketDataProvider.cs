using System;
using System.Threading;

namespace Rrs
{
    /// <summary>
    /// 数据提供
    /// </summary>
    public class PacketDataProvider
    {
        private readonly IPipeline pipeline;
        private IPacket current;
        private PacketData currentData;
        private int currentDataOffset;

        public PacketDataProvider(IPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="size">数据大小上限</param>
        /// <param name="callback">回调</param>
        /// <param name="state">状态</param>
        public void GetPacketData<TState>(int size, ReadCallback<TState> callback, TState state)
        {
            var args = new object[] { size, callback, state };

            if (current == null)
            {
                pipeline.Input(OnInputPacket<TState>, args);
            }
            else if (currentData != null && currentData.Size > currentDataOffset)
            {
                currentData.Discard(currentDataOffset);
                OnPacketRead<TState>(currentData, args);
            }
            else
            {
                current.Read(OnPacketRead<TState>, args);
            }
        }

        void OnInputPacket<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            current = packet;
            current.Read(OnPacketRead<TState>, args);
        }

        void OnPacketRead<TState>(PacketData data, object[] args)
        {
            if (data.Completed)
            {
                pipeline.Input(OnInputPacket<TState>, args);
            }
            else
            {
                var size = (int)args[0];
                currentData = data;
                currentDataOffset = size;
                if (size < data.Size)
                {
                    data = new PacketData(data.Packet, data.Buffer, size);
                }
                ((ReadCallback<TState>)args[1])(data, (TState)args[2]);
            }
        }
    }
}

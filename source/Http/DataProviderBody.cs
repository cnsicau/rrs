namespace Rrs.Http
{
    class DataProviderBody : IHttpBody
    {
        private int bodySize;
        private readonly PacketDataProvider data;

        public DataProviderBody(int bodySize, PacketDataProvider data)
        {
            this.bodySize = bodySize;
            this.data = data;
        }

        public void Read<TState>(ReadCallback<TState> callback, TState state)
        {
            if (bodySize <= 0)
            {
                callback(new PacketData(null, null, 0), state);
            }
            else
            {
                data.GetPacketData(bodySize, OnRead<TState>, new object[] { callback, state });
            }
        }

        void OnRead<TState>(PacketData data, object[] args)
        {
            bodySize -= data.Size; // 移除剩余大小
            ((ReadCallback<TState>)args[0])(data, (TState)args[1]);
        }
    }
}

namespace Rrs.Http
{
    public class BufferBody : IHttpBody
    {
        public BufferBody(int size, byte[] buffer)
        {
            this.Size = size;
            this.Buffer = buffer;
        }

        public int Size { get; }

        public byte[] Buffer { get; }

        public void Read<TState>(ReadCallback<TState> callback, TState state)
        {
            callback(new PacketData(null, Buffer, Size), state);
        }
    }
}

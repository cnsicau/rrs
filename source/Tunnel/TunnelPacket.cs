using System;
using System.Collections.Generic;

namespace Rrs.Tunnel
{
    /// <summary>
    /// 信息头说明
    ///  位       码     说明
    /// 0 - 3     MAGIC  标识头  4位 (1111) 15
    /// 4 - 6     VER    版本    3位 (010)  2
    /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
    /// 10 - 23   SIZE   长度    14位 1 - 8192
    /// </summary>
    public abstract class TunnelPacket : IPacket
    {
        #region Inner Class
        class CommandPacket : TunnelPacket
        {
            static readonly Dictionary<TunnelPacketType, byte[]> headers = new Dictionary<TunnelPacketType, byte[]>(7)
            {
                { TunnelPacketType.Authenticate, GenerateHeaderBytes(TunnelPacketType.Authenticate) },
                { TunnelPacketType.Ping, GenerateHeaderBytes(TunnelPacketType.Ping) },
                { TunnelPacketType.Pong, GenerateHeaderBytes(TunnelPacketType.Pong) },
                { TunnelPacketType.Active, GenerateHeaderBytes(TunnelPacketType.Active) },
                { TunnelPacketType.Actived, GenerateHeaderBytes(TunnelPacketType.Actived) },
                { TunnelPacketType.Terminate, GenerateHeaderBytes(TunnelPacketType.Terminate) },
            };

            static byte[] GenerateHeaderBytes(TunnelPacketType type)
            {
                var bytes = new byte[HeaderSize];
                HeaderSerializer.Serialize((int)type, 0, bytes);
                return bytes;
            }

            TunnelPacketType type;

            public CommandPacket(TunnelPipeline pipeline, TunnelPacketType type) : base(pipeline)
            {
                this.type = type;
            }

            public override void Read<TState>(ReadCallback<TState> callback, TState state = default(TState))
            {
                callback(headers[type], 0, state); // 无内容
            }

            public override void ReadHeader<TState>(ReadCallback<TState> callback, TState state = default(TState))
            {
                callback(headers[type], HeaderSize, state);
            }
        }

        /// <summary>
        /// 创建命令类型数据包（除Data外)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static public TunnelPacket CreateCommandPacket(TunnelPipeline source, TunnelPacketType type)
        {
            if (type == TunnelPacketType.Data) throw new NotSupportedException(type.ToString());

            return new CommandPacket(source, type);
        }
        #endregion

        public const int HeaderSize = 3;

        public const int MagicValue = 15;       // 1111
        public const int VersionValue = 2;      // 010
        private readonly TunnelPipeline pipeline;
        private bool disposed;

        private int magic = MagicValue;
        private int version = VersionValue;
        private TunnelPacketType type;
        private int length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeline"></param>
        public TunnelPacket(TunnelPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// 幻值
        /// </summary>
        public int Magic
        {
            get { return magic; }
            set { magic = value; }
        }

        /// <summary>
        /// 版本
        /// </summary>
        public int Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public TunnelPacketType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// 关联源
        /// </summary>
        public IPipeline Source { get { return pipeline; } }

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool Disposed { get { return disposed; } }

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public abstract void Read<TState>(ReadCallback<TState> callback, TState state = default(TState));

        /// <summary>
        /// 读取信息头
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public abstract void ReadHeader<TState>(ReadCallback<TState> callback, TState state = default(TState));

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose() { disposed = true; }
    }
}

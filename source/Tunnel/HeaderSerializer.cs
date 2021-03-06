﻿namespace Rrs.Tunnel
{

    /// <summary>
    ///  位       码     说明
    /// 0 - 3     MAGIC  标识头  4位 (1111) 15
    /// 4 - 6     VER    版本    3位 (010)  2
    /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
    /// 10 - 23   SIZE   长度    14位 1 - 8192
    /// </summary>
    public class HeaderSerializer
    {
        /// <summary>
        /// 序列化内容至缓冲区前3字节
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="dataSize"></param>
        /// <param name="buffer"></param>
        public static void Serialize(int packetType, int dataSize, byte[] buffer)
        {
            buffer[2] = (byte)(dataSize & 0xff);
            buffer[1] = (byte)((dataSize >> 8) | ((0x7 & packetType) << 6));
            buffer[0] = (byte)((TunnelPacket.MagicValue << 4) | (TunnelPacket.VersionValue << 1) | (packetType >> 2));
        }

        /// <summary>
        /// 将buffer的前3个字节，解析到 TunnelPacket 中
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="packet"></param>
        public static void Deserialize(byte[] buffer, TunnelPacket packet)
        {
            packet.Length = ((buffer[1] & 0x3f) << 8) | buffer[2];
            packet.Type = (TunnelPacketType)((buffer[1] >> 6) | (buffer[0] & 1) << 2);
            packet.Version = (0x7 & (buffer[0] >> 1));
            packet.Magic = buffer[0] >> 4;
        }
    }
}

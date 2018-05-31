namespace rrs
{
    /// <summary>
    /// 信息头说明
    ///  位       码     说明
    /// 0 - 3     MAGIC  标识头  4位 (1111) 15
    /// 4 - 6     VER    版本    3位 (010)  2
    /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
    /// 10 - 23   SIZE   长度    14位 1 - 8192
    /// </summary>
    public class TunnelPacket : Packet
    {
        public const int HeaderSize = 3;

        public const int MagicValue = 15;       // 1111
        public const int VersionValue = 2;      // 010

        public TunnelPacket(TunnelPipeline pipeline) : base(pipeline) { }

        public int Magic { get; set; }

        public int Version { get; set; }

        public TunnelPacketType Type { get; set; }
    }
}

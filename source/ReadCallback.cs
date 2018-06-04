﻿namespace Rrs
{
    /// <summary>
    /// 报文内容读取回调
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="buffer">数据缓冲区</param>
    /// <param name="size">缓冲区内容大小，使用小于等于0的值表示包已读取完毕</param>
    /// <param name="state">状态</param>
    public delegate void ReadCallback<TState>(byte[] buffer, int size, TState state);
}

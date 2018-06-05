namespace Rrs.Http
{
    /// <summary>
    /// 行解析回调
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="pipeline"></param>
    /// <param name="buffer"></param>
    /// <param name="size"></param>
    /// <param name="state"></param>
    public delegate void LineCallback<TState>(IPipeline pipeline, char[] buffer, int size, TState state);
}

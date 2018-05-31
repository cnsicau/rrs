namespace Rrs
{
    /// <summary>
    /// 管道回调
    ///     Accept\Connect 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="pipeline"></param>
    /// <param name="success"></param>
    /// <param name="state"></param>
    public delegate void PipelineCallback<TState>(IPipeline pipeline, bool success, TState state);
}

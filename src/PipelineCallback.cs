namespace rrs
{
    /// <summary>
    /// 管道回调
    ///     Accept\Connect 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="pipeline"></param>
    /// <param name="state"></param>
    public delegate void PipelineCallback<TState>(IPipeline pipeline, TState state);
}

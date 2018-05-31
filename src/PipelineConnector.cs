using System;
using System.Threading;

namespace Rrs
{
    public class PipelineConnector : IDisposable
    {
        private readonly IPipeline source;
        private IPipeline target;
        private int disposed;

        public PipelineConnector(IPipeline source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            this.source = source;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void Connect(IPipeline target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (this.target != null) throw new InvalidOperationException("alread connected.");

            this.target = target;

            target.Interrupted += TargetInterrupted;
            source.Interrupted += SourceInterrupted;

            target.Input(CompleteInput, source);
            source.Input(CompleteInput, target);
        }

        void TargetInterrupted(object sender, EventArgs e) { using (this) source.Interrupte(); }
        void SourceInterrupted(object sender, EventArgs e) { using (this) target.Interrupte(); }

        void CompleteInput(IPipeline input, IPacket packet, IPipeline output)
        {
            output.Output(packet, CompleteOutput, input);
        }

        void CompleteOutput(IPipeline output, IPacket packet, IPipeline input)
        {
            packet.Dispose();   // used.
            input.Input(CompleteInput, output);
        }

        public IPipeline Source { get { return source; } }

        public IPipeline Target { get { return target; } }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 1) return;

            Disposed?.Invoke(this, EventArgs.Empty);
        }
    }
}

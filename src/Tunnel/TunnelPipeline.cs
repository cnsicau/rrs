﻿using System;

namespace Rrs.Tunnel
{
    public class TunnelPipeline : IPipeline
    {
        private readonly IPipeline pipeline;
        private readonly TunnelPacket inputPacket;
        private readonly TunnelPacketReader reader;
        private readonly TunnelPacketWriter writer;
        private bool interrupted = false;

        public TunnelPipeline(IPipeline pipeline)
        {
            this.pipeline = pipeline;
            inputPacket = new TunnelPacket(this);
            reader = new TunnelPacketReader(inputPacket);
            writer = new TunnelPacketWriter(this);
            pipeline.Interrupted += OnInterrupted;
        }

        public IPipeline TransPipeline { get { return pipeline; } }


        void OnInterrupted(object sender, EventArgs e)
        {
            interrupted = true;
            Interrupted?.Invoke(this, e);
        }

        public event EventHandler Interrupted;

        public void Dispose() { pipeline.Dispose(); }

        public void Input<TState>(IOCompleteCallback<TState> callback, TState state = default(TState))
        {
            if (reader.Read())
            {
                callback(this, inputPacket, state);
            }
            else
            {
                pipeline.Input(CompleteBufferInput<TState>, new object[] { callback, state });
            }
        }

        void CompleteBufferInput<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            reader.SetSource(packet.Buffer, packet.Size);
            packet.Dispose();   // used

            var callback = (IOCompleteCallback<TState>)args[0];
            var state = (TState)args[1];

            Input(callback, state); // 
        }

        public void Interrupte()
        {
            if (interrupted) return;

            //pipeline.Output()

            pipeline.Interrupte();
        }

        public void Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state = default(TState))
        {
            writer.Write(packet, callback, state);
        }
    }
}

﻿using System;
using System.Threading.Tasks;

namespace EOLib.Net.Communication
{
    public interface INetworkClient : IDisposable
    {
        bool Connected { get; }

        bool Started { get; }

        TimeSpan ReceiveTimeout { get; }

        Task<ConnectResult> ConnectToServer(string host, int port);

        void Disconnect();

        Task RunReceiveLoopAsync();

        void CancelBackgroundReceiveLoop();

        int Send(IPacket packet);

        Task<int> SendAsync(IPacket packet, int timeout = 1500);
        
        Task<int> SendRawPacketAsync(IPacket packet, int timeout = 1500);
    }
}

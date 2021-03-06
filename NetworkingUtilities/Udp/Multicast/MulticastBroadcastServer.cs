﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;

namespace NetworkingUtilities.Udp.Multicast
{
	public class MulticastBroadcastServer : AbstractServer, IReceiver
	{
		private readonly bool _acceptBroadcast;
		private readonly Dictionary<EndPoint, ControlState> _clientsBuffers;
		private readonly string _multicastAddress;

		public MulticastBroadcastServer(int localPort, string multicastGroupAddress, string interfaceName,
			bool acceptBroadcast = false, string localIp = null) : base(localIp, localPort, interfaceName)
		{
			_multicastAddress = multicastGroupAddress;
			_acceptBroadcast = acceptBroadcast;
			ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_clientsBuffers = new Dictionary<EndPoint, ControlState>();
		}

		public override void StopService() =>
			(ServerSocket == null || ServerSocket.IsDisposed()
				? (Action) (() => { })
				: () =>
				{
					try
					{
						if (!_acceptBroadcast)
							ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
								new MulticastOption(IPAddress.Parse(_multicastAddress)));
						ServerSocket.Close();
					}
					catch (ObjectDisposedException)
					{
					}
					catch (Exception socketException)
					{
						OnCaughtException(socketException, EventCode.Other);
					}
				})();


		public override void StartService()
		{
			InitializeSocket();
			Receive();
		}

		private void InitializeSocket()
		{
			EndPoint = null;
			ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			try
			{
				ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				OnReportingStatus(StatusCode.Success, "Successfully set ReuseAddress option");
				var localAdd = string.IsNullOrEmpty(Ip) ? IPAddress.Any : IPAddress.Parse(Ip);
				var groupAddress = IPAddress.Parse(_multicastAddress);
				OnReportingStatus(StatusCode.Info,
					$"Started configuring socket for {(_acceptBroadcast ? "broadcast" : "multicast")} communication");

				if (!_acceptBroadcast)
				{
					ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
					ServerSocket.EnableBroadcast = false;
					OnReportingStatus(StatusCode.Success, "Successfully unset Broadcast option");
					ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
						new MulticastOption(groupAddress, localAdd));
					OnReportingStatus(StatusCode.Success,
						$"Successfully set AddMembership multicast option ({groupAddress})");
				}
				else
				{
					ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
					OnReportingStatus(StatusCode.Success, "Successfully set Broadcast option");

					ServerSocket.EnableBroadcast = true;
				}

				var endpoint = new IPEndPoint(localAdd, Port);
				ServerSocket.Bind(endpoint);
				OnReportingStatus(StatusCode.Success, $"Successfully bound to {endpoint}");
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Bind);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		public void Receive()
		{
			try
			{
				var state = new ControlState
				{
					CurrentSocket = ServerSocket,
					BufferSize = MaxBufferSize,
					Buffer = new byte[MaxBufferSize],
					StreamBuffer = new MemoryStream()
				};
				var endPoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				ServerSocket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref endPoint,
					OnReceiveFromCallback, state);
				OnReportingStatus(StatusCode.Info,
					$"Stared receiving {(_acceptBroadcast ? "broadcast" : "multicast")} bytes via UDP socket");
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void OnReceiveFromCallback(IAsyncResult ar)
		{
			try
			{
				var end = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				if (!(ar.AsyncState is ControlState state)) return;
				var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref end);
				OnReportingStatus(StatusCode.Success,
					$"Successfully received {bytesRead} bytes by {(_acceptBroadcast ? "broadcast" : "multicast")} via UDP socket");

				if (!_clientsBuffers.ContainsKey(end))
				{
					var s = new ControlState
					{
						Buffer = new byte[MaxBufferSize],
						BufferSize = MaxBufferSize,
						StreamBuffer = new MemoryStream(),
					};
					_clientsBuffers.Add(end, s);
				}

				if (bytesRead > 0)
				{
					_clientsBuffers[end].StreamBuffer.Write(state.Buffer, 0, bytesRead);
					if (state.Buffer.Any(@byte => @byte == '\0'))
					{
						ProcessMessage(end);
						_clientsBuffers[end].StreamBuffer = new MemoryStream();
					}
				}
				else if (_clientsBuffers[end].StreamBuffer.CanWrite && _clientsBuffers[end].StreamBuffer.Length > 0)
				{
					ProcessMessage(end);
					_clientsBuffers[end].StreamBuffer = new MemoryStream();
				}

				Receive();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void ProcessMessage(EndPoint end)
		{
			if (!_clientsBuffers.ContainsKey(end)) return;
			var state = _clientsBuffers[end];
			using var stream = state.StreamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			OnNewMessage(stream.ToArray(), ((IPEndPoint) end).ToString(), EndPoint.ToString());
		}

		public override void Send(byte[] data, string to = "")
		{
			try
			{
				var endpoint = IPEndPoint.Parse(to);
				var state = new ReceiverState
				{
					Port = endpoint.Port,
					Socket = ServerSocket,
					Ip = endpoint.Address.ToString()
				};

				ServerSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, OnSendToCallback, state);
				OnReportingStatus(StatusCode.Info, $"Started sending bytes to {endpoint} via UDP socket");
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		private void OnSendToCallback(IAsyncResult ar)
		{
			try
			{
				if (!(ar.AsyncState is ReceiverState state)) return;
				var _ = state.Socket.EndSendTo(ar);
				OnReportingStatus(StatusCode.Success, $"Successfully sent {_} bytes to {state.Ip}:{state.Port}");
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}
	}
}
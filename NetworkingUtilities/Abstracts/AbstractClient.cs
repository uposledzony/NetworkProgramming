﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Publishers;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractClient : ISender, IReceiver, IService
	{
		protected const int MaxBufferSize = 4096;
		protected readonly Socket ClientSocket;
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;
		private readonly IReporter _disconnected;
		private readonly IReporter _connected;
		private readonly IReporter _statusReporter;
		protected readonly bool ServerHandler;

		public ClientEvent WhoAmI { get; protected set; }

		protected AbstractClient(Socket clientSocket, bool serverHandler = false)
		{
			ClientSocket = clientSocket;
			_statusReporter = new StatusReporter();
			_lastException = new ExceptionReporter();
			_lastMessage = new MessageReporter();
			_disconnected = new ClientReporter();
			_connected = new ClientReporter();
			ServerHandler = serverHandler;

			if (clientSocket.RemoteEndPoint is IPEndPoint endPoint &&
				clientSocket.LocalEndPoint is IPEndPoint localEndPoint)
			{
				endPoint = IPEndPoint.Parse(endPoint.ToString());
				localEndPoint = IPEndPoint.Parse(localEndPoint.ToString());
				if (ServerHandler)
					WhoAmI = new ClientEvent("", endPoint, localEndPoint);
				else
					WhoAmI = new ClientEvent("", localEndPoint, endPoint);
			}
			else
			{
				WhoAmI = new ClientEvent("UNDEFINED", new IPEndPoint(IPAddress.Any, 0),
					new IPEndPoint(IPAddress.Any, 0));
			}
		}

		public bool IsConnected() => !(ClientSocket == null || ClientSocket.IsDisposed() ||
									   ClientSocket.Poll(1000, SelectMode.SelectRead) &&
									   (ClientSocket.Available == 0) || !ClientSocket.Connected);

		public void AddExceptionSubscription(Action<object, object> procedure) =>
			_lastException.AddSubscriber(procedure);

		public void AddMessageSubscription(Action<object, object> procedure) => _lastMessage.AddSubscriber(procedure);

		public void AddOnDisconnectedSubscription(Action<object, object> procedure) =>
			_disconnected.AddSubscriber(procedure);

		public void AddOnConnectedSubscription(Action<object, object> procedure) => _connected.AddSubscriber(procedure);

		public void AddStatusSubscription(Action<object, object> procedure) => _statusReporter.AddSubscriber(procedure);

		protected void OnNewMessage(byte[] message, string from, string to) =>
			_lastMessage.Notify((message, from, to));

		protected void OnDisconnect(string id, IPEndPoint ip, IPEndPoint serverIp) =>
			_disconnected.Notify((id, ip, serverIp));

		protected void OnConnect(string id, IPEndPoint ip, IPEndPoint serverIp) =>
			_connected.Notify((id, ip, serverIp));

		protected void OnCaughtException(Exception exception, EventCode code) =>
			_lastException.Notify((exception, code));

		protected void OnReportingStatus(StatusCode code, string statusInfo) =>
			_statusReporter.Notify((code, statusInfo));

		public abstract void Send(byte[] data, string to = "");

		public abstract void Receive();

		public abstract void StopService();

		public abstract void StartService();

		protected void ProcessMessage(MemoryStream streamBuffer)
		{
			using var stream = streamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			var message = stream.ToArray();
			var (from, to) = ServerHandler ? (WhoAmI.Id, "server") : ("server", WhoAmI.Id);
			OnNewMessage(message, from, to);
		}
	}
}
﻿using System;
using System.Net;
using ReactiveUI;

namespace TimeServer.Models
{
	public class ServerModel : ReactiveObject
	{
		private int _numberOfClients;

		public static implicit operator ServerModel(ValueTuple<IPEndPoint, string> pair) =>
			Create(pair.Item1, pair.Item2);

		public static ServerModel Create(IPEndPoint pairItem1, string pairItem2) =>
			new ServerModel
			{
				Ip = pairItem1,
				Name = pairItem2
			};

		public IPEndPoint Ip { get; private set; }

		public string IpString => Ip.ToString();

		public string Name { get; private set; }

		public override string ToString() => $"{Name}|{Ip}";

		public int NumberOfClients
		{
			get => _numberOfClients;
			private set => this.RaiseAndSetIfChanged(ref _numberOfClients, value);
		}

		public int MaxClientsCount => int.MaxValue;

		public void NewClient() =>
			(NumberOfClients < MaxClientsCount ? (Action) (() => ++NumberOfClients) : () => { })();

		public void ClientDisconnected() =>
			(NumberOfClients > 0 ? (Action) (() => --NumberOfClients) : () => { })();
	}
}
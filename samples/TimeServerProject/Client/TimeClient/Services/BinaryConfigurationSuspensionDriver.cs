﻿using System;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;
using TimeClient.Models;
using TimeClient.ViewModels;

namespace TimeClient.Services
{
	public class BinaryConfigurationSuspensionDriver : ISuspensionDriver
	{
		private readonly string _path;

		public BinaryConfigurationSuspensionDriver(string path) => _path = path;

		public IObservable<object> LoadState()
		{
			ConfigViewModel configViewModel;
			try
			{
				var bytes = File.ReadAllBytes(_path);
				var discoverPeriod = bytes[..4];
				var timePeriod = bytes[4..8];
				var localPort = bytes[8..12];
				var multicastPort = bytes[12..16];
				var multicastAddress = bytes[16..20];
				var selectedServerAddressLen = bytes[20..21][0];
				var selectedServerAddress = bytes[21..(21 + selectedServerAddressLen)];
				var start = (21 + selectedServerAddressLen);
				var selectedServerPort = bytes[start..(start + 4)];
				var selectedServerName = bytes[(start + 4)..];
				var server = ServerModel.Create(
					new IPEndPoint(new IPAddress(selectedServerAddress),
						BitConverter.ToInt32(selectedServerPort)), Encoding.ASCII.GetString(selectedServerName));

				if (server.Ip.Address.Equals(IPAddress.None))
					server = null;

				configViewModel = new ConfigViewModel
				(
					discoveryQueryPeriod: BitConverter.ToInt32(discoverPeriod),
					timeQueryPeriod: BitConverter.ToInt32(timePeriod),
					localPort: BitConverter.ToInt32(localPort),
					multicastPort: BitConverter.ToInt32(multicastPort),
					multicastAddress: new IPAddress(multicastAddress).ToString()
				) {SelectedServer = server};
			}
			catch (Exception)
			{
				var r = new Random();
				configViewModel = new ConfigViewModel
				(
					discoveryQueryPeriod: 10,
					localPort: r.Next(0, ushort.MaxValue),
					timeQueryPeriod: 10,
					multicastPort: 7,
					multicastAddress: "224.0.0.0"
				);
			}

			return Observable.Return(configViewModel);
		}

		public IObservable<Unit> SaveState(object state)
		{
			if (state is ConfigViewModel model && model.HasErrors == false)
			{
				var stream = new MemoryStream();
				stream.Write(BitConverter.GetBytes(model.DiscoveryQueryPeriod), 0, 4);
				stream.Write(BitConverter.GetBytes(model.TimeQueryPeriod), 0, 4);
				stream.Write(BitConverter.GetBytes(model.LocalPort), 0, 4);
				stream.Write(BitConverter.GetBytes(model.MulticastPort), 0, 4);
				stream.Write(IPAddress.Parse(model.MulticastAddress).GetAddressBytes(), 0, 4);
				var addressBytes = model.SelectedServer?.Ip.Address.GetAddressBytes() ??
							  IPAddress.None.GetAddressBytes();
				stream.Write(BitConverter.GetBytes((byte)addressBytes.Length), 0, 1);
				stream.Write(addressBytes, 0, addressBytes.Length);
				stream.Write(BitConverter.GetBytes(model.SelectedServer?.Ip.Port ?? 0), 0, 4);
				var name = Encoding.ASCII.GetBytes(model.SelectedServer?.Name ?? "");
				stream.Write(name, 0, name.Length);
				stream.Seek(0, SeekOrigin.Begin);
				File.WriteAllBytes(_path, stream.ToArray());
			}

			return Observable.Return(Unit.Default);
		}

		public IObservable<Unit> InvalidateState()
		{
			if (File.Exists(_path))
				File.Delete(_path);
			return Observable.Return(Unit.Default);
		}
	}
}
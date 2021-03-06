﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TimeProjectServices.Services
{
	public static class LocalIdSupplier
	{
		private static readonly char[] LookUpChars =
			"123567890_+`'~-@#!&^$(){}[]qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();

		private const int IdLength = 12;
		private static readonly HashSet<string> IdSet = new HashSet<string>();
		private static readonly HashSet<ushort> PortSet = new HashSet<ushort>();

		public static string CreateId()
		{
			string id;
			do
			{
				id = GetRandomString();
			} while (IdSet.Contains(id));

			IdSet.Add(id);
			return id;
		}

		public static ushort CreatePort()
		{
			ushort port;

			do
			{
				port = GetRandomPort();
			} while (PortSet.Contains(port));

			PortSet.Add(port);
			return port;
		}

		private static ushort GetRandomPort()
		{
			using var rNg = new RNGCryptoServiceProvider();
			var bytes = new byte[2];
			rNg.GetBytes(bytes);
			return BitConverter.ToUInt16(bytes);
		}

		private static string GetRandomString()
		{
			using var rNg = new RNGCryptoServiceProvider();
			var bytes = new byte[IdLength * 8];
			rNg.GetBytes(bytes);
			var builder = new StringBuilder();
			for (var i = 0; i < IdLength; ++i)
			{
				var block = bytes[i..(i + 8)];
				var num = BitConverter.ToUInt64(block);
				builder.Append(LookUpChars[num % IdLength]);
			}

			return builder.ToString();
		}
	}
}
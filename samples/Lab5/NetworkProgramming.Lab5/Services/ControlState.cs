﻿using System.IO;
using System.Net.Sockets;

namespace NetworkProgramming.Lab5.Services
{
	public class ControlState
	{
		public Socket CurrentSocket = null;
		public long BufferSize { get; set; }
		public byte[] Buffer = null;
		public MemoryStream StreamBuffer = new MemoryStream();
	}
}
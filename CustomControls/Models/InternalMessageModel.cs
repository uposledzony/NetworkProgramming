﻿using System;
using Avalonia.Media;
using JetBrains.Annotations;

namespace CustomControls.Models
{
	public class InternalMessageModel
	{
		public string ExceptionData { get; }
		public string Data { get; }
		public DateTime Date { get; }
		public InternalMessageType Type { get; }
		public IBrush ColorBrush { [UsedImplicitly] get; }
		public ClientModel ClientModelData { get; }

		private InternalMessageModel(MessageBuilder builder)
		{
			Data = builder.MessageData;
			ExceptionData = builder.ExceptionData;
			Date = builder.Date;
			Type = builder.Type;
			ClientModelData = builder.ClientData;

			ColorBrush = Type switch
						 {
							 InternalMessageType.Error => Brush.Parse("Red"),
							 InternalMessageType.Client => Brush.Parse("DarkCyan"),
							 InternalMessageType.Server => Brush.Parse("DarkMagenta"),
							 InternalMessageType.Info => Brush.Parse("Cyan"),
							 InternalMessageType.Success => Brush.Parse("Green"),
							 _ => Brush.Parse("Black")
						 };
		}

		public static MessageBuilder Builder() => new MessageBuilder();

		public override string ToString()
		{
			var dateMsg = Date == DateTime.MinValue ? "" : $"[{Date.ToShortDateString()} {Date.ToLongTimeString()}]";
			return $"[{Type}] {dateMsg} {ClientModelData?.ToString() ?? ""} {Data}\n{ExceptionData}";
		}

		public class MessageBuilder
		{
			internal MessageBuilder()
			{
			}

			internal string MessageData;
			internal string ExceptionData;
			internal InternalMessageType Type = InternalMessageType.Info;
			internal DateTime Date;
			internal ClientModel ClientData;

			public InternalMessageModel BuildMessage()
			{
				return new InternalMessageModel(this);
			}

			public MessageBuilder WithType(InternalMessageType type)
			{
				Type = type;
				return this;
			}

			public MessageBuilder AttachTimeStamp(bool value)
			{
				if (value)
				{
					Date = DateTime.Now;
				}

				return this;
			}

			public MessageBuilder AttachExceptionData(Exception e)
			{
				var msg = $"{e.Message}\n{e.InnerException?.Message ?? ""}\n";
				ExceptionData = msg;
				return this;
			}

			public MessageBuilder AttachTextMessage(string data)
			{
				MessageData = data;
				return this;
			}

			public MessageBuilder AttachClientData(ClientModel model)
			{
				ClientData = model;
				return this;
			}
		}
	}

	public enum InternalMessageType
	{
		Info,
		Error,
		Success,
		Client,
		Server
	}
}
﻿using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace Task4
{
	public static class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		public static void Main(string[] args) => BuildAvaloniaApp()
		   .StartWithClassicDesktopLifetime(args);

		// Avalonia configuration, don't remove; also used by visual designer.
		private static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
			   .UsePlatformDetect()
			   .LogToDebug()
			   .UseReactiveUI();
	}
}
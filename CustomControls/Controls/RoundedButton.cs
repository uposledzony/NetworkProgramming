﻿using Avalonia;
using Avalonia.Controls;

namespace CustomControls.Controls
{
	public class RoundedButton : Button
	{
		public static readonly StyledProperty<CornerRadius> RadiusProperty =
			AvaloniaProperty.Register<Button, CornerRadius>(nameof(Radius));


		public CornerRadius Radius
		{
			get => GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}
	}
}
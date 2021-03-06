﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HttpStorehouse.Models;

namespace HttpStorehouse.Views
{
	public class Page
	{
		private string _css;
		private string _bindable;

		public Page()
		{
			_css = "";
			_bindable = "";
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			using var stream = System.Reflection.Assembly.GetExecutingAssembly()
			   .GetManifestResourceStream("HttpStorehouse.Views.page.css");
			if (stream != null)
			{
				using var fStreamReader = new StreamReader(stream);
				var strCss = fStreamReader.ReadToEnd();
				_css = strCss;
			}

			using var streamBindable = System.Reflection.Assembly.GetExecutingAssembly()
			   .GetManifestResourceStream("HttpStorehouse.Views.page.bindable");
			if (streamBindable != null)
			{
				using var fStreamReader = new StreamReader(streamBindable);
				var strBindable = fStreamReader.ReadToEnd();
				_bindable = strBindable;
			}
			else
			{
				throw new ArgumentException("Cannot load page");
			}
		}

		public Page BindData<TK, TV, TD>(List<IModel<TK, TV, TD>> collection, string title, string appName,
			string totalValue)
		{
			var builder = new StringBuilder();
			builder.Append(@"<table class=""center table table-striped table-hover"">");
			builder.Append(
				@"<tr><th scope=""col"">Product Key</th><th scope=""col"">Product Name</th><th scope=""col"">Product Value</th></tr>");
			collection.ForEach(model =>
				builder.Append(
					$"<tr scope=\"row\"><td>{model.Key}</td><td>{model.Description}</td><td>{model.Value}</td></tr>"));
			builder.Append(@"</table>");
			builder.Append($"<h3>Total: {totalValue}</h3>");
			_bindable = _bindable.Replace("{{appname}}", appName).Replace("{{header}}", title)
			   .Replace("{{cssloader}}", _css).Replace("{{content}}", builder.ToString());

			return this;
		}

		public override string ToString()
		{
			return _bindable;
		}
	}
}
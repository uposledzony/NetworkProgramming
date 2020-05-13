﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HttpStorehouse.Models
{
	class ProductModel : IModel<int, string, string>
	{
		public int Key { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }
	}
}

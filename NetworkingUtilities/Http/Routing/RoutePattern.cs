﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace NetworkingUtilities.Http.Routing
{
	public class RoutePattern
	{
		private readonly string _baseData;
		private readonly ICollection<IRouteElement> _routeElems;

		public RoutePattern(string baseData, ICollection<IRouteElement> elems)
		{
			_baseData = baseData;
			_routeElems = elems;
		}

		public override string ToString()
		{
			return $"Endpoint attached to {_baseData}";
		}

		public ICollection<IRouteElement> RouteElems => (_routeElems).ToImmutableList();
	}
}
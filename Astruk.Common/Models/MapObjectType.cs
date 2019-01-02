﻿using System;
using System.Collections.Generic;

namespace Astruk.Common.Models
{
	public class MapObjectType
	{
		public MapObjectType(int id, string name, IDictionary<string, Type> parameters)
		{
			Id = id;
			Name = name;
			Parameters = parameters;
		}

		public int Id { get; }
		public string Name { get; }
		public IDictionary<string, Type> Parameters { get; }
	}
}
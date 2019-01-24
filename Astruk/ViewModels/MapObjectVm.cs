using System.Collections.Generic;
using System.Drawing;

namespace Astruk.ViewModels
{
	public class MapObjectVm
	{
		public PointF? Position
		{
			get {
				if (!float.TryParse(Parameters["X"] ?? Parameters["x"], out var x) ||
				    !float.TryParse(Parameters["Y"] ?? Parameters["y"], out var y)) return null;
				return new PointF(x, y);
			}
		}

		public string Type { get; set; }
		public IDictionary<string, string> Parameters { get; set; }
	}
}
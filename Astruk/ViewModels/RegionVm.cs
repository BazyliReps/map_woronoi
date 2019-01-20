using System.Collections.Generic;
using System.Drawing;

namespace Astruk.ViewModels
{
	public class RegionVm
	{
		public KeyMapObjectVm KeyObject { get; set; }
		public IList<PointF> Vertices { get; set; }
	}
}
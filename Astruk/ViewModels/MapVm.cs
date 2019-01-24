using System.Collections.Generic;
using System.Drawing;

namespace Astruk.ViewModels
{
	public class MapVm
	{
		public IList<PointF> Vertices { get; set; }
		public IEnumerable<RegionVm> Regions { get; set; }
	}
}
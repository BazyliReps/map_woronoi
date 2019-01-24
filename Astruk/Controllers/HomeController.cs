using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;
using Astruk.ViewModels;

namespace Astruk.Controllers
{
	public class HomeController : Controller
	{
		private IMapService MapService { get; }

		public HomeController(IMapService mapService)
		{
			MapService = mapService;
		}


		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public PartialViewResult _LoadMap(IList<Vertex> vertices, IEnumerable<KeyMapObject> keyObjects,
			IEnumerable<MapObjectType> types, IEnumerable<MapObject> objects)
		{
			var map = MapService.GenerateMap(vertices, keyObjects, types, objects);

			var model = new MapVm
			{
				Vertices = map.Vertices.Select(VertexToPointF)
					.ToList(), // Select in LINQ to Objects preserves order. Return of IEnumerable is because of other LINQs
				Regions = map.Regions.Select(x => new RegionVm
				{
					Vertices = x.Vertices.Select(VertexToPointF).ToList(),
					KeyObject = new KeyMapObjectVm
						{Name = x.KeyObject.Name, Position = new PointF((float) x.KeyObject.X, (float) x.KeyObject.Y)},
					Objects = x.Objects.Select(o=>new MapObjectVm
					{
						Type = o.Type.Name,
						Parameters = o.Parameters
					})
				})
			};

			return PartialView(model);
		}

		private static PointF VertexToPointF(Vertex vertex)
		{
			return new PointF((float) vertex.X, (float) vertex.Y);
		}
	}
}
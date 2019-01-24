using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Web.Mvc;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;
using Astruk.ViewModels;
using System;
using Input = Astruk.ViewModels.Input;

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
		public PartialViewResult _LoadMap(IList<Input.VertexVm> Vertices,
			IEnumerable<Input.KeyMapObjectVm> KeyObjects, IEnumerable<Input.MapObjectTypeVm> Types,
			IEnumerable<Input.MapObjectVm> Objects)
		{
			var vertices = new List<Vertex>();
			var keyObjects = new List<KeyMapObject>();
			var types = new List<MapObjectType>();
			var objects = new List<MapObject>();

			if (Vertices != null)
			{
				vertices.AddRange(Vertices.Select(vertex => new Vertex(vertex.X, vertex.Y)));
			}

			if (KeyObjects != null)
			{
				keyObjects.AddRange(KeyObjects.Select(keyObject =>
					new KeyMapObject(keyObject.Id, keyObject.X, keyObject.Y, keyObject.Name)));
			}

			foreach (var type in Types)
			{
				var parameters = new List<KeyValuePair<string, string>>();
				for (var i = 0; i < type.Keys.Count(); i++)
				{
					parameters.Add(new KeyValuePair<string, string>(type.Keys[i], type.Values[i]));
				}

				types.Add(new MapObjectType(type.Id, type.Name, parameters));
			}

			if (Objects != null)
			{
				foreach (var obj in Objects)
				{
					var parameters = new Dictionary<string, string>();
					if (obj == null) continue;
					var type = types.First(t => t.Name == obj.Type);
					for (var i = 0; i < obj.Parameters.Count(); i++)
					{
						try
						{
							parameters.Add(type.Parameters[i].Key, obj.Parameters[i]);
						}
						catch (ArgumentOutOfRangeException)
						{
						}
					}

					objects.Add(new MapObject(obj.Id, type, parameters));
				}
			}

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
					Objects = x.Objects.Select(o => new MapObjectVm
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
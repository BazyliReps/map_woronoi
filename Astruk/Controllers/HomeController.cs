using System.Linq;
using System.Collections.Generic;
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
        public /*PartialViewResult*/JsonResult LoadMap(IList<VertexVM> Vertices, IEnumerable<KeyMapObjectVM> KeyObjects, IEnumerable<MapObjectTypeVM> Types, IEnumerable<MapObjectVM> Objects)
        {


            var points = new List<DeluanVertex>();
            var vertices = new List<Vertex>();
            var keyObjects = new List<KeyMapObject>();
            var types = new List<MapObjectType>();
            var objects = new List<MapObject>();

            if (Vertices != null) {
                int i = 0;
                foreach (var vertex in Vertices) {
                    var v = new Vertex(vertex.X, vertex.Y) {
                        Id = i++
                    };
                    vertices.Add(v);
                }
            }

            if (KeyObjects != null) {

                foreach (var keyObject in KeyObjects) {
                    var newPoint = new DeluanVertex(keyObject.X, keyObject.Y);
                    if (!points.Contains(newPoint)) {
                        points.Add(newPoint);
                    }
                    keyObjects.Add(new KeyMapObject(keyObject.Id, keyObject.X, keyObject.Y, keyObject.Name));
                }
            }

            foreach (var type in Types) {
                types.Add(new MapObjectType(type.Id, type.Name, type.Parameters));
            }

            if (Objects != null) {

                foreach (var obj in Objects) {
                    objects.Add(new MapObject(obj.Id, types.First(type => type.Id == obj.Type.Id), obj.Parameters));
                }
            }

            var testMap = MapService.GenerateMap(vertices, keyObjects, (IEnumerable<MapObjectType>)types, objects);



            return Json(testMap);

        }


    }

}

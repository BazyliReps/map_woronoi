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
        public ITriangulationService TriangulationService { get; }
        private IMapService MapService { get; }

        public HomeController(IMapService mapService, ITriangulationService triangulationService)
        {
            TriangulationService = triangulationService;
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
            List<Point> Points = new List<Point>();
            int i = 0;
            
            foreach (var keyObject in KeyObjects) {
                Points.Add(new Point(keyObject.X, keyObject.Y));
            }
            
            var Triangles = TriangulationService.Triangulate(Points);

            return Json(Triangles);

        }
        

    }

}

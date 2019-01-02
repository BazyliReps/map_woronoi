using System;
using System.Web.Mvc;
using Astruk.Common.Interfaces;

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
		public PartialViewResult LoadMap()
		{
			throw new NotImplementedException();
		}
	}
}
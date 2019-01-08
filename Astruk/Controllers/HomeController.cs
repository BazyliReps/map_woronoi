using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Astruk.Common.Interfaces;
using Astruk.Common.Models;

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
		public PartialViewResult LoadMap(IList<Vertex> vertices)
		{
			var x = vertices;

			throw new NotImplementedException();
		}
	}
}
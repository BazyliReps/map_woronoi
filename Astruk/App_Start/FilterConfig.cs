using System.Web.Mvc;
using Astruk.Filters;

namespace Astruk
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new ValidationErrorHandlerAttribute());
		}
	}
}

using System.Web.Mvc;
using Astruk.Common.Exceptions;

namespace Astruk.Filters
{
	public class ValidationErrorHandlerAttribute : HandleErrorAttribute
	{
		public override void OnException(ExceptionContext filterContext)
		{
			if (filterContext.ExceptionHandled
			    || !filterContext.HttpContext.IsCustomErrorEnabled
			    || !(filterContext.Exception is ValidationException)
			    || !IsAjax(filterContext))
				base.OnException(filterContext);

			filterContext.HttpContext.Response.StatusCode = 422; //Unprocessable Entity, it's HTTP Extension, and System.Web.HttpStatusCode doesn't contain it.
			filterContext.Result = new JsonResult
			{
				Data = ((ValidationException) filterContext.Exception).ErrorMessages,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};

			filterContext.ExceptionHandled = true;
			filterContext.HttpContext.Response.Clear();
		}

		private static bool IsAjax(ControllerContext filterContext)
		{
			return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
		}
	}
}
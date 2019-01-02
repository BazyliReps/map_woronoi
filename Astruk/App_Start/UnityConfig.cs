using Astruk.DI;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace Astruk
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

			container.RegisterServices();
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
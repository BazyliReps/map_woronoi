using Astruk.Common.Interfaces;
using Astruk.Services;
using Unity;
using Unity.Lifetime;

namespace Astruk.DI
{
	public static class DependencyInjector
	{

		public static void RegisterServices(this UnityContainer container)
		{
			container.RegisterType<IMapService, MapService>(new TransientLifetimeManager());
		}
	}
}

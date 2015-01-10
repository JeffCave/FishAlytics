using System;
using System.Web;
using System.Security.Principal;

namespace Firehall
{
	public static class Globals
	{
		private static object locker = new object();
		public static Vius.Fishing.Data.FishingData Fishing {
			get {
				if(Context.Application["FishingData"] == null){
					lock(locker){
						if(Context.Application["FishingData"] == null){
							Context.Application["FishingData"] = Vius.Fishing.Data.FishingData.Instance;
						}
					}
				}
				return Context.Application["FishingData"] as Vius.Fishing.Data.FishingData;
			}
		}

		public static HttpContext Context {
			get {
				return HttpContext.Current;
			}
		}

		public sealed class ProviderCollection {
			//public ProviderCollection();
			public Vius.Authentication.ViusProfileProvider Profiler = new Vius.Authentication.ViusProfileProvider();
			public Vius.Authentication.ViusRoleProvider Roler = new Vius.Authentication.ViusRoleProvider();
			public Vius.Authentication.ViusMembershipProvider Memberer = new Vius.Authentication.ViusMembershipProvider();
			//public Vius.Authentication.IdentityProvider Identities = new Vius.Authentication.IdentityProvider();
			public Vius.Web.PageSiteMapProvider SiteMapper = new Vius.Web.PageSiteMapProvider();
		}

		public static ProviderCollection Providers{
			get {
				if(Context.Application["Providers"] == null){
					lock(locker){
						if(Context.Application["Providers"] == null){
							Context.Application["Providers"] = new ProviderCollection();
						}
					}
				}
				return Context.Application["Providers"] as ProviderCollection;
			}
		}

	}
}


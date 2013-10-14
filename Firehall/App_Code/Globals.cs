using System;
using System.Web;

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
	}
}


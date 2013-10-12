using System;

namespace Vius
{
	public static class Utils
	{
		private static object staticLocker = new object();
		private static Random pRandom = null;
		public static Random Random {
			get {
				if(pRandom == null){
					lock(staticLocker){
						if(pRandom == null){
							pRandom = new Random();
						}
					}
				}
				return pRandom;
			}
		}
	}
}


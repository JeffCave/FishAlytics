using System;

namespace Vius.Fishing.Data
{
	public class Trips
	{
		internal static Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;
		internal static readonly string TbName = "Fishing.Trips";

		internal Trips ()
		{
		}

		public Trip this[int pk] {
			get {
				return new Trip(this, pk);
			}
		}
	}
}


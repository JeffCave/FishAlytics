using System;
using System.Collections.Generic;
using System.Linq;

namespace Vius.Fishing.Data
{
	public class Trips
	{
		internal static Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;
		internal static readonly string TbName = "\"Fishing\".\"Trips\"";

		protected readonly int UpperPoolSize = 20;
		protected List<Trip> pool = new List<Trip>();

		internal Trips ()
		{
		}

		public Trip this[int pk] {
			get {
				Trip trip = pool
					.Where(i=>{return i.Id == pk;})
					.First();
				if(trip == null){
					trip = New();
				}
				return trip;
			}
		}

		public Trip New(){
			var trip = new Trip();
			pool.Add(trip);
			return trip;
		}

		protected void ReleaseTrips ()
		{
			if (pool.Count < UpperPoolSize) {
				return;
			}

			var numtoremove = pool.Count - (UpperPoolSize*0.9);
			var sortedpool = pool.OrderByDescending(i=> i.LastActivity);

			foreach (var trip in sortedpool) {
				if(numtoremove==0){
					break;
				}
				pool.Remove(trip);
				numtoremove--;
			}
		}

	}
}


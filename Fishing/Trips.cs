using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;

namespace Vius.Fishing.Data
{
	/// <summary>
	/// Trips.
	/// </summary>
	/// <remarks>
	/// <para>
	/// $Id$
	/// $URL$
	/// </para>
	/// </remarks>
	public class Trips
	{
		internal static Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;
		public static readonly string TbName = "\"Fishing\".\"Trips\"";

		protected readonly int UpperPoolSize = 20;
		protected List<Trip> pool = new List<Trip>();

		internal Trips ()
		{
		}

		public Trip this[int pk] {
			get {

				Trip trip = null;
				foreach(var t in pool){
					if(t.Id == pk){
						trip = t;
						break;
					}
				}
				if(trip == null){
					try{
						trip = new Trip(pk);
						ReleaseTrips();
						pool.Add(trip);
					} catch {
						trip = New();
					}
				}
				return trip;
			}
		}

		public Trip New(){
			var trip = new Trip();
			//TODO: Not sure if these should be added to the pool
			//pool.Add(trip);
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

		public List<Trip> Where (string clause = null, List<DbParameter> parameters = null)
		{
			if (parameters == null) {
				parameters = new List<DbParameter>();
			}
			if (string.IsNullOrEmpty(clause)) {
				clause = " 1=1 ";
				parameters.Clear();
			}

			var trips = new List<Trip>();
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					string.Format("select * from {0} where {1} ",
					              TbName,
					              clause
						);
				foreach(var p in parameters){
					cmd.Parameters.Add(p);
				}
				using(var rs = cmd.ExecuteReader()){
					while(rs.Read()){
						trips.Add(new Trip(rs));
					}
				}
			});
			return trips;
		}

	}
}


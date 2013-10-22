using System;

namespace Vius.Fishing.Data
{
	public class Trip
	{
		private Trips provider;
		internal DateTime LastActivity = DateTime.Now;
		private static Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		#region Initialize
		internal Trip (Trips provider)
		{
			this.provider = provider;
			Initialize();
		}

		internal Trip (Trips provider, int pk)
			:this(provider)
		{
			Load(pk);
		}

		internal Trip (Trips provider, System.Data.IDataRecord rec)
			:this(provider)
		{
			Load(rec);
		}

		private void Initialize ()
		{
		}
		#endregion

		#region DataElement
		public bool IsNew {
			get {
				LastActivity = DateTime.Now;
				throw new System.NotImplementedException();
			}
		}

		public bool IsDirty {
			get {
				LastActivity = DateTime.Now;
				throw new System.NotImplementedException();
			}
		}

		public  void Dispose ()
		{
			throw new System.NotImplementedException();
		}

		public  void Save ()
		{
			LastActivity = DateTime.Now;
			Db.UseCommand(cmd => {
				if(IsNew){
					cmd.CommandText = 
						"insert into "+Trips.TbName+" () " +
						"values ()";
				} else {
					cmd.CommandText = 
						"update ";
				}

			});
		}


		internal void Load (int primarykey)
		{
			Db.UseCommand(cmd => {
				cmd.CommandText = 
					"select * \n" +
					"from   " + Trips.TbName + " \n" +
					"where  TripId = :TripId";

				var p = cmd.CreateParameter();
				p.DbType = System.Data.DbType.Int32;
				p.ParameterName = ":TripId";
				p.Value = primarykey;

				using(var rs = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow)){
					if(rs.Read()){
						Load(rs);
					}
				}
			});
		}

		internal void Load (System.Data.IDataRecord rec)
		{
			Initialize();
			for(var fld=0;fld<rec.FieldCount; fld++) {
				if(rec.IsDBNull(fld)){
					continue;
				}
				switch(rec.GetName(fld).ToLower()){
					case "tripid":
						break;
					case "tripstart":
						break;
					case "tripend":
						break;
					case "notes":
						break;
				}
			}
		}


		#endregion
	}
}


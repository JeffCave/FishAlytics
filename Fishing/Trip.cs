using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

/*namespace Vius.Fishing
{
	[Serializable]
	public class Trip:ISerializable{

		public DateTime Start{
			get;set;
		}
		public DateTime End{
			get;set;
		}

		public Trip(SerializationInfo info, StreamingContext context)
		{
			foreach(var pInfo in System.Reflection.MemberInfo(typeof(this)).GetProperties()){	
			}
		}



	}
}*/

namespace Vius.Fishing.Data
{
	[Serializable]
	public class Trip
	{
		internal DateTime LastActivity = DateTime.Now;
		private static Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		public delegate void AccessedEventHandler(object sender, EventArgs e);

		public event AccessedEventHandler Accessed;

		#region Initialize
		internal Trip ()
		{
			Accessed += (sender, e) => {
					(sender as Trip).LastActivity = DateTime.UtcNow;
				};
			Initialize();
		}

		internal Trip (int pk)
			:this()
		{
			Load(pk);
		}

		internal Trip (System.Data.IDataRecord rec)
			:this()
		{
			Load(rec);
		}

		private void Initialize ()
		{
			DbParameter p;
			if (fields == null) {
				fields = new Dictionary<string, DbParameter>();
			}
			fields.Clear();

			p = Db.Factory.CreateParameter();
			p.ParameterName = "Id";
			p.DbType = DbType.Int32;
			p.Value = DBNull.Value;
			fields.Add("Id",p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "TripStart";
			p.DbType = DbType.DateTime;
			p.Value = DBNull.Value;
			fields.Add("TripStart",p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "TripEnd";
			p.DbType = DbType.DateTime;
			p.Value = DBNull.Value;
			fields.Add("TripEnd",p);

		}

		public void Dispose ()
		{
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			foreach (var fld in fields.Values) {
				info.AddValue(fld.ParameterName, fld.Value);
			}
		}
		#endregion

		#region Fields
		protected Dictionary<string,DbParameter> fields;

		public int Id {
			get {
				if (fields["Id"].Value == DBNull.Value) {
					return -1;
				}
				return (int)fields["Id"].Value;
			}
			private set {
				if(value<=0){
					fields["Id"].Value = DBNull.Value;
				} else {
					fields["Id"].Value = value;
				}
			}
		}

		public DateTime TripStart {
			get {
				if(fields["TripStart"].Value == DBNull.Value){
					return DateTime.MinValue;
				}
				return (DateTime)fields["TripStart"].Value;
			}
			set {
				if(value == DateTime.MinValue){
					fields["TripStart"].Value = DBNull.Value;
				} else{
					fields["TripStart"].Value = value;
				}
			}
		}

		public DateTime TripEnd {
			get {
				if(fields["TripEnd"].Value == DBNull.Value){
					return DateTime.MaxValue;
				}
				return (DateTime)fields["TripEnd"].Value;
			}
			set {
				if(value == DateTime.MaxValue){
					fields["TripEnd"].Value = DBNull.Value;
				} else {
					fields["TripEnd"].Value = value;
				}
			}
		}
		#endregion

		#region DataElement
		private static Dictionary<string,string> sql = null;
		protected static Dictionary<string,string> Sql {
			get {
				if(sql == null){
					sql = new Dictionary<string, string>();
					sql.Add("insert",
						"insert into "+Trips.TbName+" ( \n" +
						"    \"Start\", \n" +
						"    \"Finish\" \n" +
						") \n" +
						"values ( \n" +
						"    :TripStart, \n" +
						"    :TripEnd \n" +
						") \n" +
						"returning \"TripId\" \n"
					);
					sql.Add("update",
						"update " + Trips.TbName + " \n" +
						"set    \"Start\" = :TripStart, \n" +
						"       \"Finish\" = :TripEnd \n" +
						"where  \"TripId\" = :TripId \n"
					);
					sql.Add("load",
						"select * \n" +
						"from   " + Trips.TbName + " \n" +
						"where  \"TripId\" = :TripId"
					);
				}
				return sql;
			}
		}

		public bool IsNew {
			get {
				Accessed(this,null);
				return(fields["Id"].Value == DBNull.Value);
			}
		}

		public bool IsDirty {
			get {
				Accessed(this,null);
				//return origFields.Equals(fields);
				return true;
			}
		}

		public bool IsValid {
			get {
				try{
					Validate();
				} catch {
					return false;
				}
				return true;
			}
		}

		public void Validate ()
		{
			if (TripStart == DateTime.MinValue) {
				throw new Exception("We at least need to know when you went.");
			}
			if (this.TripStart >= this.TripEnd) {
				throw new Exception("Start must be before End");
			}
		}


		public void Save ()
		{
			Accessed(this,null);
			if (!IsDirty) {
				return;
			}
			Validate();
			Db.UseCommand(cmd => {
				cmd.CommandText = Sql[IsNew?"insert":"update"];
				cmd.Parameters.Clear();
				foreach(var p in fields){
					cmd.Parameters.Add(p.Value);
				}
				if(IsNew){
					Id = (int)cmd.ExecuteScalar();
				} else {
					cmd.ExecuteNonQuery();
				}

			});
		}


		internal void Load (int primarykey)
		{
			Db.UseCommand(cmd => {
				cmd.CommandText = Sql["load"];
				/*
				var p = cmd.CreateParameter();
				p.DbType = System.Data.DbType.Int32;
				p.ParameterName = ":TripId";
				p.Value = primarykey;
				*/
				foreach(var p in fields){
					cmd.Parameters.Add(p);
				}

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
						this.Id = rec.GetInt32(fld);
						break;
					case "start":
						this.TripStart = rec.GetDateTime(fld);
						break;
					case "finish":
						this.TripEnd = rec.GetDateTime(fld);
						break;
					case "notes":
						break;
				}
			}
		}

		#endregion
	}
}


using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Web;
using System.Web.Security;

namespace Vius.Fishing.Data
{
	[Table(Name="Fishing.Trip")]
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
			fields.Add(p.ParameterName,p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "TripStart";
			p.DbType = DbType.DateTime;
			p.Value = DBNull.Value;
			fields.Add(p.ParameterName,p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "TripEnd";
			p.DbType = DbType.DateTime;
			p.Value = DBNull.Value;
			fields.Add(p.ParameterName,p);

			p = Db.Factory.CreateParameter();
			p.ParameterName = "Fisherman";
			p.DbType = DbType.Int64;
			p.Value = DBNull.Value;
			fields.Add(p.ParameterName,p);

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

		[Column(IsPrimaryKey=true,Name="TripId")]
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

		[Column]
		public DateTime Start {
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

		[Column(Name="Finish")]
		public DateTime Finish{
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

		[Column(Name="Fisherman")]
		public long FishermanId {
			get {
				return (long)fields["Fisherman"].Value;
			}
			set {
				long id;
				try{
					id = long.Parse(fields["Fisherman"].Value.ToString());
				} catch {
					id = 0;
				}
				if(value != id){
					fields["Fisherman"].Value = value;
					_Fisherman = null;
				}
			}
		}
		#endregion

		#region Other Fields
		public TimeSpan Duration {
			get {
				return Finish.Subtract(Start);
			}
			set{
				Finish = Start.Add(value);
			}
		}

		private MembershipUser _Fisherman = null;
		private object locker = new object();
		public MembershipUser Fisherman {
			get {
				if(_Fisherman == null){
					lock(locker){
						if(_Fisherman == null){
							_Fisherman = Membership.GetUser(FishermanId);
						}
					}
				}
				return _Fisherman;
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
						"where  \"TripId\" = :TripId \n"
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
			if (Start == DateTime.MinValue) {
				throw new Exception("No Start given. We at least need to know when you went.");
			}
			if (Start >= Finish) {
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

				var p = cmd.CreateParameter();
				p.DbType = System.Data.DbType.Int32;
				p.ParameterName = ":TripId";
				p.Value = primarykey;
				cmd.Parameters.Add(p);


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
						this.Start = rec.GetDateTime(fld);
						break;
					case "finish":
						this.Finish = rec.GetDateTime(fld);
						break;
					case "fisherman":
						this.FishermanId = rec.GetInt64(fld);
						break;
				}
			}
		}

		#endregion
	}
}


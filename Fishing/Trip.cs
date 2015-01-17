using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Web;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace Vius.Fishing.Data
{
	[Serializable]
	[Table(Name="Fishing.Trip")]
	public class Trip:ISerializable
	{
		internal DateTime LastActivity = DateTime.Now;

		public delegate void AccessedEventHandler(object sender, EventArgs e);

		#region Initialize
		public Trip ()
		{
		}
		#endregion

		#region Fields
		protected Dictionary<string,DbParameter> fields;

		[Column(IsPrimaryKey=true,Name="TripId")]
		public int Id {
			get {
				if (fields[":Id"].Value == DBNull.Value) {
					return -1;
				}
				return (int)fields[":Id"].Value;
			}
			private set {
				if(value<=0){
					fields[":Id"].Value = DBNull.Value;
				} else {
					fields[":Id"].Value = value;
				}
			}
		}

		[Column]
		public DateTime Start {
			get {
				if(fields[":TripStart"].Value == DBNull.Value){
					return DateTime.MinValue;
				}
				return (DateTime)fields[":TripStart"].Value;
			}
			set {
				if(value == DateTime.MinValue){
					fields[":TripStart"].Value = DBNull.Value;
				} else{
					fields[":TripStart"].Value = value;
				}
			}
		}

		[Column(Name="Finish")]
		public DateTime Finish{
			get {
				if(fields[":TripEnd"].Value == DBNull.Value){
					return DateTime.MaxValue;
				}
				return (DateTime)fields[":TripEnd"].Value;
			}
			set {
				if(value == DateTime.MaxValue){
					fields[":TripEnd"].Value = DBNull.Value;
				} else {
					fields[":TripEnd"].Value = value;
				}
			}
		}

		[Column(Name="Fisherman")]
		public long FishermanId {
			get {
				if(fields[":Fisherman"].Value == DBNull.Value){
					return -1;
				}
				return (long)fields[":Fisherman"].Value;
			}
			set {
				long id=0;
				try{
					id = Convert.ToInt64(fields[":Fisherman"].Value);
				} catch {
					id = 0;
				}
				if(value != id){
					fields[":Fisherman"].Value = value;
					_Fisherman = null;
				}
			}
		}
		#endregion

		#region Calculated Fields
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

		public string Serialize ()
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			string json = "";
			json = serializer.Serialize(this);
			json = Regex.Replace(json,@"\""\\/Date\((-?\d+)\)\\/\""","new Date($1)");
			return json;
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			foreach (var fld in fields.Values) {
				info.AddValue(fld.ParameterName, fld.Value);
			}
		}

	}
}


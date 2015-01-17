using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using Vius.Data;

namespace Vius.Fishing.Data
{
	[Serializable]
	[Table(Name="Fishing.Catches")]
	public class Catch: ISerializable
	{
		#region Initialize
		public Catch ()
		{
		}
		#endregion

		#region Fields
		private int catchId = 0;
		private DateTime time = DateTime.MinValue;

//		[Column(DbType="bigserial",CanBeNull=false,IsDbGenerated=true,IsPrimaryKey=true)]
		[Column]
		public int CatchId {
			get{ return catchId; }
			set{catchId = value;}
		}

		[Column(CanBeNull=false)]
		public DateTime Time {
			get{ return time;}
			set{ time = value;}
		}

//		[Column(Storage="Species", DbType="NVarChar(50)", CanBeNull=false)]
//		public string Species {
//			get;
//			set;
//		}
//
//		private User fisherman;
//		[Association(Name="fk_Catch_User", Storage="fisherman", ThisKey="Fisherman", IsForeignKey=true)]
//		[Column]
//		public int Fisherman{ get; set;}
//		{
//			get
//			{
//				return this._Customer.Entity;
//			}
//			set
//			{
//				Customer previousValue = this._Customer.Entity;
//				if (((previousValue != value) 
//					|| (this._Customer.HasLoadedOrAssignedValue == false)))
//				{
//					this.SendPropertyChanging();
//					if ((previousValue != null))
//					{
//						this._Customer.Entity = null;
//						previousValue.Orders.Remove(this);
//					}
//					this._Customer.Entity = value;
//					if ((value != null))
//					{
//						value.Orders.Add(this);
//						this._CustomerID = value.CustomerID;
//					}
//					else
//					{
//						this._CustomerID = default(string);
//					}
//					this.SendPropertyChanged("Customer");
//				}
//			}
//		}
		#endregion

		#region "DataElement"
		public void Validate ()
		{
		}
		#endregion

		#region Serializable
		/// <summary>
		/// Implement this method to serialize data. The method is called  
		/// on serialization.
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Use the AddValue method to specify serialized values.
			info.AddValue("CatchId", CatchId, this.CatchId.GetType());
			info.AddValue("Time", Time, this.Time.GetType());
//			info.AddValue("Species", Species, this.Species.GetType());
		}
		/// <summary>
		/// The special constructor is used to deserialize a <see cref="Vius.Fishing.Data.Catch"/> object.
		/// </summary>
		/// <param name="info">serialized properties</param>
		/// <param name="context">context</param>
		public Catch(SerializationInfo info, StreamingContext context)
		{
			// Reset the property value using the GetValue method.
			this.CatchId = (int)info.GetValue("CatchId", this.CatchId.GetType());
			this.Time = (DateTime)info.GetValue("Time", this.Time.GetType());
//			this.Species = (string)info.GetValue("Species", this.Species.GetType());
		}
		#endregion

	}
}


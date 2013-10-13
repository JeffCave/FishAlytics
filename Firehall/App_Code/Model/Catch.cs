using System;
using System.Collections.Generic;

namespace Firehall.Data.Fishing
{
	public class Catch: Vius.BaseDataElement<int>
	{
		private bool pIsDirty;

		#region Initialize
		public Catch ()
		{
		}

		public Catch (int catchid)
			:this()
		{
			Load(CatchId);
		}
		#endregion

		#region Fields
		public int CatchId {
			get {
				return primarykey;
			}
			protected set {
				primarykey = value;
				pIsDirty = true;
			}
		}

		private DateTime pTime = DateTime.Now;
		public DateTime Time {
			get {
				return pTime;
			}
			set {
				pTime = value;
				pIsDirty = true;
			}
		}

		private string pSpecies = "";
		public string Species {
			get {
				return pSpecies;
			}
			set {
				pSpecies = value;
				pIsDirty = true;
			}
		}
		#endregion

		#region "DataElement"
		public override bool IsNew {
			get {
				return (primarykey < 0);
			}
		}

		public override bool IsDirty {
			get {
				return pIsDirty;
			}
		}

		public override void Create ()
		{
			List<string> cmds = new List<string>(){
				"create table tFishCatch(" +
				"    CatchId int primary key autgenerate, " +
				"    Time Timestamp not null, " +
				"    UserId int not null" +
				")"
			};
			Db.ExecuteCommand(cmds);
		}

		public override void Delete ()
		{
			Db.UseCommand(cmd => {
				cmd.CommandText =
					"delete " +
					"from tFishCatch " +
					"where catchid = @catchid";

				var param = cmd.CreateParameter();
				param.ParameterName = "@catchid";
				param.Value = CatchId;
				param.DbType = System.Data.DbType.Int32;

				cmd.Parameters.Add(param);

				cmd.ExecuteNonQuery();

			});
		}

		public override void Dispose ()
		{
		}

		public override void Load ()
		{

		}

		public override void Load (int primarykey)
		{
			throw new System.NotImplementedException();
		}

		internal void Load (System.Data.IDataRecord rec)
		{
			for (var fld=0; fld<rec.FieldCount; fld++) {
				switch(rec.GetName(fld).ToLower()){
					case "catchid":
						this.CatchId = rec.GetInt32(fld);
						break;
				}
			}
		}

		public override void Save ()
		{
			throw new System.NotImplementedException();
		}

		public override void Validate ()
		{
			base.Validate();
		}
		#endregion
	}
}


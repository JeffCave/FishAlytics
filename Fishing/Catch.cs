using System;
using System.Collections.Generic;

using Vius.Data;

namespace Vius.Fishing.Data
{
	public class Catch: Vius.BaseDataElement<int>
	{
		private bool pIsDirty;
		private Catches parent;

		#region Initialize
		internal Catch (Catches parent)
		{
			this.parent = parent;
		}

		internal Catch (Catches parent, int catchid)
			:this(parent)
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
			List<string> cmds = new List<string>();
			cmds.Add(
				"create table Fishing.Catch(" +
				"    CatchId bigserial primary key autgenerate, " +
				"    Time Timestamp not null, " +
				"    Fisherman bigint not null references public.User(UserId)," +
				"    Species varchar(50)," +
				"    primary key (CatchId)" +
				")"
				);
			Db.ExecuteCommand(cmds);
		}

		public override void Delete ()
		{

		}

		public override void Dispose ()
		{
		}

		public override void Load ()
		{
			Db.UseCommand(cmd => {
				cmd.CommandText =
					"select * " +
					"from tFishCatch " +
					"where catchid = @catchid";

				var param = cmd.CreateParameter();
				param.ParameterName = "@catchid";
				param.Value = CatchId;
				param.DbType = System.Data.DbType.Int32;

				cmd.Parameters.Add(param);

				using(var rs = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow)){
					Load(rs);
					rs.Close();
				}

			});

		}

		public override void Load (int catchid)
		{
			CatchId = catchid;
			Load();
		}

		internal void Load (System.Data.IDataRecord rec)
		{
			for (var fld=0; fld<rec.FieldCount; fld++) {
				if(rec.IsDBNull(fld)){
					continue;
				}
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


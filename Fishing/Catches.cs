using System;
using System.Collections.Generic;
using System.Linq;

namespace Vius.Fishing.Data
{
	public class Catches:Vius.Data.IDataCollection<Catch>
	{
		private static readonly Vius.Data.DataProvider Db = Vius.Data.DataProvider.Instance;

		public Catches ()
		{
		}

		public Catch this[int pk] {
			get {
				return new Catch(pk);
			}
			set {

			}
		}

		public int IndexOf(Vius.Fishing.Data.Catch item){
			return item.CatchId;
		}

		public void Remove (int catchid)
		{
			Db.UseCommand(cmd => {
				cmd.CommandText =
					"delete " +
					"from tFishCatch " +
					"where catchid = @catchid";

				var param = cmd.CreateParameter();
				param.ParameterName = "@catchid";
				param.Value = catchid;
				param.DbType = System.Data.DbType.Int32;

				cmd.Parameters.Add(param);

				cmd.ExecuteNonQuery();

			}
			);

		}

		public void Remove (Vius.Fishing.Data.Catch item)
		{

		}

		public void Insert(int pk, Vius.Fishing.Data.Catch item){
			item.Save();
		}

		public int Count {
			get {
				int count = 0;
				Db.UseCommand(cmd => {
					cmd.CommandText = "select count(*) from Fishing.Catch";
					count = (int)cmd.ExecuteScalar();
				});
				return count;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		public void Add (Vius.Fishing.Data.Catch item)
		{
			if (item.IsNew) {
				item.Save();
			}
		}

	}
}


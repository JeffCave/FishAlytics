using System;
using System.Collections.Generic;

namespace Vius.Data
{
	public class BaseDataCollection<PK,ELEM>
		: System.Collections.Generic.List<ELEM>
			, Vius.Data.IDataCollection<ELEM>
			where ELEM : Vius.Data.IDataElement<PK>
	{
		public BaseDataCollection ()
		{
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

	}
}


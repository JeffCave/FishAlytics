using System;

namespace Vius
{
	public class Tuple<T1,T2>: Vius.Pair<T1,T2>{}

	public class Pair<T1,T2>
	{
		public T1 Key;
		public T2 Value;

		public Pair ()
		{
		}

		public Pair(T1 key, T2 value){
			this.Key = key;
			this.Value = value;
		}
	}
}


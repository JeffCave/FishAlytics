using System;

namespace Vius
{
	public class PooledObject<T>:IDisposable
	{
		protected internal T item = default(T);
		protected internal ObjectPool<T> pool = null;
		internal int numuses = 0;
		protected internal DateTime? locked = null;

		public T Item {
			get {
				return item;
			}
		}

		public int NumUses {
			get {
				return numuses;
			}
		}

		PooledObject (ObjectPool<T> owner, T item)
		{
			this.item = item;
			this.pool = owner;
		}

		public void Dispose ()
		{
			pool.Return(this);
		}
	}
}


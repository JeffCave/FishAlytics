using System;

namespace Vius
{
	public class PooledObject<T>:IDisposable
	{
		protected internal T item = default(T);
		protected internal ObjectPool<T> pool = null;
		protected internal int numuses = 0;
		protected internal DateTime? locked = null;
		protected internal bool disposed = false;

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

		internal PooledObject (ObjectPool<T> owner, T item)
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


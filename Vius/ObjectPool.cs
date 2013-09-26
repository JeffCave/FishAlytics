using System;
using System.Collections.Generic;

namespace Vius
{
	/// <summary>
	/// Object pool.
	/// </summary>
	/// <remarks>
	/// http://commons.apache.org/proper/commons-pool/apidocs/org/apache/commons/pool/impl/GenericObjectPool.html
	/// </remarks>
	public abstract class ObjectPool<T>:IDisposable
	{
		#region "Private Variables"
		private object locker = new object();

		private Queue<PooledObject<T>> idle = new Queue<PooledObject<T>>();
		private List<PooledObject<T>> active = new List<PooledObject<T>>();

		#endregion

		#region "Abstract Methods"

		public abstract T CreateObject();
		public abstract bool EvictionTest(PooledObject<T> pooleditem);
		public abstract bool ValidationTest(PooledObject<T> item);
		public abstract void Disposer(T item);

		#endregion


		#region Configuration

		//set the defaults for several values
		private int pMaxActive = 10;
		private int maxidle = 10;
		private int minidle = 0;
		private int pTestsPerEviction = 10;

		/// <summary>
		/// Returns the maximum number of objects that can be allocated by the pool (checked out to clients, or idle awaiting checkout) at a given time.
		/// </summary>
		/// <value>
		/// The maximum allowed active.
		/// </value>
		public int MaxActive {
			get {
				return pMaxActive;
			}
			set {
				pMaxActive = value;
			}
		}

		/// <summary>
		/// Returns the cap on the number of "idle" instances in the pool.
		/// </summary>
		/// <value>
		/// The max idle.
		/// </value>
		public int MaxIdle {
			get {
				if(maxidle < minidle){
					return minidle;
				}
				return maxidle;
			}
			set {
				maxidle = value;
			}
		}

		/// <summary>
		/// Returns the minimum number of objects allowed in the pool before the evictor thread (if active) spawns new objects.
		/// </summary>
		/// <value>
		/// The minimum idle.
		/// </value>
		public int MinIdle {
			get {
				return minidle;
			}
			set {
				if(value < 0){
					value = 0;
				}
				minidle = value;
			}
		}

		/// <summary>
		/// the max number of objects to examine during each run of the idle object evictor thread (if any).
		/// </summary>
		/// <value>
		/// the max number of objects to examine during each run of the idle object evictor thread (if any).
		/// </value>
		public int TestsPerEvictionRun {
			get {
				return pTestsPerEviction;
			}
			set {
				if(value < 1){
					value = 1;
				}
				pTestsPerEviction = value;
			}
		}

		#endregion

		/// <summary>
		/// The number of instances currently borrowed from this pool.
		/// </summary>
		/// <value>
		/// The number active.
		/// </value>
		public int NumActive {
			get {
				return active.Count;
			}
		}

		/// <summary>
		/// The total number of instances, both active and idle
		/// </summary>
		/// <value>
		/// Total Instances
		/// </value>
		public int Count {
			get {
				return NumActive + NumIdle;
			}
		}

		/// <summary>
		/// Borrows an object from the pool.
		/// </summary>
		public PooledObject<T> Borrow ()
		{
			PooledObject<T> item = null;
			lock (locker) {
				// borrowing moves the item from the idle state to the active state,
				// check that we have room for that
				if(MaxActive >= NumActive) {
					throw new Exception("Too many active instances");
				}

				//if we have an instance already, use that, otherwise create one
				while(item==null && idle.Count>0){
					item = idle.Dequeue();

					//has this been disposed of by the eviction routines?
					if(item.disposed){
						item = null;
					}

					//it is still active, but in the wrong queue?
					if(item.locked != null){
						//put it in the correct queue, and discard it
						active.Add(item);
						item = null;
					}
				}

				// we didn't find one in the idle queue so create a new one
				if(item == null){
					item = new PooledObject<T>(this,CreateObject());
				}

				//house keeping on the instance
				item.locked = DateTime.Now;
				item.numuses++;

				//add it to the active items
				active.Add(item);
				return item;
			}
		}

		/// <summary>
		/// Returns an object instance to the pool.
		/// </summary>
		/// <param name='item'>Item.</param>
		public void Return (PooledObject<T> item)
		{
			lock (locker) {
				active.Remove(item);
				item.locked = null;
				idle.Enqueue(item);
			}
		}

		/// <summary>
		/// Clears any objects sitting idle in the pool by removing them from the idle instance pool and then invoking the configured <see cref="PoolableObjectFactory.destroyObject(Object)"/> method on each idle instance.
		/// </summary>
		public void Clear()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Vius.ObjectPool`1"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="Vius.ObjectPool`1"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Vius.ObjectPool`1"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="Vius.ObjectPool`1"/> so the garbage
		/// collector can reclaim the memory that the <see cref="Vius.ObjectPool`1"/> was occupying.
		/// </remarks>
		public void Dispose ()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Perform numTests idle object eviction tests, evicting 
		/// examined objects that meet the criteria for eviction.
		/// </summary>
		public void Evict ()
		{
			List<PooledObject<T>> tests = new List<PooledObject<T>>(idle.ToArray());
			foreach (var item in tests) {
				if (EvictionTest(item)) {
					Invalidate(item);
				}
			}
		}

		/// <summary>
		/// Return the number of instances currently idle in this pool.
		/// </summary>
		/// <value>
		/// The number idle.
		/// </value>
		public int NumIdle {
			get {
				return idle.Count;
			}
		}

		/// <summary>
		/// Invalidates an object from the pool.
		/// </summary>
		/// <param name='item'>
		/// Item.
		/// </param>
		public void Invalidate (PooledObject<T> item)
		{
			lock (locker) {
				Disposer(item.Item);
				item.disposed = true;
			}
		}

		/// <summary>
		/// Start the eviction thread or service, or when delay is non-positive, stop it if it is already running.
		/// </summary>
		/// <param name='delay'>
		/// Delay.
		/// </param>
		protected  void	startEvictor (long delay)
		{
			throw new NotImplementedException();
		}


		/*
		 * 
		 * 
 long	getSoftMinEvictableIdleTimeMillis() 
          Returns the minimum amount of time an object may sit idle in the pool before it is eligible for eviction by the idle object evictor (if any), with the extra condition that at least "minIdle" amount of object remain in the pool.
 boolean	getTestOnBorrow() 
          When true, objects will be validated before being returned by the borrowObject() method.
 boolean	getTestOnReturn() 
          When true, objects will be validated before being returned to the pool within the returnObject(T).
 boolean	getTestWhileIdle() 
          When true, objects will be validated by the idle object evictor (if any).
 long	getTimeBetweenEvictionRunsMillis() 
          Returns the number of milliseconds to sleep between runs of the idle object evictor thread.
 byte	getWhenExhaustedAction() 
          Returns the action to take when the borrowObject() method is invoked when the pool is exhausted (the maximum number of "active" objects has been reached).
 void	setConfig(GenericObjectPool.Config conf) 
          Sets my configuration.
 void	setMaxActive(int maxActive) 
          Sets the cap on the number of objects that can be allocated by the pool (checked out to clients, or idle awaiting checkout) at a given time.
 void	setMaxIdle(int maxIdle) 
          Sets the cap on the number of "idle" instances in the pool.
 void	setMaxWait(long maxWait) 
          Sets the maximum amount of time (in milliseconds) the borrowObject() method should block before throwing an exception when the pool is exhausted and the "when exhausted" action is WHEN_EXHAUSTED_BLOCK.
 void	setMinEvictableIdleTimeMillis(long minEvictableIdleTimeMillis) 
          Sets the minimum amount of time an object may sit idle in the pool before it is eligible for eviction by the idle object evictor (if any).
 void	setSoftMinEvictableIdleTimeMillis(long softMinEvictableIdleTimeMillis) 
          Sets the minimum amount of time an object may sit idle in the pool before it is eligible for eviction by the idle object evictor (if any), with the extra condition that at least "minIdle" object instances remain in the pool.
 void	setTestOnBorrow(boolean testOnBorrow) 
          When true, objects will be validated before being returned by the borrowObject() method.
 void	setTestOnReturn(boolean testOnReturn) 
          When true, objects will be validated before being returned to the pool within the returnObject(T).
 void	setTestWhileIdle(boolean testWhileIdle) 
          When true, objects will be validated by the idle object evictor (if any).
 void	setTimeBetweenEvictionRunsMillis(long timeBetweenEvictionRunsMillis) 
          Sets the number of milliseconds to sleep between runs of the idle object evictor thread.
 void	setWhenExhaustedAction(byte whenExhaustedAction) 
          Sets the action to take when the borrowObject() method is invoked when the pool is exhausted (the maximum number of "active" objects has been reached).

		 */
	}
}


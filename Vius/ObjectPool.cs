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
	public class ObjectPool<T>:IDisposable
	{
		private object locker = new object();

		private Queue<PooledObject<T>> idle = new Queue<PooledObject<T>>();
		private List<PooledObject<T>> active = new List<PooledObject<T>>();

		private int maxactive = 10;
		private int maxidle = 10;
		private int minidle = 0;
		private int testspereviction = 10;

		/// <summary>
		/// Returns the maximum number of objects that can be allocated by the pool (checked out to clients, or idle awaiting checkout) at a given time.
		/// </summary>
		/// <value>
		/// The maximum allowed active.
		/// </value>
		public int MaxActive {
			get {
				return maxactive;
			}
			set {
				maxactive = value;
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
				minidle = value;
			}
		}


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
		/// Borrows an object from the pool.
		/// </summary>
		public PooledObject<T> Borrow ()
		{
			lock (locker) {
				PooledObject<T> item = idle.Dequeue();

				item.locked = DateTime.Now;
				item.numuses++;

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
				idle.Enqueue(item);
				item.locked = null;
			}
		}

		/// <summary>
		/// Clears any objects sitting idle in the pool by removing them from the idle instance pool and then invoking the configured <see cref="PoolableObjectFactory.destroyObject(Object)"/> method on each idle instance.
		/// </summary>
		public void Clear()
		{
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
		}

		/// <summary>
		/// Perform numTests idle object eviction tests, evicting examined objects that meet the criteria for eviction.
		/// </summary>
		public void Evict()
		{
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
		/// Returns the max number of objects to examine during each run of the idle object evictor thread (if any).
		/// </summary>
		/// <value>
		/// The number tests per eviction run.
		/// </value>
		public int TestsPerEvictionRun {
			get {
				return testspereviction;
			}
		}

		/// <summary>
		/// Invalidates an object from the pool.
		/// </summary>
		/// <param name='item'>
		/// Item.
		/// </param>
		public void Invalidate (T item)
		{
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
 void	setFactory(PoolableObjectFactory<T> factory) 
          Deprecated. to be removed in version 2.0
 void	setMaxActive(int maxActive) 
          Sets the cap on the number of objects that can be allocated by the pool (checked out to clients, or idle awaiting checkout) at a given time.
 void	setMaxIdle(int maxIdle) 
          Sets the cap on the number of "idle" instances in the pool.
 void	setMaxWait(long maxWait) 
          Sets the maximum amount of time (in milliseconds) the borrowObject() method should block before throwing an exception when the pool is exhausted and the "when exhausted" action is WHEN_EXHAUSTED_BLOCK.
 void	setMinEvictableIdleTimeMillis(long minEvictableIdleTimeMillis) 
          Sets the minimum amount of time an object may sit idle in the pool before it is eligible for eviction by the idle object evictor (if any).
 void	setMinIdle(int minIdle) 
          Sets the minimum number of objects allowed in the pool before the evictor thread (if active) spawns new objects.
 void	setNumTestsPerEvictionRun(int numTestsPerEvictionRun) 
          Sets the max number of objects to examine during each run of the idle object evictor thread (if any).
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
protected  void	startEvictor(long delay) 
          Start the eviction thread or service, or when delay is non-positive, stop it if it is already running.
 
		 */
	}
}


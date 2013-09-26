using System;
using System.Collections.Generic;

namespace FirehallTests.General{
	public class WebDriverPool:IDisposable{

		private int maxdriver = 10;
		private Dictionary<SupportedDrivers,List<PooledWebDriver>> alldriver = new Dictionary<SupportedDrivers, List<PooledWebDriver>>();

		private object locallocker = new object();
		private static object staticlocker = new object();

		public enum SupportedDrivers
		{
			Chrome
		}

		private static WebDriverPool instance = null;
		public static WebDriverPool Instance {
			get {
				if(instance == null){
					lock(staticlocker){
						if(instance == null){
							instance = new WebDriverPool();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FirehallTests.General.WebDriverPool"/> class.
		/// </summary>
		private WebDriverPool ()
		{
			foreach(SupportedDrivers d in Enum.GetValues( typeof(SupportedDrivers))){
				alldriver.Add(d,new List<PooledWebDriver>());
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="FirehallTests.General.WebDriverPool"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="FirehallTests.General.WebDriverPool"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="FirehallTests.General.WebDriverPool"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="FirehallTests.General.WebDriverPool"/> so the garbage collector can reclaim the memory that the
		/// <see cref="FirehallTests.General.WebDriverPool"/> was occupying.
		/// </remarks>
		public void Dispose ()
		{
			foreach (List<PooledWebDriver> drivers in alldriver.Values) {
				foreach (PooledWebDriver driver in drivers) {
					driver.driver.Quit();
				}
			}
		}

		/// <summary>
		/// Checkout this instance.
		/// </summary>
		public PooledWebDriver Checkout(){
			return Checkout(SupportedDrivers.Chrome);
		}

		/// <summary>
		/// Checkout the specified type.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		public PooledWebDriver Checkout (SupportedDrivers type)
		{
			PooledWebDriver rtn = null;
			var drivers = alldriver[type];

			lock (locallocker) {
				foreach (PooledWebDriver driver in drivers) {
					if (driver.locked != null) {
						rtn = driver;
						break;
					}
				}

				if (rtn == null) {
					if (this.maxdriver <= drivers.Count) {
						throw new Exception("Cannot create another instance of " + type.GetType().Name);
					}
					rtn = new PooledWebDriver(type);
					rtn.parent = this;
					drivers.Add(rtn);
				}

				rtn.locked = DateTime.Now;
			}
			return rtn;
		}

		/// <summary>
		/// Checkin the specified driver.
		/// </summary>
		/// <param name='driver'>
		/// Driver.
		/// </param>
		public void Checkin (PooledWebDriver driver)
		{
			if (driver.used > 100) {
				alldriver[SupportedDrivers.Chrome].Remove(driver);
				driver.driver.Close();
			}
			driver.driver.Quit();
			driver.locked = null;
		}

	}
}
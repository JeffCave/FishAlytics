using System;
using OpenQA.Selenium;

namespace FirehallTests.General
{
	/// <summary>
	/// Pooled web driver.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class PooledWebDriver:IDisposable
	{
		public IWebDriver driver;
		public int used;
		public DateTime? locked;
		public WebDriverPool parent;

		public PooledWebDriver ()
		{
			driver = null;
			used = 0;
			locked = null;
			parent = null;
		}

		public PooledWebDriver (IWebDriver driver):this()
		{
			this.driver = driver;
		}

		public PooledWebDriver (WebDriverPool.SupportedDrivers type):this()
		{
			CreateDriver(type);
		}

		public void Dispose ()
		{
			parent.Checkin(this);
		}
		
		protected void CreateDriver (WebDriverPool.SupportedDrivers type)
		{
			driver = new OpenQA.Selenium.Chrome.ChromeDriver();
		}


	}
}


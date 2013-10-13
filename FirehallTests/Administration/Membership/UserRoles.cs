using System;
using System.Threading;

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium;

using FirehallTests.General;

namespace FirehallTests.Administration.Membership
{
	/// <summary>
	/// User roles.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	[TestFixture()]
	public class UserRoles:FirehallTests.General.WebPageFixture
	{
		protected readonly static By RoleList = By.CssSelector("ul.DataList");
		protected readonly static By UserList = By.CssSelector("ul.DataList");

		public enum RoleStates{
			Off = 0,
			On = 1,
			Random = 2
		}

		protected override string PageUrl {
			get {
				return "/Administration/Membership/UserRoles.aspx";
			}
		}

		/// <summary>
		/// General look and feel of the page
		/// </summary>
		/// <remarks>
		/// <para></para>
		/// </remarks>
		[Test]
		public void Load ()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates that the roles can be changed for the given user
		/// </summary>
		/// <param name='state'>
		/// State.
		/// </param>
		/// <remarks>
		/// </remarks>
		[TestCase(RoleStates.Off)]
		[TestCase(RoleStates.On)]
		[TestCase(RoleStates.Random)]
		public void SetRoles (RoleStates state)
		{
			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				//Console.Out.WriteLine("AddRemoveCapability[01]:");
				Helper.Login(driver, "Jeff", "test");
				driver.Url = this.FullUrl;
				//Console.Out.WriteLine("AddRemoveCapability[02]:");

				//Console.Out.WriteLine("-AddRemoveCapability[03]:");
				bool checkedall = false;
				while (!checkedall) {
					var ul = driver.FindElement(RoleList);
					//Console.Out.WriteLine("--AddRemoveCapability[04]:");
					checkedall = true;
					foreach (var li in ul.FindElements(By.TagName("li"))) {
						//Console.Out.WriteLine("---AddRemoveCapability[05]:" + li.Text);
						//find the input and click on it
						var input = li.FindElement(By.TagName("input"));
						//Console.Out.WriteLine("---AddRemoveCapability[06]:");
						string checkedval = string.Format("{0}", input.GetAttribute("checked"));
						//Console.Out.WriteLine("---AddRemoveCapability[07]:'" + checkedval +"' ("+state+")");
						bool needsclick = 
								//either it matches the desired on/off state
								(state==RoleStates.On) == string.IsNullOrEmpty(checkedval)
								// or the desired state is "random" and it matches a randomly generated number
								|| state == (RoleStates)Vius.Utils.Random.Next(2,3);
						if ( needsclick ) {
							//Console.Out.WriteLine("---AddRemoveCapability[08]: changing state");
							input.Click();
							Thread.Sleep(100);
							WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
							wait.Until(drv => drv.FindElement(By.TagName("footer")));
							//Console.Out.WriteLine("---AddRemoveCapability[09]: state changed");
							checkedall = false;
							break;
						}
					}
					//Console.Out.WriteLine("---AddRemoveCapability[10]:");
				}
				//Console.Out.WriteLine("---AddRemoveCapability[11]: finished");
			}
		}
		

		/// <summary>
		/// Can the user be set by url parameter
		/// </summary>
		[Test]
		public void UserParameter ()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Change the user from the page
		/// </summary>
		/// <remarks>
		/// <para></para>
		/// </remarks>
		[Test]
		public void ChangeUser ()
		{
			throw new NotImplementedException();
		}
	}
}


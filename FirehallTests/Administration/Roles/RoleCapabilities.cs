using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selenium;

using FirehallTests.General;

namespace FirehallTests.Administration.Roles
{
	[TestFixture]
	public class RoleCapabilities:FirehallTests.General.WebPageFixture
	{
		protected readonly static By CapabilityList = By.CssSelector("ul.DataList");
		protected readonly static By RoleLabel = By.Id("ctl00_MainContent_RoleName");
		protected readonly static string RoleName = "Firefighter";

		protected override string PageUrl {
			get {
				return "/Administration/Roles/RoleCapabilities.aspx?r=" + RoleName;
			}
		}

		/// <summary>
		/// Validate Page loads reasonably
		/// </summary>
		/// <remarks>
		/// The page is supposed to load with a title (rolename), and a 
		/// list of capabilities. The capabilities are to be checkboxes 
		/// that can be selected or deselected.
		/// 
		/// Tests:
		/// <list type="number">
		/// <item><description>Role label is present</description></item>
		/// <item><description>list of capabilities</description></item>
		/// <item><description>list of capabilities is checkboxes</description></item>
		/// </list>
		/// </remarks>
		[Test]
		public void Loaded ()
		{
			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				//Console.Out.WriteLine("Loaded[1]:");
				Helper.Login(driver,"Jeff","test");
				//Console.Out.WriteLine("Loaded[2]:");

				driver.Url = this.FullUrl;
				//Console.Out.WriteLine("Loaded[3]:"+driver.Url);
				Assert.AreEqual(this.FullUrl,driver.Url,"URL changed unexpectedly");
				//Console.Out.WriteLine("Loaded[4]:");

				elem = driver.FindElement(RoleLabel);
				//Console.Out.WriteLine("Loaded[5]:" + elem.Text);
				Assert.AreEqual(RoleName, elem.Text,"Page is not labelled with Rolename");
				//Console.Out.WriteLine("Loaded[6]:finished");
			}
		}

		/// <summary>
		/// Validate Page loads reasonably
		/// </summary>
		/// <remarks>
		/// For the given role, you should be able to add capabilities
		/// 
		/// There is an edge case where adding all items is problematic
		/// 
		/// Tests:
		/// <list type="number">
		/// <item><description>Activate all capabilities (one at a time)</description></item>
		/// <item><description>Deactivate all capabilities (one at a time)</description></item>
		/// </list>
		/// </remarks>
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void SetCapabilities (bool state)
		{
			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				//Console.Out.WriteLine("AddRemoveCapability[01]:");
				Helper.Login(driver, "Jeff", "test");
				driver.Url = this.FullUrl;
				//Console.Out.WriteLine("AddRemoveCapability[02]:");

				//Console.Out.WriteLine("-AddRemoveCapability[03]:");
				bool checkedall = false;
				while(!checkedall){
					var ul = driver.FindElement(CapabilityList);
					//Console.Out.WriteLine("--AddRemoveCapability[04]:");
					checkedall = true;
					foreach(var li in ul.FindElements(By.TagName("li"))){
						//Console.Out.WriteLine("---AddRemoveCapability[05]:" + li.Text);
						//find the input and click on it
						var input = li.FindElement(By.TagName("input"));
						//Console.Out.WriteLine("---AddRemoveCapability[06]:");
						string checkedval = string.Format("{0}", input.GetAttribute("checked"));
						//Console.Out.WriteLine("---AddRemoveCapability[07]:'" + checkedval +"' ("+state+")");
						if(state == string.IsNullOrEmpty(checkedval)){
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
	}
}


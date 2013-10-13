using System;
using System.Collections.Generic;

using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.PageObjects;
using Selenium;

using FirehallTests.General;

namespace FirehallTests.Administration.Membership
{
	/// <summary>
	/// Member list.
	/// </summary>
	/// <remarks>
	/// The memberlist page allows for control of the adding members 
	/// from an administrative point of view.
	/// 
	/// $Id$
	/// $URL$
	/// </remarks>
	[TestFixture()]
	public class MemberList : WebPageFixture
	{
		protected By ByTitle = By.Id("ctl00_PageHtmlTitle");
		protected By ByMemberList = By.Id("ctl00_MainContent_UserGrid");

		protected static List<string> titles = new List<string>(){
			" ",
			"User",
			"Last",
			"Email",
			"Comment"
		};

		protected override string PageUrl {
			get {
				return "/Administration/Membership/MemberList.aspx";
			}
		}

		/// <summary>
		/// Test that the page has rendered in a reasonable fashion.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The page should load with a list of users. Each user row 
		/// should include User, Last, Email, Comments, and Action. 
		/// The action column should have edit, delete, and role edit
		/// buttons.
		/// </para>
		/// <para>
		/// Tests:
		/// <list type="bullet">
		/// <item><description>Title "Members" is present</description></item>
		/// <item><description>Table is present</description></item>
		/// <item>
		///  <description>
		///   Column Headings: 
		///   <list type="number">
		///    <item><description>Blank (Actions)</description>
		///    <item><description>User</description></item>
		///    <item><description>Last</description></item>
		///    <item><description>Email</description></item>
		///    <item><description>Comment</description></item>
		///   </list>
		///  </description>
		/// </item>
		/// <item><description>Edit label "[e]"</description></item>
		/// <item><description>Delete label "[d]"</description></item>
		/// <item><description>Role label "[r]"</description></item>
		/// </list>
		/// </para>
		/// </remarks>
		[Test]
		public void Loaded ()
		{
			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				Helper.Login(driver, "Jeff", "test");

				driver.Url = this.FullUrl;
				Assert.AreEqual(FullUrl, driver.Url, "URL changed unexpectedly");

				// Test 1. Check Title
				elem = driver.FindElement(ByTitle);
				Assert.AreEqual("Members", elem.Text, "Title is not correct");

				// Test 2. Table is present
				elem = driver.FindElement(ByMemberList);
				Assert.IsNotNull(elem, "Usergrid not present.");

				// Test 3. Column Headings
				elems = elem.FindElements(By.XPath("//table[@id='ctl00_MainContent_UserGrid']/tbody/tr[1]/th[@scope='col']"));
				Assert.AreEqual(5, elems.Count, "Table column count is incorrect");
				foreach(var colHead in elems){
					Assert.Contains(colHead.Text, titles, "Header appears to be missing");
				}

				// Test 4. Edit label


				// Test 5. Delete label


				// Test 6. Role Label
				elem = driver.FindElement(ByMemberList);
				By[] basechain = new By[]{
					ByMemberList,
					By.TagName("tr")
				};
				elems = elem.FindElements(new ByChained(basechain));
				foreach(var row in elems){
					var chain = new List<By>(basechain);

					chain.Add(By.XPath("./td[2]"));
					var username = row.FindElement(new ByChained(chain.ToArray()));
//					var link = row.FindElement(By.LinkText("[r]"));
//					link.Click();
//					Assert.IsTrue(driver.Url.Contains("UserRoles.aspx"),"Role Link had invalid destincation. Expected 'UserRoles.aspx' actual '"+driver.Url+"'");
//					var URL = new Uri(driver.Url);
//					Assert.AreEqual("u="+username.Text,URL.Query);
				}
			}
		}
	}
}


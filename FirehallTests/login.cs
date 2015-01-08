using System;
using Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using FirehallTests.General;
using NUnit.Framework;

namespace FirehallTests
{
	[TestFixture()]
	public class login: WebPageFixture
	{
		protected By UserName;
		protected By Password;
		protected By SubmitButton;
		protected By LoginStatus;
		protected By RegisterLink;

		protected override string PageUrl{
			get{
				return "/login.aspx";
			}
		}

		[TestFixtureSetUp]
		protected void Setup ()
		{
			UserName = By.Name("ctl00$MainContent$Login1$UserName");
			Password = By.Name("ctl00$MainContent$Login1$Password");
			SubmitButton = By.Name("ctl00$MainContent$Login1$LoginButton");
			LoginStatus = By.XPath("//*[@id='aspnetForm']/header/div[1]/a");
			RegisterLink = By.PartialLinkText("not registered");
		}

		[Test]
		public override void Loaded ()
		{
			throw new System.NotImplementedException();
		}

		[Test]
		public void Login ()
		{
			const string username = "Jeff";
			const string password = "test";
			const string pagedest = "http://127.0.0.1:8080/default.aspx";

			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				IWebElement elem = null;

				driver.Url = this.FullUrl;
				Assert.AreEqual("Firehall System",driver.Title, "Incorrect page (login)");

				elem = driver.FindElement(UserName);
				elem.SendKeys(username);
				elem = driver.FindElement(Password);
				elem.SendKeys(password);

				elem = driver.FindElement(SubmitButton);
				elem.Click();
				Assert.AreEqual(pagedest,driver.Url,"Unexpected target page");

				elem = driver.FindElement(LoginStatus);
				Assert.AreEqual("Logout",elem.Text,"User failed to login");
			}
		}

		[Test]
		public void FailedLogin ()
		{
			const string username = "Jeff";
			const string password = "badpassword";
			const string pagedest = "http://127.0.0.1:8080/login.aspx";

			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				IWebElement elem = null;

				driver.Url = this.FullUrl;
				Assert.AreEqual("Firehall System",driver.Title, "Incorrect page (login)");

				elem = driver.FindElement(UserName);
				elem.SendKeys(username);
				elem = driver.FindElement(Password);
				elem.SendKeys(password);

				elem = driver.FindElement(SubmitButton);
				elem.Click();
				Assert.AreEqual(pagedest,driver.Url,"Unexpected target page");

				elem = driver.FindElement(LoginStatus);
				Assert.AreEqual("Login",elem.Text,"Login status is incorrect");

				Assert.True(driver.PageSource.Contains("Your login attempt was not successful. Please try again"),"Failed Login message not present");
			}
		}

		/// <summary>
		/// Registers the link valid.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The login page should contain a link to the registration 
		/// page. This link is used by people who have ended up on the 
		/// login page, but have never created an account. This will 
		/// allow them to create an account if necessary.
		/// </para>
		/// <para>
		///  <list type="number">
		///   <item>
		///    Goto Login page
		///    VERIFY: "Register" link is present
		///   </item>
		///   <item>
		///    Click on Link
		///    VERIFY: redirected to "registration" page
		///   </item>
		/// </list>
		/// </para>
		/// </remarks>
		[Test]
		public void RegisterLinkValid ()
		{
			using (PooledWebDriver pooled = DriverPool.Checkout()) {
				IWebDriver driver = pooled.driver;
				IWebElement elem = null;

				driver.Url = this.FullUrl;
				Assert.AreEqual("Firehall System",driver.Title, "Incorrect page (login)");

				elem = driver.FindElement(RegisterLink);
				elem.Click();

				Assert.DoesNotThrow(
					()=>{elem = driver.FindElement(By.Id("ctl00_PageHtmlTitle"));},
					"Could not find Registration page title."
				);
				Assert.AreEqual("Register",elem.Text, "Unexpected page title");
			}
		}
	}
}


using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using NUnit.Framework;


namespace FirehallTests.General
{
	public class SeleniumHelper
	{
		#region Singleton
		private static Dictionary<string,SeleniumHelper> instances = new Dictionary<string, SeleniumHelper>();
		internal static SeleniumHelper Get(string baseurl)
		{
			SeleniumHelper instance = null;
			if(instances.ContainsKey(baseurl)){
				instance = instances[baseurl];
			}
			if(instance == null){
				instance = new SeleniumHelper(baseurl);
				instances.Add(baseurl,instance);
			}
			return instance;
		}
		#endregion

		private string baseurl;

		internal SeleniumHelper (string baseurl)
		{
			this.baseurl = baseurl;
		}

		public string LoggedInAs (IWebDriver driver)
		{
			const string marker = "Logged in as:";
			var spos = driver.PageSource.IndexOf(marker) + marker.Length;
			var epos = driver.PageSource.IndexOf(" ",spos);
			var username = driver.PageSource.Substring(spos,epos-spos);
			return username;
		}

		/// <summary>
		/// Determines whether this instance is logged in on the specified driver.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is logged in the specified driver; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='driver'>
		/// If set to <c>true</c> driver.
		/// </param>
		public bool IsLoggedIn (IWebDriver driver)
		{
			try {
				//Console.Error.WriteLine("IsLoggedIn[1]:");
				var LoginStatus = By.XPath("//*[@id='aspnetForm']/header/div[1]/a");
				//Console.Error.WriteLine("IsLoggedIn[2]:");
				var elem = driver.FindElement(LoginStatus);
				//Console.Error.WriteLine("IsLoggedIn[3]:" + elem.Text);
				return ("Logout" == elem.Text);
			} catch {
				//Console.Error.WriteLine("IsLoggedIn[4]: not logged in");
				return false;
			}
		}

		/// <summary>
		/// Logout of vius on the specified driver.
		/// </summary>
		/// <param name='driver'>
		/// Driver.
		/// </param>
		public void Logout (IWebDriver driver)
		{
			if (!IsLoggedIn(driver)) {
				return;
			}

			var LoginStatus = By.XPath("//*[@id='aspnetForm']/header/div[1]/a");
			var elem = driver.FindElement(LoginStatus);
			Assert.AreEqual ("Logout",elem.Text,"Could not find logout button.");

			var page = driver.Url;
			elem.Click();
			Assert.False(IsLoggedIn(driver),"Attempted to logout, but still logged in.");
			driver.Url = page;
		}

		/// <summary>
		/// Login the specified driver, username and password.
		/// </summary>
		/// <param name='driver'>
		/// Driver.
		/// </param>
		/// <param name='username'>
		/// Username.
		/// </param>
		/// <param name='password'>
		/// Password.
		/// </param>
		public void Login (IWebDriver driver, string username, string password)
		{
			if (IsLoggedIn(driver)) {
				return;
			}

			//Console.Error.WriteLine("Login[1]:");
			By UserName = By.Name("ctl00$MainContent$Login1$UserName");
			By Password = By.Name("ctl00$MainContent$Login1$Password");
			By SubmitButton = By.Name("ctl00$MainContent$Login1$LoginButton");
			//Console.Error.WriteLine("Login[2]:");

			string pagedest = baseurl + "/default.aspx";
			string page = driver.Url;
			//Console.Error.WriteLine("Login[3]:");

			IWebElement elem = null;

			// go to the login page
			driver.Url = baseurl + "/login.aspx";
			//Console.Error.WriteLine("Login[4]:" + driver.Url);

			// fill in the login form
			elem = driver.FindElement(UserName);
			elem.SendKeys(username);
			//Console.Error.WriteLine("Login[5]:");
			elem = driver.FindElement(Password);
			elem.SendKeys(password);
			//Console.Error.WriteLine("Login[6]:");

			// submit the form
			elem = driver.FindElement(SubmitButton);
			elem.Click();
			//Console.Error.WriteLine("Login[7]:" + driver.Url);
			Assert.AreEqual(pagedest,driver.Url,"Unexpected target page");

			// check to see if you are logged in
			var isloggedin = IsLoggedIn(driver);
			//Console.Error.WriteLine("Login[8]:" + isloggedin);
			Assert.IsTrue(isloggedin,"Failed to log in.");

			// put it back to the page we started on
			driver.Url = page;
			//Console.Error.WriteLine("Login[9]:"+driver.Url);
		}

	}
}


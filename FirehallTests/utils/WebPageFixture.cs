using System;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;

namespace FirehallTests.General
{
	public abstract class WebPageFixture
	{
		//a web element object for generic use
		protected IWebElement elem = null;
		protected ReadOnlyCollection<IWebElement> elems = null;

		public virtual string BaseUrl {
			get {
				return "http://127.0.0.1:8080";
			}
		}

		protected abstract string PageUrl {
			get;
		}

		private string fullurl = null;
		protected virtual string FullUrl {
			get {
				if(fullurl == null){
					fullurl = BaseUrl + PageUrl;
				}
				return fullurl;
			}
		}

		protected WebDriverPool DriverPool{
			get{
				return WebDriverPool.Instance;
			}
		}

		public SeleniumHelper Helper {
			get {
				return SeleniumHelper.Get(BaseUrl);
			}
		}

		[Test]
		public void Test ()
		{
		}

		[Test]
		public void TestHttp200 ()
		{
			Assert.IsNotNullOrEmpty (FullUrl, "Page Test Failure.");

			WebRequest request = (WebRequest)WebRequest.Create (FullUrl);
			request.Method = "GET";
			request.ContentType = "application/x-www-form-urlencoded";
			using (var response = (HttpWebResponse)request.GetResponse()) {
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
			}
		}

		[Test]
		public abstract void Loaded();
	}
}


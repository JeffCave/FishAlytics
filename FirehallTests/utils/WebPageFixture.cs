using System;
using NUnit.Framework;
using System.Net;
using System.IO;

namespace FirehallTests
{
	public abstract class WebPageFixture
	{
		protected virtual string BaseUrl {
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
	}
}


using System;
using NUnit.Framework;
using System.Net;
using System.IO;

namespace FirehallTests
{
	public abstract class WebPageFixture
	{
		protected abstract string PageUrl {
			get;
		}

		[Test]
		public void Test ()
		{
		}

		[Test]
		public void TestHttp200 ()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (PageUrl);
			request.Method = "GET";
			request.ServicePoint.Expect100Continue = false;
			request.ContentType = "application/x-www-form-urlencoded";

			using (var response = (HttpWebResponse)request.GetResponse()) {
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}


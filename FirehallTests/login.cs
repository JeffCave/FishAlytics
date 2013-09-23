using System;
using NUnit.Framework;

namespace FirehallTests
{
	[TestFixture()]
	public class login: WebPageFixture
	{
		protected override string PageUrl{
			get{
				return "/login.aspx";
			}
		}

		[Test]
		public void Login ()
		{

		}
	}
}


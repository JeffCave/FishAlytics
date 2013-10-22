using System;
using NUnit.Framework;

namespace FirehallTests.Fishing
{
	[TestFixture()]
	public class Catch:FirehallTests.General.WebPageFixture
	{
		protected override string PageUrl {
			get {
				return "/Fishing/Catch.aspx";
			}
		}

		[Test]
		public override void Loaded ()
		{
			throw new System.NotImplementedException();
		}
	}
}


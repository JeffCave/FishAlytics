using System;
using NUnit.Framework;

namespace FirehallTests.Fishing
{
	[TestFixture()]
	public class Catches:FirehallTests.General.WebPageFixture
	{
		protected override string PageUrl {
			get {
				throw new System.NotImplementedException();
			}
		}

		public override void Loaded ()
		{
			throw new System.NotImplementedException();
		}
	}
}


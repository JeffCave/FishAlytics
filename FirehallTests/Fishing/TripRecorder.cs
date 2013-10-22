using System;
using NUnit.Framework;

namespace FirehallTests.Fishing
{
	[TestFixture()]
	public class TripRecorder:FirehallTests.General.WebPageFixture
	{
		protected override string PageUrl {
			get {
				throw new System.NotImplementedException();
			}
		}

		[Test]
		public override void Loaded ()
		{
			throw new System.NotImplementedException();
		}

	}
}


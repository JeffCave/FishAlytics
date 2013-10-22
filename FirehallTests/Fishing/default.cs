using System;
using NUnit.Framework;
namespace FirehallTests.Fishing
{
	[TestFixture()]
	public class Default:FirehallTests.General.WebPageFixture
	{
		protected override string PageUrl {
			get {
				return "/Fishing/default.aspx";
			}
		}

		public override void Loaded ()
		{
			throw new System.NotImplementedException();
		}
	}
}


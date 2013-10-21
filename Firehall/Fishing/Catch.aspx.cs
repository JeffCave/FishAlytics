
using System;
using System.Web;
using System.Web.UI;

namespace Firehall.Fishing
{
	public partial class Catch : Firehall.Page
	{
		protected Vius.Fishing.Data.Catch datum;

		protected void Page_Load (object sender, EventArgs e)
		{ 
			if(!IsPostBack){
				FillForm();
			}
		}

		protected void FillForm()
		{
			var catchid = int.Parse(Request["c"]);
			datum = Globals.Fishing.Catches[catchid];
		}

	}
}


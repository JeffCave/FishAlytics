
using System;
using System.Web;
using System.Web.UI;
using System.Web.Services;

using Vius.Fishing;

namespace Firehall.Fishing
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public partial class Catch : Firehall.Page
	{
		protected Vius.Fishing.Data.Catch datum;

		/// <summary>
		/// Page_s the load.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void Page_Load (object sender, EventArgs e)
		{ 
			var action = Request["action"];
			var id = 0;
			try{
				id = int.Parse(Request["id"]);
			} 
			catch{
				return;
			}

			switch (action.ToLower()) {
				case "s":
					break;
				case "d":
					Delete(id);
					break;
				default:
					FillForm(id);
					break;
			}
		}

		protected void ReadPostback(){
			datum = new Vius.Fishing.Data.Catch();
			datum.Species = Request["Species"];
			datum.Time = DateTime.Parse(Request["Time"]);
		}

		/// <summary>
		/// Fills the form.
		/// </summary>
		protected void FillForm(int catchid)
		{
			datum = Globals.Fishing.Catches[catchid];
		}

		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[WebMethod]
		public void Delete(int id)
		{
		}

	}
}


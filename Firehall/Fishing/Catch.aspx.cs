using System;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using System.Data.Linq;
using System.Linq;

using Vius.Web;
using Vius.Fishing;
using Data = Vius.Fishing.Data;

namespace Firehall.Fishing
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// HEAD 	Same as GET but returns only HTTP headers and no document body
	/// PUT 	Uploads a representation of the specified URI
	/// DELETE 	Deletes the specified resource
	/// OPTIONS
	/// 
	/// $Id$
	/// $URL$
	/// </remarks>
	public partial class Catch : Firehall.Page
	{
		protected Data.Catch datum = new Data.Catch();

		public string Time {
			get{
				try{
					return datum.Time.ToString();
				}
				catch{
					return "";
				}

			}
		}

		/// <summary>
		/// Page_s the load.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void Page_Load (object sender, EventArgs e)
		{ 
			int id = 0;
			var url = Request.Url.Query.Replace("?", "");
			int.TryParse(url, out id);

			switch (Request.HttpMethod.ToUpperInvariant()) {
				case "POST":
					using (var dat = Vius.Fishing.Data.FishingData.GetContext()) {
//						datum = (Data.Catch)ParamSerializer.Deserialize<Data.Catch>(Request.Form);
//						dat.Log = System.Console.Out;
//						dat.Catches.InsertOnSubmit(datum);
//						try {
//							dat.SubmitChanges();
//						}
//						catch (Exception ex) {
//							System.Console.Out.WriteLine(ex.InnerException.Message);
//						}
						var qry = dat.ExecuteQuery(
							"insert into \"Fishing\".\"Catches\"(\"Time\") " +
							"values ('"+DateTime.Now.ToString()+"') " +
							"return CatchId "
							,
							null
						);
						id = qry[0];
					}
					break;
				case "DELETE":
					Delete(id);
					Response.Redirect("Catches.aspx");
					break;
				default:
					break;
			}
			FillForm(id);
		}

		/// <summary>
		/// Reads the form.
		/// </summary>
		protected void ReadForm(){
		}

		/// <summary>
		/// Fills the form.
		/// </summary>
		protected void FillForm(int catchid)
		{

			var lookup = (
					from c in Globals.Fishing.Catches
					where c.CatchId == catchid
					select c
				).FirstOrDefault();
			datum = lookup ?? datum;
		}

		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		[WebMethod(true)]
		public void Delete(int id)
		{
			Globals.Fishing.Catches.DeleteOnSubmit(Globals.Fishing.Catches.First(c => c.CatchId == id));
		}

	}
}


using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data.Linq;
using System.Linq;
using System.Data;

namespace Firehall
{
	public partial class Trips : Firehall.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			BindEventHandlers ();
			if (!Page.IsPostBack) {
				BindDataGrid ();
			}
		}

		protected void BindEventHandlers ()
		{
			TripGrid.RowDataBound += HandleRowDataBound;

		}

		public void HandleRowDataBound (object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
		{
			HyperLink link;
			Label label;
			string DATE = "yyyy-MM-dd";
			string TIME = "HH:mm";

			var row = e.Row;
			if (row.RowType == DataControlRowType.DataRow) {
				var trip = e.Row.DataItem as Vius.Fishing.Data.Trip;

				//view button
				link = row.FindControl("ViewButton") as HyperLink;
				if(link != null){
					link.NavigateUrl = String.Format(
						"Trip.aspx?t={0}",
						Server.UrlEncode(trip.Id.ToString())
					);
				}

				//start time
				label = row.FindControl("StartTime") as Label;
				if(label != null){
					label.Text = trip.Start.ToString(DATE + " " + TIME);
				}

				//finish time
				label = row.FindControl("FinishTime") as Label;
				if(label != null){
					var fmt = TIME;
					if(trip.Finish.Date != trip.Start.Date){
						fmt = DATE + " " + TIME;
					}
					if(trip.Finish != DateTime.MaxValue){
						label.Text = trip.Finish.ToString(fmt);
					} else {
						label.Text = "Active";
					}
				}

				//Duration
				label = row.FindControl("Duration") as Label;
				if(label != null){
					if(trip.Finish != DateTime.MaxValue){
						label.Text = trip.Duration.ToString();
					}
				}

				//Fisherman
				label = row.FindControl("Fisherman") as Label;
				if(label != null){
					label.Text = trip.Fisherman.UserName;
				}
			}
		}

		private void BindDataGrid ()
		{
			if (TripGrid.DataSource == null) {
				TripGrid.DataSource = Globals.Fishing.Trips.Where();
			}
			TripGrid.DataBind ();
		}

	}
}


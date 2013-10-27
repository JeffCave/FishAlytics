using System;
using System.Web;
using System.Web.UI;

namespace Firehall
{
	/// <summary>
	/// Primary entry location for a user to enter information about a fishing excursion.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The idea behind the trip object is for it to act as a collecting
	/// point for all the little bits of data. It gives the user a single
	/// location to start thinking about the problem from.
	/// </para>
	/// <para>
	/// If this is to be the primary interface for the user, then it only
	/// makes sense that they should be using a map, since all of our data
	/// is really geo-tagged. We really care where all of this action is
	/// occuring.
	/// </para>
	/// <para>
	/// http://docs.openlayers.org/library/introduction.html
	/// </para>
	/// <para>
	/// This page should contain summaries of catches, area travelled, and environmental readings.
	/// </para>
	/// <para>
	/// $Id$
	/// $URL$
	/// </para>
	/// </remarks>
	public partial class Trip : Firehall.Page
	{
		private Vius.Fishing.Data.Trip _TripData = null;
		public Vius.Fishing.Data.Trip TripData {
			get {
				if(_TripData == null){
					try{
						_TripData = Globals.Fishing.Trips[(int)ViewState["TripData"]];
					} catch {
						_TripData = Globals.Fishing.Trips.New();
						ViewState["TripData"] = null;
					}

				}
				return(_TripData);
			}
		}

		public new static Vius.Web.PageSiteMapNode CreateSiteMapNode()
		{
			var node = Firehall.Page.CreateSiteMapNode();
			node.Capabilities.Add("Fishing.Trip.Add");
			node.Capabilities.Add("Fishing.Trip.Edit");
			node.Capabilities.Add("Fishing.Trip.Own.Add");
			node.Capabilities.Add("Fishing.Trip.Own.Edit");

			return node;
		}


		public Trip(){
			PageTitle = "Fishing Trip";
		}


		void SaveTrip (object sender, EventArgs e)
		{
			try {
				UpdateData();
				TripData.Save();
				ViewState["TripData"] = TripData.Id;
			} catch (Exception ex) {
				System.Collections.Generic.Queue<string> msgs;
				if(Messages.DataSource == null){
					Messages.DataSource = new System.Collections.Generic.Queue<string>();
				}
				msgs = Messages.DataSource as System.Collections.Generic.Queue<string>;
				msgs.Enqueue(Server.HtmlEncode(ex.Message));
				Messages.DataBind();
			}
		}

		public void Page_Load ()
		{
			BindEvents();
			if (!IsPostBack) {
			}
		}

		private void BindEvents ()
		{
			btnSave.Click += SaveTrip;
			txtTripStart.TextChanged += UpdateData;
			txtTripDate.TextChanged += UpdateData;
			txtTripEnd.TextChanged += UpdateData;
		}

		private bool UpdateDataAlready = false;
		public void UpdateData (object sender, EventArgs e)
		{
			UpdateData();
		}

		public void UpdateData (bool force = true)
		{
			//this may get called multiple times, but really only 
			//needs to be called once per page load
			if (UpdateDataAlready && !force) {
				return;
			}
			UpdateDataAlready = true;

			DateTime date;
			DateTime time;

			//startdate
			try {
				date = DateTime.Parse(this.txtTripDate.Text);
				try {
					time = DateTime.Parse(this.txtTripStart.Text);
				} catch {
					time = DateTime.MinValue;
				}
				date = date
					.Subtract(date.TimeOfDay)
					.Add(time.TimeOfDay);
			} catch {
				date = DateTime.MinValue;
			}
			TripData.TripStart = date;

			//end date
			try {
				date = DateTime.Parse(this.txtTripEnd.Text);
			} catch {
				date = DateTime.MaxValue;
			}
			TripData.TripEnd = date;
		}


	}
}


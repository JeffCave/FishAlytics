using System;
using System.Web;
using System.Web.UI;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Data.Linq;
using System.Linq;

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
		/// <summary>
		/// Gets the trip data.
		/// </summary>
		/// <value>
		/// The trip data.
		/// </value>
		public Vius.Fishing.Data.Trip TripData {
			get {
				if(_TripData == null){
					try{
						int tripid = 0;
						try{
							tripid = (int)ViewState["TripData"];
						} catch {
							tripid = Convert.ToInt32(Request["t"]);
						}
						_TripData = (from t in Globals.Fishing.Trips where t.Id == tripid select t).FirstOrDefault();
					} catch {
						_TripData = new Vius.Fishing.Data.Trip();
						Globals.Fishing.Trips.InsertOnSubmit(_TripData);
						ViewState["TripData"] = null;
					}

				}
				return(_TripData);
			}
		}

		/// <summary>
		/// Creates the site map node.
		/// </summary>
		/// <returns>
		/// The site map node.
		/// </returns>
		public new static Vius.Web.PageSiteMapNode CreateSiteMapNode()
		{
			var node = Firehall.Page.CreateSiteMapNode();
			node.Capabilities.Add("Fishing.Trip.Add");
			node.Capabilities.Add("Fishing.Trip.Edit");
			node.Capabilities.Add("Fishing.Trip.Own.Add");
			node.Capabilities.Add("Fishing.Trip.Own.Edit");

			return node;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Firehall.Trip"/> class.
		/// </summary>
		public Trip(){
			PageTitle = "Fishing Trip";
		}

		/// <summary>
		/// Saves the trip.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		public void SaveTrip (object sender, EventArgs e)
		{
			try {
				UpdateData();
				Globals.Fishing.Trips.InsertOnSubmit(TripData);
				Globals.Fishing.SubmitChanges();
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

		/// <summary>
		/// Page_s the load.
		/// </summary>
		public void Page_Load ()
		{
			if (!IsPostBack) {
				FillForm();
			}
			BindEvents();
			BindClientSide();
		}

		/// <summary>
		/// Binds the events.
		/// </summary>
		private void BindEvents ()
		{
			btnSave.Click += SaveTrip;
			txtTripStart.TextChanged += UpdateData;
			txtTripDate.TextChanged += UpdateData;
			txtTripEnd.TextChanged += UpdateData;
		}

		/// <summary>
		/// The update data already.
		/// </summary>
		private bool FillFormAlready = false;

		/// <summary>
		/// Fills the form.
		/// </summary>
		public void FillForm (bool force = false)
		{
			if (FillFormAlready && !force) {
				return;
			}

			// Start Date
			txtTripDate.Text = "";
			txtTripStart.Text = "";
			if (TripData.Start != DateTime.MinValue) {
				txtTripDate.Text = TripData.Start.Date.ToString("yyyy-MM-dd");
				txtTripStart.Text = TripData.Start.TimeOfDay.ToString();
			}

			// End Date
			txtTripEnd.Text = "";
			if (TripData.Finish != DateTime.MaxValue) {
				txtTripEnd.Text = TripData.Finish.ToString();
			}
		}

		/// <summary>
		/// The update data already.
		/// </summary>
		private bool UpdateDataAlready = false;

		/// <summary>
		/// Updates the data.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		public void UpdateData (object sender, EventArgs e)
		{
			UpdateData();
		}

		/// <summary>
		/// Updates the data.
		/// </summary>
		/// <param name='force'>
		/// Force.
		/// </param>
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
			TripData.Start = date;

			//end date
			try {
				date = DateTime.Parse(this.txtTripEnd.Text);
			} catch {
				date = DateTime.MaxValue;
			}
			TripData.Finish = date;
		}

		private void BindClientSide(){
			ClientScript.RegisterClientScriptInclude("local","Trip.js");

			string js = 
					"<script language='javascript'>\n" +
					"  var txtStartDate = " + txtTripDate.ClientID + "; \n" + 
					"  var txtStartTime = " + txtTripStart.ClientID + "; \n" + 
					"  var txtEnd = " + txtTripEnd.ClientID + "; \n" + 
					"  var txtDuration = " + TripDuration.ClientID + "; \n" + 
					"  var tripdata = " + TripData.Serialize() + "; \n" + 
					"  RunPage(); \n" +
					"</script>\n"
					;
			ClientScript.RegisterStartupScript(GetType(),"startup",js);

		}

	}
}


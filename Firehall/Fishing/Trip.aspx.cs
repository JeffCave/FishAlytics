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
		public int TripId
		{
			get
			{
				if(null == ViewState["TripId"]){
					int tripid = 0;
					try{
						tripid = int.Parse(Request["t"]);
					} catch {
						tripid = 0;
					}
					ViewState["TripId"] = tripid;
				}
				return((int)ViewState["TripId"]);
			}
		}

		private Vius.Fishing.Data.Trip tripData = null;
		public Vius.Fishing.Data.Trip TripData {
			get {
				if(tripData == null){
					tripData = Globals.Fishing.Trips[TripId];
				}
				return tripData;
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

		public void Page_Load ()
		{
			if (!IsPostBack) {
			}
		}

	}
}


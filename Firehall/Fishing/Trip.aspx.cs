using System;
using System.Web;
using System.Web.UI;

namespace Firehall
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
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


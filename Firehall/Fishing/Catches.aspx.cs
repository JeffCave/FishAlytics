using System;
using System.Web;
using System.Web.UI;
using System.Linq;

namespace Firehall.Fishing
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public partial class Catches : Firehall.Page
	{
		protected void Page_Load()
		{
			if (!IsPostBack) {
				BindDataGrid();
			}
		}

		private void BindDataGrid()
		{
			if(CatchesGrid.DataSource == null){
				Globals.Fishing.Log = Console.Out;
				var list = 
					from c in Globals.Fishing.Catches
					select c;
				CatchesGrid.DataSource = list.ToList();
			}
			CatchesGrid.DataBind();
		}
	}
}


using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Linq;

namespace Firehall{

	public partial class Login : Firehall.Page
	{
		protected Dictionary<string,string> map = new Dictionary<string,string>{
			{"google","~/LoginGoogle.ashx"}
		};

		protected void Page_PreInit(object sender, EventArgs e)
		{
			if (Request["authority"] != null) {
				var auth = (string)Request["authority"];
				if (map.ContainsKey(auth)) {
					Response.Redirect(map[auth]);
				}

			}
		}

		protected void Page_Load (object sender, EventArgs e)
		{
		}

	}
}


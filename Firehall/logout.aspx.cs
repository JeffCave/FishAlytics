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

namespace Firehall{
	public partial class Logout : Firehall.Page
	{
	    protected void Page_Load(object sender, EventArgs e)
	    {
			//http://stackoverflow.com/questions/412300/formsauthentication-signout-does-not-log-the-user-out
			Firehall.Web.Auth.SignOut();
	    }
	}
}

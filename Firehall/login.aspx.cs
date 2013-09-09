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

namespace Firehall{

	public partial class Login : System.Web.UI.Page
	{

		protected void Page_Load (object sender, EventArgs e)
		{
			Login1.Authenticate += this.HandleAuthenticate;
		}

		void HandleAuthenticate (object sender, AuthenticateEventArgs e)
		{
			// if they already authenticated... so be it
			if (e.Authenticated) {
				return;
			}

			// Three valid username/password pairs: Scott/password, Jisun/password, and Sam/password.
			var users = new Dictionary<string,string>();
			users.Add ("Scott", "password");
			users.Add ("Jisun", "password");
			users.Add ("Sam", "password");
			foreach (var user in users) {
				if (Login1.UserName == user.Key && Login1.Password == user.Value) {
					// Log in the user...
					e.Authenticated = true;
//					FormsAuthentication.RedirectFromLoginPage (Login1.UserName, Login1.RememberMeSet);
					break;
				}
			}

			// If we reach here, the user's credentials were invalid
			InvalidCredentialsMessage.Visible = !e.Authenticated;

		}

	}
}


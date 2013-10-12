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

	public partial class Login : System.Web.UI.Page
	{

		protected void Page_Load (object sender, EventArgs e)
		{
			Login1.Authenticate += this.HandleAuthenticate;
			Login1.LoginError += HandleLoginError;

			if (!IsPostBack) {
				Login1.CreateUserUrl = "~/Administration/Membership/CreatingUserAccounts.aspx";
			}
		}

		protected void HandleAuthenticate (object sender, AuthenticateEventArgs e)
		{
			// if they already authenticated... so be it
			if (e.Authenticated) {
				return;
			}

			var login = (System.Web.UI.WebControls.Login)sender;
			e.Authenticated = Membership.ValidateUser(login.UserName,login.Password);
		}

		protected void HandleLoginError (object sender, EventArgs e)
		{
			var login = (System.Web.UI.WebControls.Login)sender;

			// Default fail text
			login.FailureText = "Your login attempt was not successful. Please try again.";

			// Does there exist a User account for this user?
			MembershipUser usrInfo = Membership.GetUser (login.UserName);
			if (usrInfo != null) {
				// Is this user locked out?
				if (usrInfo.IsLockedOut) {
					login.FailureText = "Your account has been locked out because of too many invalid login attempts. Please contact the administrator to have your account unlocked.";
				} else if (!usrInfo.IsApproved) {
					login.FailureText = "Your account has not yet been approved. You cannot login until an administrator has approved your account.";
				}
			}
			
		}

	}
}


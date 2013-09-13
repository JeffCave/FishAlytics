
using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Firehall
{
	public partial class CreatingUserAccounts : System.Web.UI.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			RegisterUser.CreatingUser += ValidateData;
		}

		protected void ValidateData (object sender, System.Web.UI.WebControls.LoginCancelEventArgs e)
		{
			var reguser = (CreateUserWizard)sender;
			reguser.UserName = reguser.UserName.Trim();

			//cancel if the password contains the username
			e.Cancel =  reguser.Password.ToLower().Contains(reguser.UserName.ToLower());
		}
	}
}


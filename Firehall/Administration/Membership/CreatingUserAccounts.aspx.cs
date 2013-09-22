
using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Firehall
{
	public partial class CreatingUserAccounts : System.Web.UI.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			RegisterUser.CreatingUser += ValidateData;
			RegisterUser.ActiveStepChanged += HandleActiveStepChanged;
			if (!Page.IsPostBack) { 
				// Bind the set of roles to RoleList 
				RoleList.DataSource = Roles.GetAllRoles (); 
				RoleList.DataBind (); 
			} 
		}

		void HandleActiveStepChanged (object sender, EventArgs e)
		{
			if (RegisterUser.ActiveStep.Title == "Complete") {
				var roles = new List<string> ();
				// Add the checked roles to the just-added user 
				foreach (ListItem li in RoleList.Items) { 
					if (li.Selected) { 
						roles.Add (li.Text);
					}
				} 
				Roles.AddUserToRoles (RegisterUser.UserName, roles.ToArray ()); 
			}
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


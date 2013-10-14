using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Linq;

namespace Firehall.Administration.Members
{
	using Security = System.Web.Security;

	/// <summary>
	/// User roles.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public partial class UserRoles : Firehall.Page
	{
		private string userName;

		protected void Page_Load (object sender, EventArgs e)
		{ 
			UserList.SelectedIndexChanged += HandleUserChanged;
			userName = UserList.SelectedValue; 
			if (!Page.IsPostBack) { 
				// Bind the users and roles 
				BindUsersToUserList (); 
				BindRolesToList (); 
				CheckRolesForSelectedUser();
			} 
		}

		protected void HandleUserChanged (object sender, EventArgs e)
		{
			CheckRolesForSelectedUser();
		}

		protected void HandleRoleChanged (object sender, EventArgs e)
		{
			var chkbox = sender as CheckBox;
			try{
				if (chkbox.Checked) {
					System.Web.Security.Roles.AddUserToRole(userName, chkbox.Text);
				} else {
					System.Web.Security.Roles.RemoveUserFromRole(userName, chkbox.Text);
				}
			} catch (System.Configuration.Provider.ProviderException ex){
				//This is likely just the system complaining that the user was already
				//added or removed. This is a stupid message because if that is the case,
				//then job done?

				//silence IDE warnings
				var msg = ex.Message;
				msg = "";
				System.Console.Write(msg);
			}
		}

		private void BindUsersToUserList ()
		{ 
			// Get all of the user accounts 
			MembershipUserCollection users = Membership.GetAllUsers(); 
			UserList.DataSource = users; 
			UserList.DataBind();

			var username = Request["u"];
			UserList.SelectedValue = username;
		}
 
		private void BindRolesToList ()
		{
			// Get all of the roles
			string[] roles = Security.Roles.GetAllRoles(); 
			UsersRoleList.DataSource = roles; 
			UsersRoleList.DataBind();
		}

		private void CheckRolesForSelectedUser ()
		{ 
			// Determine what roles the selected user belongs to 
			string selectedUserName = UserList.SelectedValue; 
			string[] selectedUsersRoles = Security.Roles.GetRolesForUser(selectedUserName); 

			// Loop through the Repeater's Items and check or uncheck the checkbox as needed 
			foreach (RepeaterItem ri in UsersRoleList.Items) { 
				// Programmatically reference the CheckBox 
				CheckBox RoleCheckBox = ri.FindControl("RoleCheckBox") as CheckBox; 
				// See if RoleCheckBox.Text is in selectedUsersRoles 
				RoleCheckBox.Checked = (selectedUsersRoles.Contains(RoleCheckBox.Text));
			} 
		}

	}
}


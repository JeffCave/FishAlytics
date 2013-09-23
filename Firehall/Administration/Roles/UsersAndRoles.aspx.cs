
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Linq;

namespace Firehall
{
	public partial class UsersAndRoles : Firehall.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{ 
			UserList.SelectedIndexChanged += HandleUserChanged;
			RoleList.SelectedIndexChanged += HandleSelectedRoleIndexChanged;
			RolesUserList.RowDeleting += HandleRowDeleting;
			AddUserToRoleButton.Command += HandleAddUser;
			if (!Page.IsPostBack) { 
				// Bind the users and roles 
				BindUsersToUserList (); 
				BindRolesToList (); 
				CheckRolesForSelectedUser();
				DisplayUsersBelongingToRole();
			} 
		}

		void HandleAddUser (object sender, CommandEventArgs e)
		{
			string rolename = RoleList.SelectedValue; 
			string username = UserNameToAddToRole.Text; 

			username = username.Trim ();

			// Make sure that a value was entered 
			if (string.IsNullOrEmpty (username)) { 
				ActionStatus.Text = "You must enter a username in the textbox."; 
				return; 
			} 

			// Make sure that the user exists in the system 
			MembershipUser user = Membership.GetUser (username); 
			if (user == null) { 
				ActionStatus.Text = string.Format ("The user {0} does not exist in the system.", username); 
				return; 
			} 

			// Make sure that the user doesn't already belong to this role 
			if (Roles.IsUserInRole (username, rolename)) { 
				ActionStatus.Text = string.Format ("User {0} already is a member of role {1}.", username, rolename); 
				return; 
			} 

			// If we reach here, we need to add the user to the role 
			Roles.AddUserToRole (username, rolename); 

			// Clear out the TextBox 
			UserNameToAddToRole.Text = string.Empty; 

			// Refresh the GridView 
			DisplayUsersBelongingToRole (); 
			if (username == UserList.SelectedValue) {
				CheckRolesForSelectedUser();
			}

			// Display a status message 

			ActionStatus.Text = string.Format ("User {0} was added to role {1}.", username, rolename); 
		}

		void HandleRowDeleting (object sender, GridViewDeleteEventArgs e)
		{
			// Get the selected role 
			string selectedRoleName = RoleList.SelectedValue;
			GridView grid = (GridView)sender;

			// Remove the user from the role 
			var user = ((Label)grid.Rows [e.RowIndex].FindControl ("UserNameLabel")).Text;
			Roles.RemoveUserFromRole (user, RoleList.SelectedValue);

			// Refresh the GridView 
			DisplayUsersBelongingToRole (); 
			if (user == UserList.SelectedValue) {
				CheckRolesForSelectedUser ();
			}

			// Display a status message 
			ActionStatus.Text = string.Format ("Users {0} was removed from role {1}.", e.Values, selectedRoleName); 
		}


		void HandleSelectedRoleIndexChanged (object sender, EventArgs e)
		{
			DisplayUsersBelongingToRole();
		}

		protected void HandleUserChanged (object sender, EventArgs e)
		{
			CheckRolesForSelectedUser();
		}

		protected void HandleRoleChanged (object sender, EventArgs e)
		{
			var checkbox = (CheckBox)sender;
			var role = checkbox.Text;

			var user = UserList.Text;

			if (checkbox.Checked) {
				Roles.AddUserToRole (user, role);
				ActionStatus.Text = string.Format ("User {0} was added to role {1}.", user, role); 
			} else {
				Roles.RemoveUserFromRole (user, role);
				ActionStatus.Text = string.Format ("User {0} was removed role {1}.", user, role); 
			}

			if (RoleList.Text == role) {
				DisplayUsersBelongingToRole();
			}
		}

		private void BindUsersToUserList ()
		{ 
			// Get all of the user accounts 
			MembershipUserCollection users = Membership.GetAllUsers (); 
			UserList.DataSource = users; 
			UserList.DataBind (); 
		}
 
		private void BindRolesToList ()
		{ 
			// Get all of the roles 
			string[] roles = Roles.GetAllRoles (); 
			UsersRoleList.DataSource = roles; 
			UsersRoleList.DataBind ();
			RoleList.DataSource = roles;
			RoleList.DataBind ();
		}

		private void CheckRolesForSelectedUser ()
		{ 
			// Determine what roles the selected user belongs to 
			string selectedUserName = UserList.SelectedValue; 
			string[] selectedUsersRoles = Roles.GetRolesForUser (selectedUserName); 

			// Loop through the Repeater's Items and check or uncheck the checkbox as needed 
			foreach (RepeaterItem ri in UsersRoleList.Items) { 
				// Programmatically reference the CheckBox 
				CheckBox RoleCheckBox = ri.FindControl ("RoleCheckBox") as CheckBox; 
				// See if RoleCheckBox.Text is in selectedUsersRoles 
				RoleCheckBox.Checked = (selectedUsersRoles.Contains(RoleCheckBox.Text)) ;
			} 
		}

		private void DisplayUsersBelongingToRole ()
		{ 
			// Bind the list of users to the GridView 
			RolesUserList.DataSource = Roles.GetUsersInRole (RoleList.SelectedValue);
			RolesUserList.DataBind (); 
		}
	}
}


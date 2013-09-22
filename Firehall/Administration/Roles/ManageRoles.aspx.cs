
using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Firehall
{
	public partial class ManageRoles : System.Web.UI.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			CreateRoleButton.Command += HandleCommand;
			RoleList.RowDeleting += HandleRowDeleting;

			if (!IsPostBack) {
				DisplayRolesInGrid ();
			}
		}

		/// <summary>
		/// Handles the row deleting.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		void HandleRowDeleting (object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
		{
			// Get the RoleNameLabel
			Label RoleNameLabel = RoleList.Rows [e.RowIndex].FindControl ("RoleNameLabel") as Label;

			// Delete the role
			Roles.DeleteRole (RoleNameLabel.Text,false);

			// Rebind the data to the RoleList grid
			DisplayRolesInGrid ();
		}

		/// <summary>
		/// Handles the command.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e' />
		void HandleCommand (object sender, System.Web.UI.WebControls.CommandEventArgs e)
		{

			string newRoleName = RoleName.Text.Trim ();

			try {
				Roles.CreateRole (newRoleName);

				RoleName.Text = string.Empty;
				MessageBox.Text = "Created role '" + newRoleName + "'";
				DisplayRolesInGrid();
			} catch (Exception ex) {
				this.MessageBox.Text = "Failed to create Role: " + ex.Message;
			}

		}

		/// <summary>
		/// Display the roles in grid.
		/// </summary>
		private void DisplayRolesInGrid ()
		{
			RoleList.DataSource = Roles.GetAllRoles ();
			RoleList.DataBind ();
		}

	}



}


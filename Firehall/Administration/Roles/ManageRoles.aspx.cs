
using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Firehall
{
	public partial class ManageRoles : Firehall.Page
	{
		protected void Page_Load (object sender, EventArgs e)
		{
			CreateRoleButton.Command += HandleCommand;
			RoleList.RowDeleting += HandleRowDeleting;
			RoleList.RowDataBound += HandleRowDataBound;

			if (!IsPostBack) {
				DisplayRolesInGrid ();
			}
		}

		void HandleRowDataBound (object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow) {
				HyperLink link = (HyperLink)e.Row.FindControl("RoleButton");
				link.NavigateUrl = String.Format(
					"RoleCapabilities.aspx?r={0}",
					Server.UrlEncode(e.Row.DataItem.ToString())
				);
				Label label = (Label)e.Row.FindControl("RoleNameLabel");
				label.Text = Server.HtmlEncode(e.Row.DataItem.ToString());
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


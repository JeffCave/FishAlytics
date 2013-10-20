using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;
using Auth = Vius.Authentication;

namespace Firehall.Admnistration.Membership
{
	public partial class MemberList : Firehall.Page
	{

		protected void Page_Load (object sender, EventArgs e)
		{
			BindEventHandlers ();
			if (!Page.IsPostBack) {
				BindUserGrid ();
			}
		}

		protected void BindEventHandlers ()
		{
			UserGrid.RowDeleting += HandleUserDeleting;
			UserGrid.RowEditing += HandleuserEditing;
			UserGrid.RowUpdating += HandleUserUpdating;
			UserGrid.RowCancelingEdit += HandleUserCancelingEdit;
			UserGrid.RowDataBound += HandleRowDataBound;
			UserGrid.RowCreated += HandleRowCreated;
		}

		protected void HandleRowCreated (object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow && e.Row.RowIndex != UserGrid.EditIndex) {
				// Programmatically reference the Edit and Delete LinkButtons
				LinkButton EditButton = e.Row.FindControl ("EditButton") as LinkButton;
				LinkButton DeleteButton = e.Row.FindControl ("DeleteButton") as LinkButton;

				EditButton.Visible = Auth.Capabilities.Check("Firehall.Users.Edit");
				DeleteButton.Visible = Auth.Capabilities.Check("Firehall.Users.Delete");

				if(!EditButton.Visible && !DeleteButton.Visible){
					UserGrid.Columns[0].Visible = false;
				}
			}
		}

		protected void HandleRowDataBound (object sender, GridViewRowEventArgs e)
		{
			var row = e.Row;
			if (row.RowType == DataControlRowType.DataRow) {
				var user = (MembershipUser)e.Row.DataItem;

				//Delete button
				LinkButton del = row.FindControl("DeleteButton") as LinkButton;
				if(del != null){
					del.OnClientClick = string.Format (
						"return confirm('Confirm delete of user {0}.');",
						user.UserName.Replace ("'", @"\'"));
				}

				//role button
				HyperLink link = row.FindControl("RolesButton") as HyperLink;
				if(link != null){
					link.NavigateUrl = String.Format(
						"UserRoles.aspx?u={0}",
						Server.UrlEncode(user.UserName)
					);
				}
			}
		}

		protected void HandleUserCancelingEdit (object sender, GridViewCancelEditEventArgs e)
		{
			UserGrid.EditIndex = -1;
			BindUserGrid ();
		}

		protected void HandleUserUpdating (object sender, System.Web.UI.WebControls.GridViewUpdateEventArgs e)
		{
			if (!Auth.Capabilities.Check("User.Edit")) {
				return;
			}

			// Exit if the page is not valid
			if (!Page.IsValid) {
				return;
			}

			// Determine the username of the user we are editing
			string UserName = UserGrid.DataKeys [e.RowIndex].Value.ToString ();

			// Read in the entered information and update the user
			TextBox EmailTextBox = UserGrid.Rows[e.RowIndex].FindControl ("Email") as TextBox;
			TextBox CommentTextBox = UserGrid.Rows[e.RowIndex].FindControl ("Comment") as TextBox;

			// Return information about the user
			MembershipUser UserInfo = System.Web.Security.Membership.GetUser (UserName);

			// Update the User account information
			UserInfo.Email = EmailTextBox.Text.Trim ();
			UserInfo.Comment = CommentTextBox.Text.Trim ();

			System.Web.Security.Membership.UpdateUser (UserInfo);

			// Revert the grid's EditIndex to -1 and rebind the data
			UserGrid.EditIndex = -1;
			BindUserGrid ();
		}

		protected void HandleuserEditing (object sender, System.Web.UI.WebControls.GridViewEditEventArgs e)
		{
			UserGrid.EditIndex = e.NewEditIndex;
			BindUserGrid ();
		}

		protected void HandleUserDeleting (object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
		{
			// Determine the username of the user we are editing
			string UserName = UserGrid.DataKeys[e.RowIndex].Value.ToString();
			
			// Delete the user
			System.Web.Security.Membership.DeleteUser(UserName);

			// Revert the grid's EditIndex to -1 and rebind the data
			UserGrid.EditIndex = -1;
			BindUserGrid();
		}

		private void BindUserGrid ()
		{    
			MembershipUserCollection allUsers = System.Web.Security.Membership.GetAllUsers ();
			UserGrid.DataSource = allUsers;
			UserGrid.DataBind ();
		}
	}
}


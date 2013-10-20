using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Security = System.Web.Security;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vius.Authentication;

namespace Firehall.Administration.Roles
{
	public partial class RoleCapabilities : Firehall.Page
	{
		private Role role;

		/// <summary>
		/// Gets the role this page is associated with
		/// </summary>
		/// <value>
		/// The role.
		/// </value>
		protected Role Role {
			get {
				if(role == null){
					role = Role.GetRole(RoleName.Text);
				}
				return role;
			}
		}

		protected void Page_Load (object sender, EventArgs e)
		{ 
			if (!Page.IsPostBack) {
				var name = Request["r"];
				if(string.IsNullOrEmpty(name)){
					Response.Redirect("ManageRoles.aspx",true);
				}
				RoleName.Text = name.ToString();
				BindCapabilitiesToList (); 
			} 
		}

		/// <summary>
		/// Handles the click changing the capability
		/// </summary>
		/// <param name='sender'>
		/// Checkbox that was clicked
		/// </param>
		/// <param name='e'>
		/// Arguments
		/// </param>
		protected void HandleCapabilityChanged (object sender, System.EventArgs e)
		{
			var checkbox = (CheckBox)sender;
			var capability = checkbox.Text;
			var role = this.Role;

			if (checkbox.Checked) {
				role += capability;
			} else {
				role -= capability;
			}
		}

		/// <summary>
		/// Binds the capabilities to list.
		/// </summary>
		private void BindCapabilitiesToList ()
		{ 
			// Get all of the roles 
			ReadOnlyCollection<string> capabilities = Vius.Authentication.Capabilities.AllCapabilities;
			CapabilityList.DataSource = capabilities;
			CapabilityList.DataBind();

			// Loop through the Repeater's Items and check or uncheck the checkbox as needed 
			foreach (RepeaterItem ri in CapabilityList.Items) { 
				// Programmatically reference the CheckBox 
				CheckBox CapabilityCheckBox = (CheckBox)ri.FindControl ("CapabilityCheckBox"); 
				// See if RoleCheckBox.Text is in selectedUsersRoles 
				CapabilityCheckBox.Checked = this.Role.Contains(CapabilityCheckBox.Text);
			} 

		}

	}
}


using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Security = System.Web.Security;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Firehall.Administration.Roles
{
	public partial class RoleCapabilities : Firehall.Page
	{
		private Vius.Role role;

		protected void Page_Load (object sender, EventArgs e)
		{ 
			if (!Page.IsPostBack) {
				//TODO: implement these
				//string role = Request["r"].ToString();
				//this.role = Vius.Role.GetRole(role);

				BindCapabilitiesToList (); 
				//CheckCapabilitiesForRole();
			} 
		}

		protected void HandleCapabilityChanged (object sender, System.EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void BindCapabilitiesToList ()
		{ 
			// Get all of the roles 
			ReadOnlyCollection<string> capabilities = Vius.Capabilities.AllCapabilities;
			CapabilityList.DataSource = capabilities;
			CapabilityList.DataBind();
		}

		private void CheckCapabilitiesForRole ()
		{
			// Loop through the Repeater's Items and check or uncheck the checkbox as needed 
			role.ImmediateSave = false;
			foreach (RepeaterItem ri in CapabilityList.Items) { 
				// Programmatically reference the CheckBox 
				CheckBox CapabilityCheckBox = (CheckBox)ri.FindControl ("CapabilityCheckBox"); 
				// See if RoleCheckBox.Text is in selectedUsersRoles 
				CapabilityCheckBox.Checked = role.Contains(CapabilityCheckBox.Text);
			} 
			role.Save();
		}

	}
}


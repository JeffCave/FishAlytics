
using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;

namespace Firehall
{
	public partial class CreatingUserAccounts : System.Web.UI.Page
	{
		const string passwordQuestion = "What is your favourite colour";

		protected void Page_Load (object sender, EventArgs e)
		{
			if (!IsPostBack) {
				SecurityQuestion.Text = passwordQuestion;
			}
		}

		protected void CreateAccountButton_Click (object sender, EventArgs e)
		{
			MembershipCreateStatus createStatus;
			//MembershipUser newUser = 
			Membership.CreateUser (Username.Text, Password.Text, Email.Text, SecurityQuestion.Text, SecurityAnswer.Text, true, out createStatus);
			switch (createStatus) {
			case MembershipCreateStatus.Success:
				CreateAccountResults.Text = "The user account was successfully created.";
				break;
			default:
				CreateAccountResults.Text = String.Format (
							"Status returned: {0}",
							Enum.GetName (typeof(MembershipCreateStatus), createStatus)
				);
				break;

			}
		}


	}
}


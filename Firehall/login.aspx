<%@ Page Language="C#" Inherits="Firehall.Login" MasterPageFile="~/Site.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<asp:Login id="Login1" runat="server"
			CreateUserText="Not registered yet? Create an account!"
			CreateUserUrl="~/Membership/CreatingUserAccounts.aspx"
			RemberMeSet="True"
			TitleText=""
			UserNameLabelText="Username:"
		/>
	<asp:ValidationSummary id="ValidationSummary1" ValidationGroup="Login1" runat="server" />
</asp:Content>

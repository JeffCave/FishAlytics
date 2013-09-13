<%@ Page Language="C#" AutoeventWireup="true" Inherits="Firehall.CreatingUserAccounts" MasterPageFile="~/Site.master" CodeFile="CreatingUserAccounts.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
<asp:CreateUserWizard id="RegisterUser" runat="server">
	<WizardSteps>
		<asp:CreateUserWizardStep id="CreateUserStep1" runat="server" UserLabelText="Test" />
		<asp:CompleteWizardStep id="CompletedStep1" runat="server" />
	</WizardSteps>
</asp:CreateUserWizard>
</asp:Content>

<%@ Page Language="C#" AutoeventWireup="true" Inherits="Firehall.CreatingUserAccounts" MasterPageFile="~/Site.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
<asp:CreateUserWizard id="RegisterUser" runat="server">
	<WizardSteps>
		<asp:CreateUserWizardStep id="CreateUserStep1" runat="server" />
		<asp:WizardStep id="SpecifyRolesStep" runat="server" StepType="Step" Title="Specify Roles" AllowReturn="False">
			<asp:CheckBoxList id="RoleList" runat="server">
			</asp:CheckBoxList>
		</asp:WizardStep>
		<asp:CompleteWizardStep id="CompletedStep1" runat="server" />
	</WizardSteps>
</asp:CreateUserWizard>
</asp:Content>

<%@ Page Language="C#" Inherits="Firehall.ManageRoles" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<b>Create a New Role: </b>
<asp:TextBox ID="RoleName" runat="server" />
<br />
<asp:Label id="MessageBox" runat="server" />
<br />
<asp:Button ID="CreateRoleButton" runat="server" Text="Create Role" />
<br />
<asp:GridView ID="RoleList" runat="server" AutoGenerateColumns="false">
	<Columns>
		<asp:TemplateField ShowHeader="False">
			<ItemTemplate>
				<asp:LinkButton ID="DeleteButton" runat="server" CausesValidation="False" CommandName="Delete" Text="[x]"></asp:LinkButton>
				<asp:HyperLink Id="RoleButton" runat="server">[c]</asp:HyperLink>
			</ItemTemplate>
		</asp:TemplateField>
		<asp:TemplateField HeaderText="Role">
			<ItemTemplate>
				<asp:Label runat="server" ID="RoleNameLabel" />
			</ItemTemplate>
		</asp:TemplateField>
	</Columns>    
</asp:GridView>
</asp:Content>




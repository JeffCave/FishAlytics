<%@ Page Language="C#" Inherits="Firehall.Administration.Members.UserRoles" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<h3>Manage Roles by User</h3>
	<p align="center">
		<asp:Label ID="ActionStatus" runat="server" CssClass="Important"></asp:Label> 
	</p>
	<p>
		<asp:DropDownList ID="UserList" runat="server" AutoPostBack="True" DataTextField="UserName" DataValueField="UserName" /> 
	</p>
	<p>
		<asp:Repeater ID="UsersRoleList" runat="server"> 
			<ItemTemplate>
				<asp:CheckBox runat="server" ID="RoleCheckBox" AutoPostBack="true" Text='<%# Container.DataItem %>' OnCheckedChanged="HandleRoleChanged" /> 
				<br /> 
			</ItemTemplate> 
		</asp:Repeater>
	</p>
	

</asp:Content>

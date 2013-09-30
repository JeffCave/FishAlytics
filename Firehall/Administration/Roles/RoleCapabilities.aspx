<%@ Page Language="C#" Inherits="Firehall.Administration.Roles.RoleCapabilities" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<h3>Manage Role Capabilities</h3>
	<p align="center">
		<asp:Label ID="ActionStatus" runat="server" CssClass="Important"></asp:Label> 
	</p>
	<p>
		<asp:Label ID="RoleName" runat="server" /> 
	</p>
	<p>
		<asp:Repeater ID="CapabilityList" runat="server"> 
			<ItemTemplate>
				<asp:CheckBox runat="server" ID="CapabilityCheckBox" AutoPostBack="true" Text='<%# Container.DataItem %>' OnCheckedChanged="HandleCapabilityChanged" /> 
				<br /> 
			</ItemTemplate> 
		</asp:Repeater>
	</p>
	
</asp:Content>

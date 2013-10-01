<%@ Page Language="C#" Inherits="Firehall.Administration.Roles.RoleCapabilities" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<p>
	This is a listing of the current system capabilities associated with the "<%=this.Role.Name%>" role.
	</p>
	<p>
		<h3><asp:Label ID="RoleName" runat="server" /> </h3>
	</p>
	<ul id="CapabilityList" class="DataList">
		<asp:Repeater ID="CapabilityList" runat="server"> 
			<ItemTemplate>
				<li><asp:CheckBox runat="server" ID="CapabilityCheckBox" AutoPostBack="true" Text='<%# Container.DataItem %>' OnCheckedChanged="HandleCapabilityChanged" /></li>
			</ItemTemplate> 
		</asp:Repeater>
	</ul>
	
</asp:Content>

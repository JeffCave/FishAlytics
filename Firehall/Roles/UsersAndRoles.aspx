<%@ Page Language="C#" Inherits="Firehall.UsersAndRoles" MasterPageFile="~/Site.master" %>
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
	
	<h3>Manage Users By Role</h3> 
	<p> 
		<b>Select a Role:</b> 
		<asp:DropDownList ID="RoleList" runat="server" AutoPostBack="true" />
	</p> 
<p>      <asp:GridView ID="RolesUserList" runat="server" AutoGenerateColumns="false" 

          EmptyDataText="No users belong to this role."> 
          <Columns>
            		<asp:CommandField DeleteText="Delete User" ShowDeleteButton="true" />

               <asp:TemplateField HeaderText="Users"> 
                    <ItemTemplate> 
                         <asp:Label runat="server" id="UserNameLabel" 
                              Text='<%# Container.DataItem %>'></asp:Label> 

                    </ItemTemplate> 
               </asp:TemplateField> 
          </Columns> 
     </asp:GridView> </p>
     <p> 
     <b>UserName:</b> 
     <asp:TextBox ID="UserNameToAddToRole" runat="server"></asp:TextBox> 
     <br /> 
     <asp:Button ID="AddUserToRoleButton" runat="server" Text="Add User to Role" /> 

</p>
</asp:Content>

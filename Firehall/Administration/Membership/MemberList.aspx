<%@ Page Language="C#" Inherits="Firehall.Admnistration.Membership.MemberList" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<asp:LoginView ID="LoginView1" runat="server">
	  <RoleGroups>
			<asp:RoleGroup Roles="Administrators">
				<ContentTemplate>
					As an Administrator, you may edit and delete user accounts. 
					Remember: With great power comes great responsibility!
				</ContentTemplate>
			</asp:RoleGroup>
			<asp:RoleGroup Roles="Supervisors">
				<ContentTemplate>
					As a Supervisor, you may edit users&#39; Email and Comment information. 
					Simply click the Edit button, make your changes, and then click Update.
				</ContentTemplate>
			</asp:RoleGroup>
		</RoleGroups>
		<LoggedInTemplate>
			You are not a member of the Supervisors or Administrators roles. Therefore you cannot edit or delete any user information.
		</LoggedInTemplate>
		<AnonymousTemplate>
			You are not logged into the system. Therefore you cannot edit or delete any user information.
		</AnonymousTemplate>
	</asp:LoginView>
	<asp:GridView runat="server" id="UserGrid" AutoGenerateColumns="false" DataKeyNames="UserName">
		<Columns>
			<asp:TemplateField ShowHeader="False">
				<EditItemTemplate>
					<asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True"  CommandName="Update" Text="Update"></asp:LinkButton>
					<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
				</EditItemTemplate>
				<ItemTemplate>
					<asp:LinkButton ID="EditButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
					<asp:LinkButton ID="DeleteButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
					<asp:LinkButton ID="RolesButton" runat="server" CausesValidation="False" CommandName="Roles" Text="Roles"></asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:CommandField DeleteText="[X]" EditText="[e]" ShowDeleteButton="true" ShowEditButton="true" />
			<asp:boundfield datafield="UserName" readonly="true" headertext="User"/>
			<asp:boundfield datafield="LastLoginDate" readonly="true" headertext="Last" HtmlEncode="false" DataFormatString="{0:d}" />
			<asp:TemplateField HeaderText="Email">    
				<ItemTemplate>    
					<asp:Label runat="server" ID="Label1" Text='<%# Eval("Email") %>' />
				</ItemTemplate>    
				<EditItemTemplate>    
					<asp:TextBox runat="server" ID="Email" Text='<%# Bind("Email") %>' />
					
          <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"    
               ControlToValidate="Email" Display="Dynamic"    
               ErrorMessage="You must provide an email address." 
               SetFocusOnError="True">*</asp:RequiredFieldValidator>    
					
          <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"    
               ControlToValidate="Email" Display="Dynamic"    
               ErrorMessage="The email address you have entered is not valid. Please fix 
               this and try again."    
               SetFocusOnError="True"    

               ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*">*
					</asp:RegularExpressionValidator>    
				</EditItemTemplate>    
			</asp:TemplateField>

			<asp:TemplateField HeaderText="Comment">    
				<ItemTemplate>    
					<asp:Label runat="server" ID="Label2" Text='<%# Eval("Comment") %>'></asp:Label>    
				</ItemTemplate>    
				<EditItemTemplate>    
					<asp:TextBox runat="server" ID="Comment" TextMode="MultiLine" Columns="40" Rows="4" Text='<%# Bind("Comment") %>'></asp:TextBox>    
				</EditItemTemplate>    
			</asp:TemplateField>
			
		</Columns>
	</asp:GridView>
	<asp:ValidationSummary ID="ValidationSummary1" runat="server" ShowMessageBox="True" ShowSummary="False" />
</asp:Content>

<%@ Page Language="C#" AutoeventWireup="true" Inherits="Firehall.CreatingUserAccounts" MasterPageFile="~/Site.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
<table>
	<tr>
		<th>Alias</th>
		<td><asp:TextBox runat="server" id="Username" /></td>
	</tr>
	<tr>
		<th>Password</th>
		<td><asp:TextBox runat="server" id="Password" TextMode="Password"></asp:TextBox> </td>
	</tr>
	<tr>
		<th>Email</th>
		<td><asp:TextBox runat="server" id="Email" /></td>
	</tr>
	<tr>
		<th><asp:Label runat="server" id="SecurityQuestion" Text="" /></th>
		<td><asp:TextBox runat="server" id="SecurityAnswer" /></td>
	</tr>
	<tr>
		<td colspan="2">
			<asp:Button runat="server" id="CreateAccountButton" Text="Create the User Account" OnClick="CreateAccountButton_Click" />
		</td>
	</tr>
	<tr>
		<td colspan="2"><asp:Label runat="server" id="CreateAccountResults" /></td>
	</tr>
</table>
</asp:Content>

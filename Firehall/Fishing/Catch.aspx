<%@ Page Language="C#" Inherits="Firehall.Catch" MasterPageFile="~/Site.master" %>
<%@ MasterType VirtualPath="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<table>
		<tr>
			<th><asp:Label runat="server" id="lblTime">When:</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Time" Text='' /></td>
		</tr>
		<tr>
			<th><asp:Label runat="server" id="lblSpecies">Species:</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Species" Text='' /></td>
		</tr>
		<tr>
			<th><asp:Label runat="server" id="lblLength">Length:</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Length" Text='' /></td>
		</tr>
		<tr>
			<th><asp:Label runat="server" id="lblWeight">Weight</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Weight" Text='' /></td>
		</tr>
	</table>
</asp:Content>



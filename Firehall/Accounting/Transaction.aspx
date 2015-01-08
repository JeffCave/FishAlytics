<%@ Page Language="C#" Inherits="Firehall.Transaction" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<table>
		<tr>
			<th><asp:Label runat="server" id="lblId">Transaction ID:</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Id" Text='' /></td>
		</tr>
		<tr>
			<th><asp:Label runat="server" id="lblDate">Date</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Date" Text='' /></td>
		</tr>
		<tr>
			<th><asp:Label runat="server" id="lblComment">Comment</asp:Label></th>
			<td><asp:TextBox runat="server" ID="Comment" Text='' /></td>
		</tr>
	</table>

</asp:Content>



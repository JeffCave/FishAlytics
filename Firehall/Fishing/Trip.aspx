<%@ Page Language="C#" Inherits="Firehall.Trip" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<table class="form">
	<tr>
		<td colspan="2">
			<asp:Button runat="server" id="Save" />
		</td>
	</tr>
	<tr>
		<th><asp:Label runat="server" id="lblTripStart">Start</asp:Label></th>
		<td><asp:TextBox runat="server" id="TripStart" /></td>
	</tr>
	<tr>
		<th><asp:Label runat="server" id="lblTripEnd">End</asp:Label></th>
		<td><asp:TextBox runat="server" id="TripEnd" /></td>
	</tr>	
	<tr>
		<th><asp:Label runat="server" id="lblRoute">Route</asp:Label></th>
		<td>
			<asp:Repeater runat="server" id="Route">
				<ItemTemplate>
					<asp:TextBox runat="server" id="lat" />
					<asp:TextBox runat="server" id="long" />
					<br />
				</ItemTemplate>
			</asp:Repeater>
		</td>
	</tr>	
</table>
</asp:Content>



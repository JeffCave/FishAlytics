<%@ Page Language="C#" Inherits="Firehall.Login" MasterPageFile="~/Site.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<asp:Login id="Login1" runat="server" />
	<asp:ValidationSummary id="ValidationSummary1" ValidationGroup="Login1" runat="server" />
	<asp:Label id="InvalidCredentialsMessage" runat="server" visible="false">
		Login/Password doesn't match anything in our system.
	</asp:Label>
</asp:Content>

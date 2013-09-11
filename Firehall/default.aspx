<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" Inherits="Firehall.Default" CodeFile="default.aspx.cs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">

	<asp:Panel runat="server" ID="AuthenticatedMessagePanel">
		<asp:Label runat="server" ID="WelcomeBackMessage"></asp:Label>
	</asp:Panel>
	
	<asp:Panel runat="Server" ID="AnonymousMessagePanel">
		<asp:HyperLink runat="server" ID="lnkLogin" Text="Log In" NavigateUrl="~/login.aspx"></asp:HyperLink>
	</asp:Panel>
	
</asp:Content>
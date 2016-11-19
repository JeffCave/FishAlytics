<%@ Page Language="C#" Inherits="Firehall.Fishing.Catches" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

 <a href="./Catch.aspx">New</a>
	<asp:GridView runat="server" id="CatchesGrid" AutoGenerateColumns="true" DataKeyNames="CatchId">
	</asp:GridView>

</asp:Content>

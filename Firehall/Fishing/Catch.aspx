<%@ Page Language="C#" Inherits="Firehall.Fishing.Catch" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<input type='hidden' id='id' value='' />

When:<br />
<input type="datetime" id="Time" value='' /><br />
<br />
Species:<br />
<input type='text' id="Species" value='' /><br />
<br />
Length:<br />
<input type='number' id="Length" value='' /><br />
<br />
Weight:<br />
<input type='number' id="Weight" value='' /><br />
<br />
<input type='submit' id="action" value='Save' /><br />

</asp:Content>



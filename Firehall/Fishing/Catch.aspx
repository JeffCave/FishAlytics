<%@ Page Language="C#" Inherits="Firehall.Fishing.Catch" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<form method="delete">
<input type='hidden' id='id' name="CatchId" value='<%=datum.CatchId%>' />

<p>
<label for="Time">When</label>
<input type="datetime" id="Time" name="Time" value='<%=datum.Time%>' />
</p>
<lable for="Species">Species<br />
<input type='text' id="Species" value='' /><br />
<br />
Length:<br />
<input type='number' id="Length" value='' /><br />
<br />
Weight:<br />
<input type='number' id="Weight" value='' /><br />
<br />
<input type='submit' id="action" value='Save' /><br />
</form>
</asp:Content>



<%@ Page Language="C#" Inherits="Firehall.Administration.Sitemap.Sitemap" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<table>
 <thead>
  <tr>
   <th>Page</th>
   <th>Id</th>
   <th>Title</th>
   <th>Director</th>
   <th>DateReleased</th>
  </tr>
 </thead>
 <tbody>
  <% this.RenderTableRows(); %>
 </tbody>
</table>
</asp:Content>
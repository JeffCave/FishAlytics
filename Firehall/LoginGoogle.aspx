<%@ Page Language="C#" AutoEventWireup="true" Inherits="Firehall.LoginGoogle" Codebehind="LoginGoogle.aspx.cs" %>
<!DOCTYPE html>
<html>
<head>
 <title></title>
</head>
<body>
 <a href="<%=this.GoogleTarget%>">
  <img src="https://developers.google.com/+/images/branding/sign-in-buttons/Red-signin_Medium_base_44dp.png" />
 </a>
 <div>Welcome, <%=this.UserEmail %></div>
</body>
</html>

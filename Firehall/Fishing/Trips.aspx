<%@ Page Language="C#" Inherits="Firehall.Fishing.Trips" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	<asp:GridView runat="server" id="TripGrid" AutoGenerateColumns="false" DataKeyNames="Id">
		<Columns>
			<asp:TemplateField ShowHeader="False">
				<ItemTemplate>
					<asp:HyperLink Id="ViewButton" runat="server">[v]</asp:HyperLink>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField headertext="Start">
				<ItemTemplate>
					<asp:Label Id="StartTime" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField headertext="Finish">
				<ItemTemplate>
					<asp:Label Id="FinishTime" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField headertext="Duration">
				<ItemTemplate>
					<asp:Label Id="Duration" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField headertext="Fisherman">
				<ItemTemplate>
					<asp:Label Id="Fisherman" runat="server" />
				</ItemTemplate>
			</asp:TemplateField>
		</Columns>
	</asp:GridView>

</asp:Content>



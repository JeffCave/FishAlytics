<%@ Page Language="C#" Inherits="Firehall.Trip" MasterPageFile="~/Site.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<ajax:ToolkitScriptManager runat="Server" EnableScriptGlobalization="true" EnableScriptLocalization="true" ID="ScriptManager1" CombineScripts="false" />

<ul>
<asp:Repeater id="Messages" runat="server">
<ItemTemplate>
<li><%# Container.DataItem %></li>
</ItemTemplate>
</asp:Repeater>
</ul>



<asp:Button runat="server" id="btnSave" Text="Save" />
<asp:Button runat="server" id="btnCatch" Text="Fish On!" />
<asp:Button runat="server" id="btnChangeRig" Text="Change Rig" />
<input type="checkbox" id="chkFishing">Line In/Out</input>

<h2>Time</h2>
<div class="bundle" style="display:block">
	<asp:Label runat="server" id="lblTripDate">Date</asp:Label><br />
	<asp:TextBox runat="server" id="txtTripDate" autocomplete="off" />
	<ajax:CalendarExtender ID="ajaxTripDate" runat="server" TargetControlID="txtTripDate" />
</div>
<div class="bundle" style="display:inline-block">
	<asp:Label runat="server" id="lblTripStart">Start</asp:Label><br />
	<asp:TextBox runat="server" id="txtTripStart" autocomplete="off" />
</div>
--->
<div id="TripEnd" class="bundle" style="display:inline-block">
	<asp:Label runat="server" id="lblTripEnd">End</asp:Label><br />
	<asp:TextBox runat="server" id="txtTripEnd" />
	<input type="button" value="Done" onclick="document.getElementById('TripEnd').visible=true;this.visible=false;"/>
</div>	
<div class="bundle" style="display:block">
	<asp:Label runat="server" id="lblTripDuration">Duration</asp:Label><br />
	<asp:TextBox runat="server" id="TripDuration" autocomplete="off" />
</div>

<h2><asp:Label runat="server" id="lblMap">Map</asp:Label></h2>
<input type="range" name="DisplayRange" min="1" max="255" value="255" />
<div id="map" class="map" style="width:512px;height:256px;border:1px solid #ccc;" ></div>

<h2><asp:Label runat="server" id="lblFriends">Friends</asp:Label></h2>
<div class="select" style="display:inline-block;">
	<span style="display:block;" class="label">Came With:</span>
	<select style="width:200px;" multiple="multiple" id="joined" size="10">
		<option value="2">Joe</option>
	</select>
</div>
<div class="select" style="display:inline-block;">
	<span style="display:block;">Punked Out:</span>
	<select style="width:200px;" multiple="multiple" id="available" size="10">
		<option value="1">Mike</option>
		<option value="5">Shannon</option>
		<option value="9">Jake</option>
		<option value="12">John</option>
	</select>
</div>

<h2><asp:Label runat="server" id="lblCatches">Catches</asp:Label></h2>
<table>
	<tr>
		<th>Time</th>
		<th>Species</th>
		<th>Length</th>
		<th>Weight</th>
	</tr>
	<tr>
		<td><asp:TextBox id="Time" runat="server">Time</asp:TextBox></td>
		<td><asp:TextBox id="Species" runat="server">Species</asp:TextBox></td>
		<td><asp:TextBox id="Length" runat="server">Length</asp:TextBox></td>
		<td><asp:TextBox id="Weight" runat="server">Weight</asp:TextBox></td>
	</tr>
</table>

<h2><asp:Label runat="server" id="lblRig">Rig</asp:Label></h2>
<p>
This might work better as "tags" where the user can enter any 
keyword, but we suggest keywords as they type. So as the user
starts to type "fly" we suggest "fly-fishing"; as they type "min" we
suggest minnow. This would allow users to enter the names of all their 
favourite rigs, with the little variations:
<ul>
	<li>"Carolina Rig", "Yamagatsu Worm"</li>
	<li>"Yamagatsu", "Wacky"</li>
	<li>"frozen minnow","spinner","slow retrieve"</li>
	<li>"live minnow","bobber"</li>
	<li>"live worm","bobber"</li>
</ul>
</p>
<p>
Based on the data above, the user favours Yamagatsu's, but experiments with
different rigs. We would likely want to advertise Yamagatsu products.
</p>
<p>
As we learn more about the type of data users are entering, we can suggest
more strongly, but the first bit is about learning.
</p>
<table>
	<tr>
		<th>Time</th>
		<th>Rig</th>
	</tr>
	<tr>
		<td><asp:TextBox id="Time_1" runat="server">20:13</asp:TextBox></td>
		<td><asp:TextBox id="Rig_1" runat="server">"Gulp! Minnow","jerky retrieve","spinner","1/8 ounce"</asp:TextBox></td>
	</tr>
	<tr>
		<td><asp:TextBox id="Time_2" runat="server">20:13</asp:TextBox></td>
		<td><asp:TextBox id="Rig_2" runat="server">"frozen minnow","slow retrieve","spinner","1/8 ounce"</asp:TextBox></td>
	</tr>
</table>



<script src='http://dev.virtualearth.net/mapcontrol/v3/mapcontrol.js'></script>
<script src="http://openlayers.org/api/OpenLayers.js"></script>
<script defer="defer" type="text/javascript">
	var map = new OpenLayers.Map('map');
	//var layerBing = new OpenLayers.Layer.VirtualEarth("VE");
	var layerOSM = new OpenLayers.Layer.XYZ(
            "OpenStreetMap", 
            [
                "http://otile1.mqcdn.com/tiles/1.0.0/map/${z}/${x}/${y}.png",
                "http://otile2.mqcdn.com/tiles/1.0.0/map/${z}/${x}/${y}.png",
                "http://otile3.mqcdn.com/tiles/1.0.0/map/${z}/${x}/${y}.png",
                "http://otile4.mqcdn.com/tiles/1.0.0/map/${z}/${x}/${y}.png"
            ],
            {
                attribution: "Data, imagery and map information provided by <a href='http://www.mapquest.com/'  target='_blank'>MapQuest</a>, <a href='http://www.openstreetmap.org/' target='_blank'>Open Street Map</a> and contributors, <a href='http://creativecommons.org/licenses/by-sa/2.0/' target='_blank'>CC-BY-SA</a> <img src='http://developer.mapquest.com/content/osm/mq_logo.png' border='0' />",
                transitionEffect: "resize"
            }
        );
	var layerMapQuest = new OpenLayers.Layer.XYZ(
            "Imagery",
            [
                "http://otile1.mqcdn.com/tiles/1.0.0/sat/${z}/${x}/${y}.png",
                "http://otile2.mqcdn.com/tiles/1.0.0/sat/${z}/${x}/${y}.png",
                "http://otile3.mqcdn.com/tiles/1.0.0/sat/${z}/${x}/${y}.png",
                "http://otile4.mqcdn.com/tiles/1.0.0/sat/${z}/${x}/${y}.png"
            ],
            {
                attribution: "Tiles Courtesy of <a href='http://open.mapquest.co.uk/' target='_blank'>MapQuest</a>. Portions Courtesy NASA/JPL-Caltech and U.S. Depart. of Agriculture, Farm Service Agency. ",
                transitionEffect: "resize"
            }
        );
	var layerOpenLayer = new OpenLayers.Layer.WMS(
			"OpenLayers WMS",
			"http://vmap0.tiles.osgeo.org/wms/vmap0", 
			{layers: 'basic'} 
		);
	var layerDmData = new OpenLayers.Layer.WMS(
		"Canadian Data",
		"http://www2.dmsolutions.ca/cgi-bin/mswms_gmap",
		{
			layers: 
//				"bathymetry," +
				"land_fn," + 
				"park," + 
				"drain_fn," + 
				"drainage," +
//			  "prov_bound," + 
//			  "fedlimit," + 
			  "rail," + 
//			  "popplace,",
			  "road" + 
			transparent: "true",
			format: "image/png"
		},
		{isBaseLayer: false}
	);


	var layerVector = new OpenLayers.Layer.Vector("Overlay");
	var feature = new OpenLayers.Feature.Vector(
					new OpenLayers.Geometry.Point(-71, 42),
					{some:'data'},
					{
						externalGraphic: 'http://www.google.com/mapfiles/marker.png', 
						graphicHeight: 21, 
						graphicWidth: 16
					}
				);
	layerVector.addFeatures(feature);
	
	
	map.addLayer(layerOpenLayer);
//	map.addLayer(layerBing);
	map.addLayer(layerDmData);
	map.addLayer(layerVector);
	map.addLayer(layerOSM);
	map.addLayer(layerMapQuest);
	
	map.zoomToMaxExtent();
	map.addControl(new OpenLayers.Control.LayerSwitcher());
</script>
</asp:Content>



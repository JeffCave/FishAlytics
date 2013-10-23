<%@ Page Language="C#" Inherits="Firehall.Trip" MasterPageFile="~/Site.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<table class="form">
	<tr>
		<td colspan="2">
			<asp:Button runat="server" id="Save" Text="Save" />
		</td>
	</tr>
	<tr>
		<td></td>
		<td>
			<div class="bundle" style="display:inline-block">
				<asp:Label runat="server" id="lblTripStart">Start</asp:Label><br />
				<asp:TextBox runat="server" id="TripStart" />
			</div>
			<input type="range" name="DisplayRange" min="1" max="255" value="255" />
			<div class="bundle" style="display:inline-block">
				<asp:Label runat="server" id="lblTripEnd">End</asp:Label><br />
				<asp:TextBox runat="server" id="TripEnd" />
			</div>	
		</td>
	</tr>
	<tr>
		<th><asp:Label runat="server" id="lblMap">Map</asp:Label></th>
		<td>
			<div id="map" class="map" style="width:512px;height:256px;border:1px solid #ccc;" ></div>
		</td>
	</tr>
	<tr>
		<th><asp:Label runat="server" id="lblFriends">Friends</asp:Label></th>
		<td>
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
		</td>
	</tr>	
	<tr>
		<th><asp:Label runat="server" id="lblCatches">Catches</asp:Label></th>
		<td>
			<asp:Repeater runat="server" id="Catches">
				<ItemTemplate>
					<img id="FishThumb" runat="server" src="/icons/default-fish.png"/>
					<asp:TextBox id="Name" runat="server">Time</asp:TextBox>
					<asp:TextBox id="Name" runat="server">Size</asp:TextBox>
				</ItemTemplate>
			</asp:Repeater>
		</td>
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
                attribution: "Data, imagery and map information provided by <a href='http://www.mapquest.com/'  target='_blank'>MapQuest</a>, <a href='http://www.openstreetmap.org/' target='_blank'>Open Street Map</a> and contributors, <a href='http://creativecommons.org/licenses/by-sa/2.0/' target='_blank'>CC-BY-SA</a>  <img src='http://developer.mapquest.com/content/osm/mq_logo.png' border='0'>",
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
                attribution: "Tiles Courtesy of <a href='http://open.mapquest.co.uk/' target='_blank'>MapQuest</a>. Portions Courtesy NASA/JPL-Caltech and U.S. Depart. of Agriculture, Farm Service Agency. <img src='http://developer.mapquest.com/content/osm/mq_logo.png' border='0'>",
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



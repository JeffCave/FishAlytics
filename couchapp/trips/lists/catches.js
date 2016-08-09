function(head, req) {
	var Mustache = require("lib/mustache");
//	var List = require("vendor/couchapp/lib/list");
//	var path = require("vendor/couchapp/lib/path").init(req);
//	var Atom = require("vendor/couchapp/lib/atom");
//	
//	var indexPath = path.list('index','recent-posts',{descending:true, limit:10});
//	var feedPath = path.list('index','recent-posts',{descending:true, limit:10, format:"atom"});
//	var commentsFeed = path.list('comments','comments',{descending:true, limit:10, format:"atom"});
//	
//	var path_parts = req.path;
	// The provides function serves the format the client requests.
	// The first matching format is sent, so reordering functions changes 
	// thier priority. In this case HTML is the preferred format, so it comes first.
	provides("html", function() {
		var doc = {
			mapdata: {
				type: "FeatureCollection",
				features: []
			}
		};
		while (row = getRow()) {
			doc.mapdata.features.push({
					type: "Feature"
					,properties: {
						time: row.value.timestamp
						,"popupContent": Mustache.render(this.templates.partials.map.catch, row.value)
						,"effort":60/row.value.stats.time
					}
					,geometry: {
						type: "Point",
						coordinates: row.value.coords
					}
				});
		}
		doc.mapdata = JSON.stringify(doc.mapdata);
		html = Mustache.render(this.templates.catchmap, doc, this.templates.partials);
		return html;
	});
};


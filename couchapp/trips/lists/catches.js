/* global provides */
/* global getRow */
function lists_catches(head, req) {
	if(!req.userCtx.name){
		provides("html",function(){
			return [""
				,"<html><body>"
				,"<p>Attempting to redirect to google auth</p>"
				,"<script>setTimeout(function(){window.location='./auth'},1000)</script>"
				,"</body></html>"
			].join('\n')
			;

		});
		return;
	}
	var Mustache = require("lib/mustache");
	var utils = require("lib/utils");
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
			},
			PageTitle:"Catches",
			userCtx : req.userCtx
		};
		for (var row = getRow(); row; row = getRow()) {
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
		doc.BaseUrl = utils.getBaseUrl(req);
		doc.DbUrl = utils.getBasePath(req).slice(0,3).join('/');
		var html = Mustache.render(this.templates.catchmap, doc, this.templates.partials);
		return html;
	});
}


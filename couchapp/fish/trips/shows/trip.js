function showsTrip(doc, req) {  
	var ddoc = this;
	var Mustache = require("lib/mustache");
	
	var page = {
		header : {
			blogName : 'FishAlytics - Couch',
			session : req.userCtx,
		},
		scripts : {},
		pageTitle : doc ? doc.date : "Create a new post",
	};
	
	var data = {};
	if (doc) {
		data.doc = JSON.stringify(doc);
		data.date = new Date(doc.date).toISOString().substring(0,19);
		data.lat = doc.coords.latitude;
		data.long = doc.coords.longtitude;
		data.alt = doc.coords.altitude;
		data.species = doc.species;
		data.notes = doc.notes;
		//    data.tags = doc.tags.join(", ");
	} 
	else {
		data.doc = JSON.stringify({
			type : "post",
			format : "markdown"
		});
	}
	
	return Mustache.to_html(ddoc.templates.trip, data, ddoc.templates.partials);
}


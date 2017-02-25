/* global provides */
function listsTripts(head, req) {
	var Mustache = require("lib/mustache");
	var utils = require("lib/utils");
	
	// The provides function serves the format the client requests.
	// The first matching format is sent, so reordering functions changes 
	// thier priority. In this case HTML is the preferred format, so it comes first.
	provides("html", function() {
		var page = {
			PageTitle:"Trips"
			,userCtx : req.userCtx
			,BaseUrl : utils.getBaseUrl(req)
		};
		var html = Mustache.render(this.templates.trips, page, this.templates.partials);
		return html;
	});
}

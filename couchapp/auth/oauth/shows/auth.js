/**
 * 
 */
function (doc, req) { // jshint ignore: line
	var Mustache = require("lib/mustache");
	
	req.params = [];
	
	req.params = req.params.concat(Object.keys(req.form).map(function(k){
			return {
				'key' : k, 
				'val' : req.form[k],
			};
		}));
	req.params = req.params.concat(Object.keys(req.query).map(function(k){
			return {
				'key' : k, 
				'val' : req.query[k],
			};
		}));
	
	req.action = req.requested_path.join('/')
		.split('#')[0]
		.split("?")[0]
		.replace(/\/_show\/auth$/,'/_update/auth')
		;
	if(req.query.state){
		var state = JSON.parse(req.query.state);
		if(state._id){
			req.action += "/" + state._id;
		}
	}
	
	return Mustache.to_html(this.lib.templates.proxy, req);
}


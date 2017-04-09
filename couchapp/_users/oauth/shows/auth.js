/**
 * 
 */
function showAuth(doc, req) {
	var Mustache = require("lib/mustache");
	
	req.params = Object.keys(req.form).map(function(k){
			return {
				'key' : k, 
				'val' : req.form[k],
			};
		});
	req.params = req.params.concat(Object.keys(req.query).map(function(k){
			return {
				'key' : k, 
				'val' : req.query[k],
			};
		}));
	
	req.action = req.raw_path
		.split('#')[0]
		.split("?")[0]
		;
	if(req.query.state){
		var state = JSON.parse(req.query.state);
		if(state._id){
			req.loc += "/" + state._id;
		}
	}
	
	return Mustache.to_html(this.lib.proxy, req);
}


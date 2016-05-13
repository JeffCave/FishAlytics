function(doc, req) {  
	var ddoc = this;
	var Mustache = require("lib/mustache");
	var path = require("vendor/couchapp/lib/path").init(req);
    
    var data = {
    	header: {}
    	,scripts:{}
    	,title:'My Thingy'
    	,assets : path.asset()
    };
      
      
    return Mustache.to_html(ddoc.templates.login, data, ddoc.templates.partials);
}


function showLogin(doc, req) {  
	var ddoc = this;
	var Mustache = require("lib/mustache");
    
    var data = {
    	header: {}
    	,scripts:{}
    	,title:'My Thingy'
    };
      
      
    return Mustache.to_html(ddoc.templates.login, data, ddoc.templates.partials);
}


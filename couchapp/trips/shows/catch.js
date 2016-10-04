function(doc, req) {
	var utils = require("lib/utils");
	
	if(!req.userCtx.name){
		provides("html",function(){
			return [""
				,"<html><body>"
				,"<p>Attempting to redirect to google auth</p>"
				,"<script>setTimeout(function(){window.location='../auth'},1000)</script>"
				,"</body></html>"
			].join('\n')
			;

		});
		return;
	}
	var Mustache = require("lib/mustache");
	
	doc = doc || {};
	
	var page = {
		PageTitle : 'FishAlytics'
		,userCtx : req.userCtx
		
	};
	
	/*
	if(doc._id && doc._id.split(".")[0] !== "trip"){
		return {
			code:404
			,body:'' //this._attachments.404
		};
	}
	*/
	
	doc._id = doc._id || 'trip.' + Date.now();
	doc.catches = doc.catches || {};
	if(doc.catches.length === 0){
		return {
			code:416
			,body:'' //this._attachments.404
		};
	}
	
	page.doc = doc;

	page.BaseUrl = utils.getBaseUrl(req);
	page.debug = JSON.stringify(req,null,4);
	
	page.PageTitle = 'FishAlytics';
	page.NewCatchId = Math.exp(20).toFixed(0);
	page.catchid = Object.keys(req.query)[0] || undefined;
	page.catch = doc.catches[page.catchid] || {};
	page.id = (doc._id || "").split(".")[1] || "";
	page.catchids = [];
	for(var i=0; i<doc.catches.length; i++){
		page.catchids.push({
			key: i
			,value: doc.catches[i].timestamp
			,isCurrent:(i==page.catchid)
		});
	}
	return Mustache.to_html(this.templates.catch, page, this.templates.partials);
}

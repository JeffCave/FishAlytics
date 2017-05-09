module.exports = new (function(){ // jshint ignore:line
	
	this.getBaseUrl = function(req){
		return this.getBasePath(req).join('/');
	};
	
	this.getBasePath = function(req){
		var BaseUrl = req.headers.Referer;
		BaseUrl = !(BaseUrl && BaseUrl.split(':')[0] === 'https') ? "http:/" : "https:/";
		BaseUrl = [
				BaseUrl
				,req.headers.Host
			];
		if(req.requested_path[1] === '_design'){
			BaseUrl = BaseUrl.concat(req.requested_path.slice(0,4));
		}
		return BaseUrl;
	};
	
})();

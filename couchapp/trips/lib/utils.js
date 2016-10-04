module.exports = new (function(){
	
	this.getBaseUrl = function(req){
		return this.getBasePath(req).join('/');
	};
	
	this.getBasePath = function(req){
		var BaseUrl = req.headers.Referer;
		BaseUrl = !(BaseUrl && BaseUrl.split(':')[0] === 'https') ? "http:/" : "https:/";
		BaseUrl = [
				BaseUrl
				,req.headers.Host
				,req.path[0]
			];
		if(req.path[1] === '_design'){
			BaseUrl = BaseUrl.concat(req.requested_path.slice(1,4));
		}
		return BaseUrl;
	};
})();

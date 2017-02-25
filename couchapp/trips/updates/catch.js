function updatesCatch(doc, req) {
	if (!doc) {
		return [null,''];
	}
	
	//setup the meta data
	var now = (new Date()).toISOString();
	doc.meta = doc.meta || {
		created : now
	};
	doc.meta.modified = now;
	doc.meta.type = 'trip';
	doc.meta.by = req.userCtx.name || "un-authed";
	
	//attempt to make this look as much like a fishing trip as possible
	//in otherwords... initialize what we can
	doc.fisherman = doc.fisherman || req.userCtx.name;
	doc.catches = doc.catches || [];
	
	var catchid = Object.keys(req.query)[0];
	if(doc.catches.length <= catchid){
		catchid = doc.catches.length;
		doc.catches.push({});
	}
	
	var toApproxNum = function(o){
			var n = null;
			try{
				n = Math.abs(parseInt(o));
				var e = (o||"").slice(-1)=="~"?"~":"";
				if(e){
					n = n + e;
				}
				return n;
			}
			catch(ex){
				n = null;
			}
			return n;
		};
	
	var c = doc.catches[catchid];
	
	c.fish = c.fish || {};
	c.fish.species = (req.form.species || c.fish.species || "").trim() || undefined;
	c.fish.weight = toApproxNum(req.form.weight) || c.fish.weight || undefined;
	c.fish.length = toApproxNum(req.form.length) || c.fish.length || undefined;
	c.fish.girth = toApproxNum(req.form.girth) || c.fish.girth || undefined;
	
	this.renderer = eval("("+this.shows.catch+")");
	return [doc, this.renderer(doc,req)];
}

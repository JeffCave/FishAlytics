/* global toJSON */
function trips(doc, req) {
	if (!doc) {
		return [null,''];
	}
	//setup the meta data
	var now = Date.getDate().toISOString();
	doc._meta = doc._meta || {
		'created' : now,
	};
	doc._meta.modified = now;
	doc._meta.type = 'trip';
	doc._meta.by = req.userCtx.name;
	
	//attempt to make this look as much like a fishing trip as possible
	//in otherwords... initialize what we can
	doc.fisherman = doc.fisherman || req.userCtx.name;
	doc.catches = doc.catches || [];
	
	return [doc, toJSON(doc)];
}

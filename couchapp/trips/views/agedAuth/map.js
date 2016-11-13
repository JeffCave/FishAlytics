function(doc){
	if(doc._id.substring(0,5) != 'auth.') return;
	
	var canDelete = (true === doc.canDelete);
	
	var now = new Date();
	if(doc.expires < now.getTime()){
		canDelete = true;
	}
	
	if(canDelete){
		emit(doc._id, {
			_id: doc._id,
			_rev: doc._rev,
			_deleted:true
		});
	}
}

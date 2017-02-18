/* global emit */
function agedAuth(doc){
	if(doc._id.substring(0,5) != 'auth.') return;
	
	emit(doc.expires, {
		_id: doc._id,
		_rev: doc._rev,
		_deleted:true
	});
}

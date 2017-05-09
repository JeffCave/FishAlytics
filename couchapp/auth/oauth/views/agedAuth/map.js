/* global emit */
// jshint -W025
/**
 * trips/views/agedAuth
 */
function (doc){
	if(doc._id.substring(0,5) != 'auth.') return;
	
	emit(doc.expires, {
		_id: doc._id,
		_rev: doc._rev,
		_deleted:true
	});
}

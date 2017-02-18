/* global emit */
function licenses(doc) {
	if (doc._id.split('.')[0] !== 'lic') {
		return;
	}
	emit(doc._id, doc);
}

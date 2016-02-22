 
function(doc) {
	if (doc._id.substring(0,4) !== 'trip') {
	}
	doc.catches.foreach(function(catch){
		emit(doc.user, catch);
	});
};

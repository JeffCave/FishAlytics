/* global emit */
// jshint -W025
/**
 * trips/views/TripSummary
 */
function (doc) {
    if (doc._id.substring(0,5) !== 'trip.') {
		return;
	}
	var key = [
		doc.fisherman,
		doc._id.split('.')[1],
	];
	var agg = {
		"trip" : doc._id,
		"fef" : 0,
	};
	
	emit(key, agg);
}

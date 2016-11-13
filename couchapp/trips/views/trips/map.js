function(doc) {
    if (doc._id.substring(0,5) !== 'trip.') {
		return;
	}
	
	var d = JSON.parse(JSON.stringify(doc));
	var date = null;
	
	d.start = Date.parse("9000-01-01");
	d.finish = Date.parse("1970-11-01");
	d.catches.concat(d.waypoints).forEach(function(e){
		date = Date.parse(e.timestamp);
		if(isNaN(date)) return;
		if(date < d.start){
			d.start = date;
		}
		if(date > d.finish){
			d.finish = date;
		}
	});
	
	date = new Date(d.start);
	var key = [
		doc.fisherman,
		date.getUTCFullYear(),
		date.getUTCMonth()+1,
		date.getUTCDate(),
		date.getUTCHours()
	];
	
	emit(key, d);
};

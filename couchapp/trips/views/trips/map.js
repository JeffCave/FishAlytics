function trips(doc) {
    if (doc._id.substring(0,5) !== 'trip.') {
		return;
	}
	
	var d = JSON.parse(JSON.stringify(doc));
	var date = null;
	
	d.location = d.location || d._id;
	
	d.start = Date.parse("9000-01-01");
	d.finish = Date.parse("1970-11-01");
	d.fish = {
		'total' : d.catches.length,
		'length' : 0,
		'weight' : 0,
		'freq' : {}
	};
	d.catches.concat(d.waypoints).forEach(function(e){
		date = Date.parse(e.timestamp);
		if(isNaN(date)) return;
		if(date < d.start){
			d.start = date;
		}
		if(date > d.finish){
			d.finish = date;
		}
		if(e.fish){
			d.fish.freq[e.fish.species] = (d.fish.freq[e.fish.species] || 0)+1;
			d.fish.weight += e.fish.weight;
			d.fish.length += e.fish.length;
		}
	});
	
	
	var maxFish = null;
	for(var f in d.fish.freq){
		maxFish = maxFish || {'name':f,'count':d.fish.freq[f]};
		if(maxFish.count < d.fish.freq[f]){
			maxFish.name = f;
			maxFish.count = d.fish.freq[f];
		}
	}
	d.fish.freq = maxFish.name;
	
	
	date = new Date(d.start);
	var key = [
		doc.fisherman,
		date.getUTCFullYear(),
		date.getUTCMonth()+1,
		date.getUTCDate(),
		date.getUTCHours()
	];
	
	emit(key, d);
}

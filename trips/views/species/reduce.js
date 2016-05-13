function (key, values, rereduce) {
	
	var stats = function(values, rereduce){
		$stat = {
			sum:0
			,count:0
			,avg:0
		};
		values.forEach(function(val){
			if(rereduce){
				$stat.sum += val.sum;
				$stat.count += val.count;
				// this is a more interesting way to calculate average
				//$stat.avg += val.avg/val.count;
			}
			else{
				if(val!==null){
					$stat.sum += val;
					$stat.count++;
				}
			}
		});
		$stat.avg = $stat.sum/$stat.count;
		return $stat;
	};
	
	$val = {
		timestamp:[]
		,weight:[]
		,length:[]
	};
	
	values.forEach(function(val){
		$val.timestamp.push(val.timestamp);
		$val.weight.push(val.fish.weight);
		$val.length.push(val.fish.length);
	});
	$val.timestamp = stats($val.timestamp);
	$val.weight = stats($val.weight);
	$val.length = stats($val.length);
	
	return $val;
}

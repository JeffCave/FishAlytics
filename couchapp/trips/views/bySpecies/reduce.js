/**
 * Reduce Species 
 * 
 */
function bySpecies(key, values, rereduce) {
	
	var $stats = function(values, rereduce){
		let $stat = {
			sum:0
			,count:0
			,avg:0
		};
//		values.forEach(function(val){
//			if(rereduce){
//				$stat.sum += val.sum;
//				$stat.count += val.count;
//				// this is a more interesting way to calculate average
//				//$stat.avg += val.avg/val.count;
//			}
//			else{
//				if(val!==null){
//					$stat.sum += val;
//					$stat.count++;
//				}
//			}
//		});
//		$stat.avg = $stat.sum/$stat.count;
//		return $stat;
	};
	
	var $val = {
		timestamp:[]
		,weight:[]
		,length:[]
	};
	
	values.forEach(function(val){
//		$val.timestamp.push(val.timestamp);
//		$val.weight.push(val.fish.weight);
		$val.length.push(val.value.fish.length);
	});
//	$val.timestamp = stats($val.timestamp,rereduce);
//	$val.weight = stats($val.weight,rereduce);
//	$val.length = stats($val.length,rereduce);
//	$val.lwFactor = stats($val.lwFactor,rereduce);
	
	return $val;
}

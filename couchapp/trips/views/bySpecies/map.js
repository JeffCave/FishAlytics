/**
 * Species Maps
 * 
 * Within our catches we record the species caught.
 * 
 */
function(doc) {
	//!code views/lib/catch.js
	
	$catches =catchUtils.asCatches(doc);
	for($c in $catches){
		$key = $catches[$c];
		$key = $key.val;
		$key = JSON.parse(JSON.stringify($key));
		$key = [
			$key.fish.species,
			$key.coords[0],
			$key.coords[1],
			$key.timestamp
		];
		
		$val = JSON.parse(JSON.stringify($catches[$c].val));
		delete $val.licenses;
		delete $val.status;
		delete $val.rig;
		delete $val.stats.lines;
		delete $val.stats.span;
		
		emit($key,$val); 
	}
	
}
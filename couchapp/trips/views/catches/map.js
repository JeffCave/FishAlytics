/**
 * Catches
 */
function(doc) {
	//!code views/lib/catch.js
	$catches =catchUtils.asCatches(doc); 
	for($c in $catches){
		emit($catches[$c].key,$catches[$c].val); 
	}
};

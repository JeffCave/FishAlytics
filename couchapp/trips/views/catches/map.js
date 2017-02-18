/* global catchUtils */
/* global emit */
/**
 * Catches
 */
function catches(doc) {
	//!code views/lib/catch.js
	var $catches =catchUtils.asCatches(doc); 
	for(var $c in $catches){
		emit($catches[$c].key,$catches[$c].val); 
	}
}

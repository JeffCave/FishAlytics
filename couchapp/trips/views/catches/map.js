/* global catchUtils */
/* global emit */
/**
 * Catches
 */
function catches (doc) {
	var catchUtils = require('lib/catch');
	var $catches =catchUtils.asCatches(doc); 
	for(var $c in $catches){
		emit($catches[$c].key,$catches[$c].val); 
	}
}

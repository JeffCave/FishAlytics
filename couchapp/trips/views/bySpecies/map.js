/* global emit */
// jshint -W025
/**
 * Species Maps
 * 
 * Within our catches we record the species caught.
 * 
 */
function(doc) {
	'use strict';
	
	var catchUtils = require('lib/catch');
	
	var $catches =catchUtils.asCatches(doc);
	for(var $c in $catches){
		var $key = $catches[$c];
		$key = $key.val;
		$key = JSON.parse(JSON.stringify($key));
		$key = [
			$key.fish.species,
			$key.coords[0],
			$key.coords[1],
			$key.timestamp
		];
		
		var $val = JSON.parse(JSON.stringify($catches[$c].val));
		delete $val.licenses;
		delete $val.status;
		delete $val.rig;
		delete $val.stats.lines;
		delete $val.stats.span;
		
		emit($key,$val); 
	}
	
}

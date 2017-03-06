/**
 * Validations for Trip documents
 */ 
function /*validate_doc_update*/(newDoc, oldDoc, userCtx, secObj) { //jshint ignore:line
	if('trip' != newDoc._id.split('.')[0].toLowerCase()){
		return;
	}
	
	//*******************************************************************
	//* Helpers
	var Validators = {
		'waypoint' : function(waypoint){
			//must be a valid date
			var $val = Date.parse(waypoint.timestamp);
			if(!$val){
				throw({ forbidden : 'Waypoint specifies an invalid timestamp: ' + waypoint.timestamp });
			}
			//location is optional
			if(waypoint.coords){
				if(!Array.isArray(waypoint.coords)){
					throw({ forbidden : 'Waypoint is an invalid format' });
				}
				//if you do specify a location, we need at least LAT and LONG
				$val = waypoint.coords[0];
				if(isNaN($val)){
					throw({ forbidden : 'Waypoint specifies an invalid longitude: ' + $val });
				}
				if(Math.abs($val) > 180){
					throw({ forbidden : 'Waypoint specifies an invalid longitude: ' + $val });
				}
				$val = parseFloat(waypoint.coords[1]);
				if(isNaN($val)){
					throw({ forbidden : 'Waypoint specifies an invalid latitude: ' + $val });
				}
				if(Math.abs($val) > 90){
					throw({ forbidden : 'Waypoint specifies an invalid latitude: ' + $val });
				}
			}
		},
		'catch' : function(ctch){
			//catches are just spectial types of waypoint
			try{
				Validators.waypoint(ctch);
			}
			catch(e){
				e.forbidden = e.forbidden.replace(/Waypoint/g,'Catch');
				throw(e);
			}
		}
	};
	
	//*******************************************************************
	//* Required fields
	var msg = "All trips require a '{field}' field ";
	['fisherman','catches','waypoints'].forEach(function(field){
			if(!newDoc[field]){
				throw({ forbidden : msg.replace('{field}',field) });
			}
		});
	
	
	//*******************************************************************
	//* Fisherman must be constrained to known users
	
	
	//*******************************************************************
	//* Catches must be valid definition
	if(!Array.isArray(newDoc.catches)){
		throw({ forbidden : 'catches must be an array' });
	}
	for(var c=newDoc.catches.length-1; c>=0; c--){
		var fish = newDoc.catches[c];
		Validators['catch'](fish);
	}
	
	// *******************************************************************
	// * Waypoints must be valid definition
	if(!Array.isArray(newDoc.waypoints)){
		throw({ forbidden : 'Waypoints must be an array' });
	}
	if(newDoc.waypoints.length < 2){
		throw({ forbidden : 'A trip must contain at least a start and end time' });
	}
	for(var w=newDoc.waypoints.length-1; w>=0; w--){
		var waypoint = newDoc.waypoints[w];
		Validators.waypoint(waypoint);
	}
	
	// *******************************************************************
	// * License is optional
	// *  - FK: but must be an existing one
	// *  - Array data type
	if(newDoc.licenses){
		if(!Array.isArray(newDoc.licenses)){
			throw({ forbidden : 'Licenses must be an array' });
		}
//		var db = require('/_utils/script/jquery.couch.js') //('http://localhost:5984/fish');
//		db.view('licenses', 'licenses',{ keys: newDoc.licenses }, function(err, body) {
//			if (!err) {
//				if(!body.total_rows){
//					throw({forbidden : 'Licenses must exist'});
//				}
//			}
//		});
	}
}

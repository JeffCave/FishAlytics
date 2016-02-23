/**
 * Validations for Trip documents
 */ 
function(newDoc, oldDoc, userCtx, secObj) {
	if('trip' != newDoc._id.split('.')[0].toLowerCase()){
		return;
	}
	
	
	//*******************************************************************
	//* Helpers
	var Validators = {
		'waypoint' : function(waypoint){
			//must be a valid date
			$val = Date.parse(waypoint.timestamp);
			if(!$val){
				throw({ forbidden : 'Waypoint specifies an invalid timestamp: ' + waypoint.timestamp });
			}
			//location is optional
			if(waypoint.coords){
				//if you do specify a location, we need at least LAT and LONG
				$val = parseFloat(waypoint.coords.longitude);
				if(isNaN($val)){
					throw({ forbidden : 'Waypoint specifies an invalid longitude: ' + $val });
				}
				if($val < -180 || $val > 180){
					throw({ forbidden : 'Waypoint specifies an invalid longitude: ' + $val });
				}
				$val = parseFloat(waypoint.coords.latitude);
				if(isNaN($val)){
					throw({ forbidden : 'Waypoint specifies an invalid latitude: ' + $val });
				}
				if($val < -90 || $val > 90){
					throw({ forbidden : 'Waypoint specifies an invalid latitude: ' + $val });
				}
			}
		},
		'catch' : function(ctch){
			//timestamp is optional on a catch
			if(ctch.timestamp){
 				//must be a valid date
 				$val = Date.parse(ctch.timestamp);
 				if(!$val){
 					throw({ forbidden : 'Catch specifies and invalid timestamp: ' + $val });
 				}
			}
			//location is optional
			if(ctch.coords){
				$msg = "Catch locations require a '{field}' field ";
				['longitude','latitude'].forEach(function(field){
						if(!ctch.coords[field]){
							throw({ forbidden : $msg.replace('{field}',field) });
						}
					});
				//if you do specify a location, we need at least LAT and LONG
				$val = parseFloat(ctch.coords.longitude);
				if(isNaN($val)){
					throw({ forbidden : 'Catch specifies an invalid longitude: ' + $val });
				}
				if($val < -180 || $val > 180){
					throw({ forbidden : 'Catch specifies an invalid longitude: ' + $val });
				}
				$val = parseFloat(ctch.coords.latitude);
				if(isNaN($val)){
					throw({ forbidden : 'Catch specifies an invalid latitude: ' + $val });
				}
				if($val < -90 || $val > 90){
					throw({ forbidden : 'Catch [' + c + '] specifies an invalid latitude: ' + $val });
				}
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
}

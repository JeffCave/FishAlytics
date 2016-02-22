/**
 * Validations for Licenses
 */ 
function(newDoc, oldDoc, usserCtx, secObj) {
	if('lic' != newDoc._id.split('.')[0].toLowerCase()){
		return;
	}
	
	//*******************************************************************
	//* Helpers
	var Validators = {
	};
	
	//*******************************************************************
	//* Required fields
	$msg = "All licenses require a '{field}' field ";
	[	'fisherman'
		,'authority'
		,'type'
		,'license'
		,'region'
		,'licensee'
		,'issued'
		,'valid'
		,'issued'
	].forEach(function(field){
			if(!newDoc[field]){
				throw({ forbidden : $msg.replace('{field}',field) });
			}
		});
	
	
	// *******************************************************************
	// * Fisherman must be constrained to known users
	
	
	// *******************************************************************
	// * Issuance has some constraints
	//must contain a valid date
	$val = Date.parse(newDoc.issued.timestamp);
	if(!$val){
		throw({ forbidden : '"Issued" specifies an invalid timestamp: ' + newDoc.issued.timestamp });
	}
	
	// *******************************************************************
	// * Valid is must be a valid time range
	$val = Date.parse(newDoc.valid.start);
	if(!$val){
		throw({ forbidden : '"Valid" specifies an invalid start: ' + newDoc.valid.start });
	}
	$val = Date.parse(newDoc.valid.finish);
	if(!$val){
		throw({ forbidden : '"Valid" specifies an invalid finish: ' + newDoc.valid.finish });
	}
	
}

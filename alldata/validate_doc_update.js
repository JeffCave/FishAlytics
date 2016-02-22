/**
 * Validation rules for *all* objects in the Fishing database
 * <p>
 * 
 * </p>
 * <ul>
 *  <li>All entities MUST have a meta data field</li>
 *  <li>meta-data will have a 'created' field of type Date</li>
 *  <li>meta-data will have a 'modified' field of type Date</li>
 *  <li>meta-data will have a 'by' field to indicate the author</li>
 *  <li>the 'by' field will be constrained to known users</li>
 * </ul>
 * 
 */
function(newDoc, oldDoc, userCtx, secObj) {
	var isChanged = (function(oldDoc,newDoc){
			var o = JSON.parse(JSON.stringify(oldDoc));
			var n = JSON.parse(JSON.stringify(newDoc));
			o._rev = "";
			o._revisions = "";
			n._rev = "";
			n._revisions = "";
			o = JSON.stringify(o);
			n = JSON.stringify(n);
			
			return (n !== o);
		})(oldDoc,newDoc);
	if(!isChanged){
		throw({forbidden : "Entity is unchanged. Nothing to do."});
	}
	
	var isAdmin = (function(userCtx, secObj){
			isadmin = (userCtx.roles.indexOf('_admin') !== -1)
				|| (
					secObj && secObj.admins && secObj.admins.names 
					&& secObj.admins.names.indexOf(userCtx.name) !== -1
				);
			// see if the user a database admin specified by role
			if (!isadmin && secObj && secObj.admins && secObj.admins.roles){
				var db_roles = secObj.admins.roles;
				for(var idx = 0; idx < userCtx.roles.length; idx++) {
					var user_role = userCtx.roles[idx];
					if(db_roles.indexOf(user_role) !== -1) {
						return true;
					}
				}
			}
			return isadmin;
		})(userCtx, secObj)
		;
	
	//META-DATA enforcement
	if(!newDoc.meta){
		throw({forbidden : "Entities are expected to maintain meta data"});
	}
	var meta = newDoc.meta;
	var msg = "META-DATA: requires '{field}' field ";
	['created','modified','by'].forEach(function(field){
			if(!meta[field]){
				throw({ forbidden : msg.replace('{field}',field) });
			}
		});
	$val = Date.parse(meta.created);
	if(isNaN($val)){
		throw({forbidden : "Creation date must be a date"});
	}
	if($val > Date.now()){
		throw({forbidden : "Creation date cannot be in the future"});
	}
	$val = Date.parse(meta.modified);
	if(Date.parse(meta.modified) > Date.now()){
		throw({forbidden : "Modified date cannot be in the future"});
	}
//	if(!allusers.contains(meta.by)){
//		throw({forbidden : "FK: data modified by unknown user"});
//	}
	
	// UPDATE Enforcement
	if(oldDoc){
		if (oldDoc.meta){
			//META DATA
			if(oldDoc.meta.created && toJSON(oldDoc.meta.created) != toJSON(newDoc.meta.created)){
				throw({ forbidden : "May not change creation timestamp" });
			}
		}
	}
}

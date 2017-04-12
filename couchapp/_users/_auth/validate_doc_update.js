
function(newDoc, oldDoc, userCtx, secObj) {
	var isAdmin = (function(userCtx, secObj) {
		// see if the user is a server admin
		if(userCtx.roles.indexOf('_admin') !== -1) {
			return true; // a server admin
		}
		
		// see if the user a database admin specified by name
		if(secObj && secObj.admins && secObj.admins.names) {
			if(secObj.admins.names.indexOf(userCtx.name) !== -1) {
				return true; // database admin
			}
		}
		
		// see if the user a database admin specified by role
		if(secObj && secObj.admins && secObj.admins.roles) {
			var db_roles = secObj.admins.roles;
			userCtx.roles.forEach(function(role){
				if(db_roles.indexOf(role) !== -1) {
					return true; // role matches!
				}
			});
		}
		
		return false; // default to no admin
	})(userCtx,secObj);
	
	if (newDoc._deleted === true) {
		// allow deletes by admins and matching users
		// without checking the other fields
		if (!isAdmin && !(userCtx.name == oldDoc.name)) {
			throw({forbidden: 'Only admins may delete other user docs.'});
		}
		return;
	}
	
	
	// we only allow user docs for now
	var isValidType = (newDoc._id.substring(0,5) === 'auth.' || newDoc.type === 'user');
	if (!isValidType) {
		throw({forbidden : 'doc.type must be user'});
	}
	
	// Only validate the 'user' type other validate docs should update their domain
	if(newDoc.type !== 'user'){
		return;
	}
	
	// required fields
	['name','roles'].forEach(function(field){
			if (!newDoc[field]) {
				throw({forbidden: 'doc.'+field+' is required'});
			}
		});
	
	if (!isArray(newDoc.roles)) {
		throw({forbidden: 'doc.roles must be an array'});
	}
	
	newDoc.roles.forEach(function(role){
		if (typeof role !== 'string') {
			throw({forbidden: 'doc.roles can only contain strings'});
		}
	});
	
	if (newDoc._id !== ('org.couchdb.user:' + newDoc.name)) {
		throw({forbidden: 'Doc ID must be of the form org.couchdb.user:name'});
	}
	
	if (oldDoc) { // validate all updates
		if (oldDoc.name !== newDoc.name) {
			throw({forbidden: 'Usernames can not be changed.'});
		}
	}
	
	if (newDoc.password_sha && !newDoc.salt) {
		throw({forbidden: 'Users with password_sha must have a salt.' +
			'See /_utils/script/couch.js for example code.'
		});
	}
	
	if (newDoc.password_scheme === "pbkdf2") {
		if (typeof(newDoc.iterations) !== "number") {
			throw({forbidden: "iterations must be a number."});
		}
		if (typeof(newDoc.derived_key) !== "string") {
			throw({forbidden: "derived_key must be a string."});
		}
	}
	
	
	if (!isAdmin) {
		if (oldDoc) { // validate non-admin updates
			if (userCtx.name !== newDoc.name) {
				throw({
					forbidden: 'You may only update your own user document.'
				});
			}
			// validate role updates
			var oldRoles = oldDoc.roles.sort();
			var newRoles = newDoc.roles.sort();
			
			if (oldRoles.length !== newRoles.length) {
				throw({forbidden: 'Only _admin may edit roles'});
			}
			
			for (var i = 0; i < oldRoles.length; i++) {
				if (oldRoles[i] !== newRoles[i]) {
					throw({forbidden: 'Only _admin may edit roles'});
				}
			}
		} else if (newDoc.roles.length > 0) {
			throw({forbidden: 'Only _admin may set roles'});
		}
	}
	
	// no system roles in users db
	newDoc.roles.forEach(function(role){
		if (role[0] === '_') {
			throw({
				forbidden: 'No system roles (starting with underscore) in users db.'
			});
		}
	});
	
	// no system names as names
	if (newDoc.name[0] === '_') {
		throw({forbidden: 'Username may not start with underscore.'});
	}
	
	var badUserNameChars = [':'];
	badUserNameChars.forEach(function(c){
		if (newDoc.name.indexOf(c) >= 0) {
			throw({forbidden: 'Character `' + c +'` is not allowed in usernames.'});
		}
	});
}

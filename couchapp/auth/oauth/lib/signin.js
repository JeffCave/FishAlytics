// File lib/user/signin.js

/* global log */
module.exports.isAllowed = function(user,req){
	return module.exports.checkAllowed(user,req,true).length > 0;
}
module.exports.checkAllowed = function(user, req, failFast){
	failFast = !(failFast === false);
	
	const tests = [
		{error:400,msg:"Not granted on this site",assert:function(){return (req.query && user.roles.indexOf(req.query.site)>=0)}},
		{error:401,msg:"Incorrect password",assert:function(){
			const pbkdf2 = require("lib/crypto-js/pbkdf2");
			var hash = pbkdf2.pbkdf2(req.forms.password, user.salt, { keySize: 256/32, iterations: doc.iterations }).toString().substring(0,40);
			return (hash===user.derived_key);
		}},
	];
	
	
	//ES6: should use "any"/"some"
	var failures = tests.map(function(t){
		if(!t.assert()){
			delete t.assert;
			return t;
		}
	});
	
	return failures;
	
}

/**
 * https://runkit.com/jeffcave/59026587a4033a0012945870#
 * 
 */
module.exports.signin = function(user,req,secret){
	const hmac = require("lib/crypto-js/hmac-sha1");
	const base64 = require("lib/base64");
	var resp = {
			headers : {
				"Content-Type" : "application/json",
				"Set-Cookie" : "AuthSession={{sesscookie}}; Version=1; Path=/; HttpOnly"
			},
			body : { ok:true, msg:"successfully authenticated"},
		};
	
	var fail = function(reason,code){
			delete resp.headers['Set-Cookie'];
			resp.body.ok = false;
			resp.body.msg = reason;
			if(code) resp.code = code;
			return resp;
		};
	// check that we have all the pre-requisites
	if (!user || !req) return fail("Invalid document or request objects.",500);
	if (user.type !== "user") return fail("Invalid document type ("+user.type+")",500);
	if (!(user.salt && user.derived_key && user.roles)) return fail("salt, derived_key or roles not found",400);
	if (secret === '{**SECRETKEY**}') return fail("secret key is still a placeholder value ({**SECRETKEY**})",500);
	
	// check that the person is allowed
	//if(var failures = checkAllowed(doc,req,true)) return fail(failures.msg,failures.error);
	
	// who it is, and when it was created
	var sessdata = [user.name,new Date()];
	sessdata[1] = Math.floor(sessdata[1].getTime()/1000).toString(16).toUpperCase();
	sessdata.push(sessdata.join(':'));
	// configsecret from config.httpd_auth.secret
	var configsecret = secret+user.salt;
	
	/** Never expose this
	resp.headers.secret = secret;
	resp.headers.salt = user.salt;
	resp.headers.data = sessdata[2];
	*/
	
	
	var hash = hmac(sessdata[2],configsecret);
	sessdata[2] = hash.words
		.map(function(v){
				var bytes = [0,0,0,0]
					.slice(0,Math.min(4,hash.sigBytes))
					.map(function(d,i){ return (v >>> (8*i)) % 256; })
					.reverse()
					;
				hash.sigBytes -= bytes.length;
				return bytes;
			})
		.reduce(function(a,d){ return a.concat(d);},[])
		.map(function(d){return String.fromCharCode(d);})
		.join('')
		;
	sessdata = sessdata.join(':');
	
	sessdata = base64
		.btoa(sessdata)
		.replace(/\//g,'_')
		.replace(/\+/g,'-')
		.replace(/=/g,'')
		;

	resp.headers['Set-Cookie'] = resp.headers['Set-Cookie']
		.replace(/{{sesscookie}}/g,sessdata)
		;
	return resp;
};

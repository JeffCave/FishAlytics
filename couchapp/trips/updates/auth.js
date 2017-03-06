/**
 * Authentication Handler
 * 
 */
function /*updatesAuth*/(doc,req){ // jshint ignore:line
	var base64 = require("lib/base64");
	var utils = require("lib/utils");
	//generally useful calculations
	var token = {email:'',sub:'',email_verified:''};
	//var origDoc = JSON.parse(JSON.stringify(doc));
	var timestamp = Date.now();
	//var localAuth = "Basic " + base64.btoa("triggerjob:triggerjob");
	var localAuth = "Basic dHJpZ2dlcmpvYjp0cmlnZ2Vyam9i";
	
	
	
	if(!doc){
		doc = {
			_id : "auth." + req.uuid
			,phase:-1
			,meta:{
				created:timestamp
				,modified:timestamp
			}
			,BaseUrl : utils.getBaseUrl(req)
			,expires:timestamp + 1000*60
			//,triggers:{
			//	//expire:{
			//	//	path:"auth." + req.uuid
			//	//	,method:"DELETE"
			//	//	,start:1000*60
			//	//}
			//}
		};
	}
	
	
	// copy any variables from the post, or 
	// query string into the document
	var $a = [req.form,req.query];
	for(var $b=$a.length-1; $b>=0; $b--){
		for(var s in $a[$b]){
			if(s.substr(0,1) === '_' || s === 'state'){
				continue;
			}
			if(s === 'triggers' || s === 'triggered'){
				continue;
			}
			doc[s] = doc[s] || $a[$b][s];
		}
	}
	// set calculated values
	doc.meta.modified = timestamp;
	
	var phases = {
		error:{
			phase:-1
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				code:504
				,headers : {
					"Status" : "504"
				}
				,body : "<html><body>Did not receive a timely response from authentication party.</body></html>"
			}
		}
		,begin : {
			phase:0
			,isphase:function(doc,req){
				return false !== (doc.code || false);
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : this.templates.login
			}
		}
		,redirectToAuthSource:{
			phase:1
			,isphase:function(doc,req){
				return false !== (doc.code || false);
			}
			,resp:{
				code:303
				,headers : {
					"Status" : "303"
					,"Location" : [""
						,"https://accounts.google.com/o/oauth2/v2/auth?"
						,"&response_type=code" 
						,"&client_id=239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com"
						,"&scope=email"
						,"&state="+encodeURIComponent(JSON.stringify(doc))+""
						,"&redirect_uri=" + encodeURIComponent(doc.BaseUrl + "/auth")
						,"&include_granted_scopes=true"
						,"&nonce=" + encodeURIComponent(doc._id)
						].join('')
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<p class='pagecenter'>Attempting to redirect to google auth</p>"
						//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
						//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
						,"</body></html>"
					].join('\n')
			}
		}
		,waitOnAuthSource: {
			phase:2
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<p class='pagecenter'>Verifying with {{{authsource}}}...</p>"
						,"<form method='POST' action='{{{BaseUrl}}}/auth/{{{_id}}}'>"
						//,"<input type='submit' value='waitmore' />"
						,"</form>"
						,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
						//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		}
		,checkuser:{
			phase:3
			,isphase:function(doc,req){
				return doc.triggered && doc.triggered.verify;
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<p class='pagecenter'>Verified with {{{authsource}}}.</p>"
						,"<form method='POST' action='./{{{_id}}}'>"
						//,"<input type='submit' value='waitmore' />"
						,"</form>"
						,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						//,"<table>"
						//," <tr><td>Email</td><td>"+token.email+"</td></tr>"
						//," <tr><td>Sub</td><td>"+token.sub+"</td></tr>"
						//," <tr><td>Verified</td><td>"+token.email_verified+"</tr>"
						//,"</table>"
						//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
						//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		}
		,setpass:{
			phase:4
			,isphase:function(doc,req){
				return doc.triggered && doc.triggered.checkuser;
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<p class='pagecenter'>Logging in...</p>"
						,"<form method='POST' action='./{{{_id}}}'>"
						//,"<input type='submit' value='waitmore' />"
						,"</form>"
						,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						//,"<table>"
						//," <tr><td>Email</td><td>"+token.email+"</td></tr>"
						//," <tr><td>Sub</td><td>"+token.sub+"</td></tr>"
						//," <tr><td>Verified</td><td>"+token.email_verified+"</tr>"
						//,"</table>"
						//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
						//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		}
		,dologin:{
			phase:5
			,isphase:function(doc,req){
				return doc.triggered && doc.triggered.setPass;
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<p class='pagecenter'>Logging in</p>"
						,"<form method='POST' action='/_session?next=/catchmap'>"
						//," <input type='hidden' name='next' value='{{{BaseUrl}}}' />"
						," <input type='hidden' name='name'     value='{{{id_token.email}}}' />"
						," <input type='hidden' name='password' value='{{{code}}}' />"
						//," <input type='submit' value='Try logging in ' />"
						,"</form>"
						,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1)</script>"
						//,"<table>"
						//," <tr><td>Email</td><td>"+token.email+"</td></tr>"
						//," <tr><td>Sub</td><td>"+token.sub+"</td></tr>"
						//," <tr><td>Verified</td><td>"+token.email_verified+"</tr>"
						//,"</table>"
						//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
						//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		}
	};
	
	var resp = [
		phases.begin.resp
		,phases.redirectToAuthSource.resp
		,phases.waitOnAuthSource.resp
		,phases.checkuser.resp
		,phases.setpass.resp
		,phases.dologin.resp
	];
	
	
	
	if(doc && doc.expires < timestamp){ 
		return [
			null //{"_id":doc._id,"_rev":doc._rev,"_deleted":true}
			,phases.error.resp
		];
	}
	else if(phases.dologin.isphase(doc,req)){
		doc.phase = phases.dologin.phase;
		doc.setpass = doc.triggered.setPass.code;
		doc.canDelete = true;
		resp = phases.dologin.resp;
		//if(doc.triggered.setPass.code !== 200){
		//	
		//}
		// TODO: username/password == doc.id_token.sub / doc.code
	}
	else if(phases.setpass.isphase(doc,req)){
		doc.phase = phases.setpass.phase;
		resp = phases.setpass.resp;
		doc.triggers = doc.triggers || {};
		doc.triggers.setPass = {
			path: doc.BaseUrl + "/_users/org.couchdb.user%3A" + encodeURIComponent(doc.id_token.email)
			,headers:{"Authorization":localAuth}
			,method:"PUT"
			,start:0
		};
		switch(doc.triggered.checkuser.code){
			case 200:
				doc.triggers.setPass.params = JSON.parse(doc.triggered.checkuser.out);
				break;
			//case 404:
			default:
				doc.triggers.setPass.params = {
					_id:"org.couchdb.user:" + doc.id_token.email
					,name:doc.id_token.email
					,roles:[]
					,type:"user"
				};
				break;
		}
		doc.triggers.setPass.params.password = doc.code;
	}
	else if(phases.checkuser.isphase(doc,req)){
		doc.phase = phases.checkuser.phase;
		resp = phases.checkuser.resp;
		doc.triggers = doc.triggers || {};
		if(doc.triggered.verify.code !== 200){
			//TODO
		}
		if(doc.triggered.verify.out){
			doc.id_token = doc.triggered.verify.out;
			doc.id_token = JSON.parse(doc.id_token);
			doc.id_token = doc.id_token.id_token.split('.')[1];
			doc.id_token = base64.atob(doc.id_token);
			doc.id_token = JSON.parse(doc.id_token);
		}
		doc.triggers.checkuser={
				path: doc.BaseUrl + "/_users/org.couchdb.user%3A" + encodeURIComponent(doc.id_token.email)
				,headers:{"Authorization":localAuth}
				,method:"GET"
				,storepositive:true
				,start:0
			};
	}
	else if(doc.triggers && doc.triggers.verify){
		//we are still waiting... nothignt really to do
		doc.phase = phases.waitOnAuthSource.phase;
		resp = phases.waitOnAuthSource.resp;
		doc.blockSave = true;
	}
	else if(doc.code){
		doc.phase = phases.waitOnAuthSource.phase;
		resp = phases.waitOnAuthSource.resp;
		doc.triggers = {verify:{
				path:"https://accounts.google.com/o/oauth2/token"
				,headers:{'content-type':'application/x-www-form-urlencoded'}
				,method:"POST"
				,asquery:false
				,storepositive:true
				,start:0
				,form:{
					code:doc.code
					,client_id:"239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com"
					,client_secret:"QyYKQRBx7HuKI-q11oJnkK-d"
					,redirect_uri: (doc.BaseUrl + "/auth")
					,grant_type:"authorization_code"
				}
			}};
	}
	else if(doc._rev){
		doc.phase = phases.redirectToAuthSource.phase;
		resp = phases.redirectToAuthSource.resp;
	}
	else{
		doc.phase = phases.begin.phase;
		resp = phases.begin.resp;
	}
	
	if(doc.id_token){
		token.email = doc.id_token.email;
		token.sub = doc.id_token.sub;
		token.email_verified = doc.id_token.email_verified;
	}
	
	
	var Mustache = require("lib/mustache");
	resp.body = Mustache.to_html(resp.body, doc, this.templates.partials);
	if(doc.blockSave){
		doc = null;
	}
	
	return [doc, resp];
}

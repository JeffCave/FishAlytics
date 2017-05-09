/**
 * Authentication Handler
 * 
 */
function (doc,req){ // jshint ignore:line
	var base64 = require("lib/base64");
	var utils = require("lib/utils");
	//generally useful calculations
	var token = {email:'',sub:'',email_verified:''};
	//var origDoc = JSON.parse(JSON.stringify(doc));
	var timestamp = Date.now();
	//var localAuth = "Basic " + base64.btoa("triggerjob:triggerjob");
	var localAuth = "Basic dHJpZ2dlcmpvYjp0cmlnZ2Vyam9i";
	var oauth={
		google:{
			client_id:"239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com",
			client_secret:"QyYKQRBx7HuKI-q11oJnkK-d",
		}
	};
	
	
	if(!doc){
		doc = {
			//debug : {},
			_id : "auth." + req.uuid
			,phase:0
			,meta:{
				created:timestamp,
				modified:timestamp,
			}
			,BaseUrl : utils.getBaseUrl(req)
			,expires:timestamp + 1000*60
			,triggers:{
				expire:{
					path:"auth." + req.uuid
					,method:"DELETE"
					,start:1000*60*60
				}
			}
		};
		if(doc.debug){
			delete doc.triggers.expire;
		}
	}
	
	
	// copy any variables from the post, or 
	// query string into the document
	[req.form,req.query].forEach(function(form){
		Object.keys(form).forEach(function(key){
			var isvalid = true;
			// ignore private variables
			if (key.substr(0,1) === '_') isvalid = false;
			// ignore various keys explicitly
			if(['state','triggers','triggered'].indexOf(key) >=0){
				 isvalid = false;
			}
			if(isvalid){
				doc[key] = form[key] || doc[key];
			}
		});
	});
	// set calculated values
	doc.meta.modified = timestamp;
	
	var phases = {
		timeout:{
			phase:408
			,isphase:function(doc,req){
				// if the document has expired, this phase can handle it
				return (doc && doc.expires < doc.meta.modified.timestamp);
			}
			,action: function(){
				//doc = {"_id":doc._id,"_rev":doc._rev,"_deleted":true};
				doc._deleted = true;
			}
			,resp:{
				code:504
				,headers : {
					"Status" : "504"
				}
				,body : [
					"<html>",
					"<head>",
					"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>",
					"</head>",
					"<body>",
					"<p class='pagecenter'>Did not receive a timely response from authentication party.</p>",
					"{#debug}",
					"<pre>{{debug.doc}}</pre><hr />",
					"<pre>{{debug.req}}</pre>",
					"{/debug}",
					"</body>",
					"</html>",
					''
					].join('\n')
			}
		}
		,begin : {
			phase:0
			,isphase:function(doc,req){
				// if the document expires in the future, this phase can handle it
				return (doc && doc.expires >= doc.meta.modified.timestamp);
			},
			action:function(){
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : this.lib.templates.login
			}
		}
		,redirectToAuthSource:{
			phase:1
			,isphase:function(doc,req){
				// if we have setup the auth session already, we can redirect
				// the user to google knowing there is something waiting when
				// the user gets back
				return false !== (doc._rev || false);
			},
			action: function(){
				doc.blockSave = true;
			}
			,resp:{
				code:303
				,headers : {
					"Status" : "303"
					,"Location" : [""
						,"https://accounts.google.com/o/oauth2/v2/auth?"
						,"&response_type=code" 
						,"&client_id="+oauth.google.client_id
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
						,"<a href='" + [""
							,"https://accounts.google.com/o/oauth2/v2/auth?"
							,"&response_type=code" 
							,"&client_id="+oauth.google.client_id
							,"&scope=email"
							,"&state="+encodeURIComponent(JSON.stringify(doc))+""
							,"&redirect_uri=" + encodeURIComponent(doc.BaseUrl + "/auth")
							,"&include_granted_scopes=true"
							,"&nonce=" + encodeURIComponent(doc._id)
							].join('') + "'>Click to get there...</a>"
						,"<p class='pagecenter'>Attempting to redirect to {{{authsource}}}</p>",
						"{#debug}",
						"<pre>{{debug.doc}}</pre><hr />",
						"<pre>{{debug.req}}</pre>",
						"</body></html>",
						"{/debug}"
					].join('\n')
			}
		}
		,confirmWithOAuthSource: {
			phase:2
			,isphase:function(doc,req){
				// This phase can handle it if the user has returned with a code
				// from google
				return false !== (doc.code || false);
			},
			action: function(){
				//doc = JSON.parse(req.form.state);
				if(req.form.state){
					doc.authsource = JSON.parse(req.form.state).authsource;
					doc.next = JSON.parse(req.form.state).next;
				}
				doc.triggers = doc.triggers || {};
				doc.triggers.verify = {
					path:"https://accounts.google.com/o/oauth2/token"
					,headers:{'content-type':'application/x-www-form-urlencoded'}
					,method:"POST"
					,asquery:false
					,storepositive:true
					,start:0
					,form:{
						code:''+doc.code,
						client_id:oauth.google.client_id,
						client_secret:oauth.google.client_secret,
						redirect_uri: (doc.BaseUrl + "/auth"),
						grant_type:"authorization_code",
					}
				};

			}
			,resp: {
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<form method='POST' action='{{{BaseUrl}}}/auth/{{{_id}}}'>"
						,"<p class='pagecenter'>Verifying with {{{authsource}}}...</p>"
						,"{{#debug}}<input type='submit' value='waitmore' />{{/debug}}"
						,"</form>",
						"{{^debug}}<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>{{/debug}}",
						"</body></html></html>"
					].join('\n')
			}
		}
		,waitOnAuthSource: {
			phase:3
			,isphase:function(doc,req){
				// This is a fairly generic phase. We are checking to see that 
				// there are triggers pending. This would mean that we have an 
				// outstanding request with a 3rd party and should just wait 
				// for them to respond. (ignore the expires poison pill)
				return doc.triggers && Object.keys(doc.triggers).length > 1;
			},
			action: function(){
				// do nothing, just suggest to the user to come back in a 
				// second to see if their request has been processed yet
				doc.blockSave = true;
			}
			,resp: {
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html>"
						,"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>"
						,"<body>"
						,"<form method='POST' action='{{{BaseUrl}}}/auth/{{{_id}}}'>"
						,"<p class='pagecenter'>Verifying with {{{authsource}}}....</p>",
						"{{#debug}}<input type='submit' value='waitmore' />{{/debug}}",
						"</form>",
						"{{^debug}}<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>{{/debug}}",
						"{{#debug}}",
						"<pre>{{doc}}</pre><hr />",
						"<pre>{{debug.req}}</pre>",
						"{{/debug}}",
						"</body></html></html>"
					].join('\n')
			}
		}
		,checkuser:{
			phase:4
			,isphase:function(doc,req){
				// if google responded with a verification package, this 
				// phase is applicable 
				return doc.triggered && doc.triggered.verify;
			},
			action: function(){
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
						//,headers:{"Authorization":localAuth}
						,method:"GET"
						,storepositive:true
						,start:0
					};
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [
						"<html>",
						"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>",
						"<body>",
						"<p class='pagecenter'>Verified with {{{authsource}}}.</p>",
						"<form method='POST' action='./{{{_id}}}'>",
						"{{#debug}}<input type='submit' value='waitmore' />{{/debug}}",
						"</form>",
						"{{^debug}}<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>{{/debug}}",
						"{{#debug}}",
						"<table>",
						" <tr><td>Email</td><td>{{doc.id_token.email}}</td></tr>",
						" <tr><td>Sub</td><td>{{doc.id_token.sub}}</td></tr>",
						" <tr><td>Verified</td><td>{{doc.id_token.email_verified}}</td></tr>",
						"</table>",
						"<pre>{{debug.doc}}</pre><hr />",
						"<pre>{{debug.req}}</pre>",
						"{{/debug}}",
						"</body></html></html>"
					].join('\n')
			}
		}
		,createuser:{
			phase:5
			,isphase:function(doc,req){
				// if we know that a verififed user is in our database, this 
				// phase is applicable
				return doc.triggered && doc.triggered.checkuser && 
					doc.triggered.checkuser.code == 404
					;
			},
			action: function(){
				doc.triggers = doc.triggers || {};
				doc.triggers.createUser = {
					path: doc.BaseUrl + "/_users/org.couchdb.user%3A" + encodeURIComponent(doc.id_token.email),
					//headers:{"Authorization":localAuth},
					method: "PUT",
					start: 0,
					storepositive:true,
					params: {
						_id: "org.couchdb.user:" + doc.id_token.email,
						name: doc.id_token.email,
						roles: [],
						type: "user",
						password: doc.code,
					},
				};
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [
						"<html>",
						"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>",
						"<body>",
						"<p class='pagecenter'>Verified with {{{authsource}}}.</p>",
						"<form method='POST' action='./{{{_id}}}'>",
						"{{#debug}}<input type='submit' value='waitmore' />{{/debug}}",
						"</form>",
						"{{^debug}}<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>{{/debug}}",
						"{{#debug}}",
						"<table>",
						" <tr><td>Email</td><td>{{doc.id_token.email}}</td></tr>",
						" <tr><td>Sub</td><td>{{doc.id_token.sub}}</td></tr>",
						" <tr><td>Verified</td><td>{{doc.id_token.email_verified}}</td></tr>",
						"</table>",
						"<hr />",
						"<pre>{{debug.doc}}</pre>",
						"<hr />",
						"<pre>{{debug.req}}</pre>",
						"{{/debug}}",
						"</body></html></html>"
					].join('\n')
			}
		}
		,getuser:{
			phase:6
			,isphase:function(doc,req){
				return (doc.triggered && doc.triggered.checkuser && doc.triggered.checkuser.code != 404) ||
					(doc.triggered && doc.triggered.createUser)
					;
			},
			action: function(){
				doc.triggers = doc.triggers || {};
				doc.triggers.getUser = {
					path: doc.BaseUrl + "/_users/org.couchdb.user%3A" + encodeURIComponent(doc.id_token.email),
					//headers:{"Authorization":localAuth},
					method: "GET",
					start: 0,
					storepositive:true,
				};
			}
			,resp:{
				headers : {
					"Content-Type" : "text/html"
				}
				,body : [
						"<html>",
						"<style>.pagecenter{display:block; position:absolute; top:33%; transform:translateY(-50%); left:50%; transform:translateX(-50%); }</style>",
						"<body>",
						"<p class='pagecenter'>Logging in...</p>",
						"<form method='POST' action='./{{{_id}}}'>",
						"{{#debug}}<input type='submit' value='waitmore' />{{/debug}}",
						"</form>",
						"{{^debug}}<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>{{/debug}}",
						"{{#debug}}",
						"<table>",
						" <tr><td>Email</td><td>{{doc.id_token.email}}</td></tr>",
						" <tr><td>Sub</td><td>{{doc.id_token.sub}}</td></tr>",
						" <tr><td>Verified</td><td>{{doc.id_token.email_verified}}</td></tr>",
						"</table>",
						"<hr />",
						"<pre>{{debug.doc}}</pre>",
						"<hr />",
						"<pre>{{debug.req}}</pre>",
						"{{/debug}}",
						"</body></html></html>"
					].join('\n')
			}
		}
		,dologin:{
			phase:7
			,isphase:function(doc,req){
				return (doc.triggered && doc.triggered.getUser);
			},
			action: function(){
			}
			,resp: function(){
				const signin = require('lib/signin').signin;
				var user = JSON.parse(doc.triggered.getUser.out);
				var cookie = signin(user,req,'WOUpQ37Vcfz4cV8rTewKGwypbnJ5UT');
				if(doc.next){
					cookie.code = 303;
					cookie.headers.Status = "303";
					cookie.headers.Location = doc.next;
				}
				return cookie;
			}
		}
	};
	
	
	doc.sortedPhases = '';
	var phase = null;
	// sort them in descending order. We want to test things closest to 
	// completion as possible first.
	var sortedPhases = Object
		.keys(phases)
		.map(function(key){return phases[key];})
		.sort(function(a,b){return b.phase - a.phase;})
		;
	// find the most relevant phase
	for(var p in sortedPhases){
		doc.sortedPhases += p + ',';
		phase = sortedPhases[p];
		if(phase.isphase(doc,req)){
			break;
		}
	}
	phase.action();
	doc.phase = phase.phase;
	
	if(typeof phase.resp === 'function') phase.resp = phase.resp();
	if(typeof phase.resp.body === 'object') phase.resp.body = JSON.stringify(phase.resp.body);
	
	if(doc.debug) [doc,req].forEach(function(d){
			phase.resp.body += "<pre>"+JSON.stringify(d,null,'\t')+"</pre><hr />";
		});
	
	var Mustache = require("lib/mustache");
	phase.resp.body = Mustache.to_html(phase.resp.body, doc);
	if(doc.blockSave){
		doc = null;
	}
	
	return [doc, phase.resp];
}

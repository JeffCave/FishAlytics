/**
 * Authentication Handler
 * 
 */
function(doc,req){
	var base64 = require("lib/base64");
	//generally useful calculations
	var token = {email:'',sub:'',email_verified:''};
	var origDoc = JSON.parse(JSON.stringify(doc));
	var timestamp = Date.now();
	//var localAuth = "Basic " + base64.btoa("triggerjob:triggerjob");
	var localAuth = "Basic dHJpZ2dlcmpvYjp0cmlnZ2Vyam9i";
	if(doc && doc.expires < timestamp) doc = null;
	
	
	
	if(!doc){
		doc = {
			_id : "auth." + req.uuid
			,phase:-1
			,meta:{
				created:timestamp
				,modified:timestamp
			}
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
	
	// determine which phase we are in
	//doc.phase++;
	
/*	
	var phases = {
		error{
			phase:-1
			,isphase:function(doc,req){
				return false;
			}
			,resp:{code:500}
		}
		,begin : {
			phase:0
			,isphase:function(doc,req){
				return false !== (doc.code || false);
			}
			,resp:{
				headers : {"Content-Type" : "text/html"}
				,body : [""
						,"<html>"
						," <body>"
						,"  <form method='POST' action='/fishdev/_design/trips/_rewrite/auth/{{id}}'>"
						,"   Login with: "
						,"   <ul>"
						,"    <li><input type='submit' name='authsource' value='Google' /></li>"
						,"   </ul>"
						,"  </form>"
						," </body>"
						,"</html>"
					].join('\n')
			}
		},
		redirectToAuthSource:{
			phase:1
			,isphase:function(doc,req){
				return false !== (doc.code || false);
			}
			,resp:{
				code:303
				,headers : {
					"Status" : "303"
					,"Location" : 
						"https://accounts.google.com/o/oauth2/v2/auth?"
						+ "&response_type=code" 
						+ "&client_id=239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com"
						+ "&scope=email"
						+ "&state={{encdoc}}"
						//+ "&redirect_uri=https%3A%2F%2Ffishalytics.smileupps.com%2Fauth"
						+ "&redirect_uri=http%3A%2F%2Flvh.me:5984%2Ffishdev%2F_design%2Ftrips%2F_rewrite%2Fauth"
						+ "&include_granted_scopes=true"
						+ "&nonce={{id}}"
				}
				,body : [""
						,"<html><body>"
						,"<p>Attempting to redirect to google auth</p>"
						,"<pre>{{dispDoc}}</pre><hr />"
						,"<pre>{{dispRec}}</pre>"
						,"</body></html>"
					].join('\n')
			}
		},
		waitOnAuthSource: {
			phase:2
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				code:200
				,headers : {"Content-Type" : "text/html"}
				,body : [""
						,"<html><body>"
						,"Verifying with {{authsource}}...<br />"
						,"<form method='POST' action='./"+doc._id+"'>"
						,"<input type='submit' value='waitmore' />"
						//,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						,"</form>"
						,"<hr /><pre>{{dispDoc}}</pre>"
						,"<hr /><pre>{{dispRec}}</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		},
		checkuser:{
			phase:3
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				,headers : {"Content-Type" : "text/html"}
				,body : [""
						,"<html><body>"
						,"<p>Verified with {{authsource}}.</p>"
						,"<p>Checking local...</p>"
						,"<form method='POST' action='./{{id}}'>"
						,"<input type='submit' value='waitmore' />"
						,"</form>"
						//,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						,"<table>"
						," <tr><td>Email</td><td>"+token.email+"</td></tr>"
						," <tr><td>Sub</td><td>"+token.sub+"</td></tr>"
						," <tr><td>Verified</td><td>"+token.email_verified+"</tr>"
						,"</table>"
						,"<hr /><pre>{{dispDoc}}</pre>"
						,"<hr /><pre>{{dispRec}}</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		},
		setpass:{
			phase:4
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				,headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html><body>"
						,"<p>Updating password...</p>"
						,"<form method='POST' action='./{{doc._id}}'>"
						,"<input type='submit' value='waitmore' />"
						,"</form>"
						//,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						,"<table>"
						," <tr><td>Email</td><td>{{id_token.email}}</td></tr>"
						," <tr><td>Sub</td><td>{{id_token.sub}}</td></tr>"
						," <tr><td>Verified</td><td>{{id_token.email_verified}}</tr>"
						,"</table>"
						,"<hr /><pre>{{dispDoc}}</pre>"
						,"<hr /><pre>{{dispRec}}</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		},
		dologin:{
			phase:5
			,isphase:function(doc,req){
				return false;
			}
			,resp:{
				,headers : {
					"Content-Type" : "text/html"
				}
				,body : [""
						,"<html><body>"
						,"<form method='POST' action='/_session?next="+encodeURIComponent("http://lvh.me:5984/fishdev/_design/trips/_rewrite/")+"'>"
						,"<input type='submit' value='Try logging in ' />"
						//,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
						,"</form>"
						,"<p>Finalized, user should get redirected to _session</p>"
						,"<table>"
						," <tr><td>Email</td><td>{{id_token.email}}</td></tr>"
						," <tr><td>Sub</td><td>{{id_token.sub}}</td></tr>"
						," <tr><td>Verified</td><td>{{id_token.email_verified}}</tr>"
						,"</table>"
						,"<hr /><pre>{{dispDoc}}</pre>"
						,"<hr /><pre>{{dispRec}}</pre>"
						,"</body></html></html>"
					].join('\n')
			}
		}
	};
*/	
	
	if(doc.triggered && doc.triggered.setPass){
		doc.phase = 5;
		doc.setpass = doc.triggered.setPass.code;
		//if(doc.triggered.setPass.code !== 200){
		//	
		//}
		// TODO: username/password == doc.id_token.sub / doc.code
	}
	else if(doc.triggered && doc.triggered.checkuser){
		doc.phase = 4;
		doc.triggers = doc.triggers || {};
		doc.triggers.setPass = {
			path:"http://127.0.0.1:5984/_users/org.couchdb.user%3A" + doc.id_token.sub
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
					_id:"org.couchdb.user:" + doc.id_token.sub
					,name:doc.id_token.sub
					,roles:[]
					,type:"user"
				};
				break;
		}
		doc.triggers.setPass.params.password = doc.code;
	}
	else if(doc.triggered && doc.triggered.verify){
		doc.phase = 3;
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
				path:"http://127.0.0.1:5984/_users/org.couchdb.user%3A" + doc.id_token.sub
				,headers:{"Authorization":localAuth}
				,method:"GET"
				,storepositive:true
				,start:0
			};
	}
	else if(doc.triggers && doc.triggers.verify){
		//we are still waiting... nothignt really to do
		doc.phase = 2;
		doc.blockSave = true;
	}
	else if(doc.code){
		doc.phase = 2;
		doc.triggers = {verify:{
				path:"https://accounts.google.com/o/oauth2/token"
				,headers:{'content-type':'application/x-www-form-urlencoded'}
				,method:"POST"
				,asquery:true
				,storepositive:true
				,start:0
				,params:{
					code:doc.code
					,client_id:"239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com"
					,client_secret:"QyYKQRBx7HuKI-q11oJnkK-d"
					,redirect_uri:"http://lvh.me:5984/fishdev/_design/trips/_rewrite/auth"
					,grant_type:"authorization_code"
				}
			}};
	}
	else if(doc._rev){
		doc.phase = 1;
	}
	else{
		doc.phase = 0;
	}
	
	var encDoc = JSON.stringify(doc);
	encDoc = encodeURIComponent(encDoc);
	if(doc.id_token){
		token.email = doc.id_token.email;
		token.sub = doc.id_token.sub;
		token.email_verified = doc.id_token.email_verified;
	}
	
	var resp = [
		{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [ ""
					,"<html>"
					," <body>"
					,"  <form method='POST' action='/fishdev/_design/trips/_rewrite/auth/"+doc._id+"'>"
					,"   Login with: "
					,"   <ul>"
					,"    <li><input type='submit' name='authsource' value='Google' /></li>"
					,"   </ul>"
					,"  </form>"
					," </body>"
					,"</html>"
				].join('\n')
		}
		,{
			code:303
			,headers : {
				"Status" : "303"
				,"Location" : 
					"https://accounts.google.com/o/oauth2/v2/auth?"
					+ "&response_type=code" 
					+ "&client_id=239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com"
					+ "&scope=email"
					+ "&state="+encDoc+""
					//+ "&redirect_uri=https%3A%2F%2Ffishalytics.smileupps.com%2Fauth"
					+ "&redirect_uri=http%3A%2F%2Flvh.me:5984%2Ffishdev%2F_design%2Ftrips%2F_rewrite%2Fauth"
					+ "&include_granted_scopes=true"
					+ "&nonce=" + doc._id
			}
			,body : [""
					,"<html><body>"
					,"<p>Attempting to redirect to google auth</p>"
					,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
					,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
					,"</body></html>"
				].join('\n')
		}
		,{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [""
					,"<html><body>"
					,"Verifying with "+doc.authsource+"...<br />"
					,"<form method='POST' action='./"+doc._id+"'>"
					//,"<input type='submit' value='waitmore' />"
					,"</form>"
					,"<script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
					//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
					//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
					,"</body></html></html>"
				].join('\n')
		}
		,{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [""
					,"<html><body>"
					,"<p>Verified with "+doc.authsource+".</p>"
					,"<p>Checking local...</p>"
					,"<form method='POST' action='./"+doc._id+"'>"
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
		,{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [""
					,"<html><body>"
					,"<p>Updating password...</p>"
					,"<form method='POST' action='./"+doc._id+"'>"
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
		,{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [""
					,"<html><body>"
					,"<p>Finalized, user should get redirected to _session</p>"
					,"<form method='POST' action='/_session?next=/fishdev/_design/trips/_rewrite/'>"
					," <input type='hidden' name='next'     value='"+encodeURIComponent("http://lvh.me:5984/fishdev/_design/trips/_rewrite/")+"' />"
					," <input type='hidden' name='name'     value='"+token.sub+"' />"
					," <input type='hidden' name='password' value='"+doc.code+"' />"
					//," <input type='submit' value='Try logging in ' />"
					,"</form>"
					," <script>setTimeout(function(){document.getElementsByTagName('form')[0].submit();},1000)</script>"
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
	];
	resp = resp[doc.phase];
	if(doc.blockSave){
		doc = null;
	}
	
	return [doc, resp];
}

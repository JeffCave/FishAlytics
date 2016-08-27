/**
 * Authentication Handler
 * 
 */
function(doc,req){
	//!code lib/base64.js
	
	
	var token = {email:'',sub:'',email_verified:''};
	var origDoc = JSON.parse(JSON.stringify(doc));
	var timestamp = Date.now();
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
		};
	}
	var $a = [req.form,req.query];
	for(var $b=$a.length-1; $b>=0; $b--){
		for(var s in $a[$b]){
			if(s.substr(0,1) === '_' || s === 'state'){
				continue;
			}
			if(s === 'trigger'){
				continue;
			}
			doc[s] = doc[s] || $a[$b][s];
		}
	}
	doc.meta.modified = timestamp;
	doc.phase++;
	
	if(doc.triggered){
		doc.blockSave = true;
		if(doc.triggered.verify.code !== 200){
			
		}
		if(doc.triggered.verify.out){
			doc.id_token = JSON.parse(atob(
					JSON.parse(doc.triggered.verify.out)
						.id_token
						.split('.')[1]
				));
		}
		token.email = doc.id_token.email;
		token.sub = doc.id_token.sub;
		token.email_verified = doc.id_token.email_verified;
	}
	else if(doc.triggers){
		//we are still waiting... nothignt really to do
		doc.phase--;
		doc.blockSave = true;
	}
	else if(doc.code){
		doc.triggers = {verify:{
				path:"https://accounts.google.com/o/oauth2/token"
				,headers:{'content-type' : 'application/x-www-form-urlencoded'}
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
	
	var encDoc = JSON.stringify(doc);
	encDoc = encodeURIComponent(encDoc);
	
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
					//,"<p>Attempting to redirect to google auth</p>"
					//,"<pre>"+JSON.stringify(doc,null,"\t")+"</pre><hr />"
					//,"<pre>"+JSON.stringify(req,null,"\t")+"</pre>"
					,"</body></html>"
				].join('\n')
		}
		,{
			headers : {
				"Content-Type" : "text/html"
			}
			,body : [""
					,"<html><body>"
					,"<form method='POST' action='./"+doc._id+"'>"
					,"Verifying with "+doc.authsource+"...<br />"
					//,"<input type='submit' value='waitmore' />"
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
					,"<form method='POST' action='./"+doc._id+"'>"
					,"<p>Finalized, user should get sesion token</p>"
					,"<table>"
					,"<tr><td>Email</td><td>"+token.email+"</td></tr>"
					,"<tr><td>Sub</td><td>"+token.sub+"</td></tr>"
					,"<tr><td>Verified</td><td>"+token.email_verified+"</tr>"
					,"</table>"
					//,"<hr />"
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

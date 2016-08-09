function(doc,req){
	var statetoken = req.uuid;
	
	var resp =  {
		"headers" : {
			"Content-Type" : "text/html"
			//,"Status" : 303
			//,"Location" : ""
		},
		"body" : "<html><body>Login with: <ul><li><a href=''>Login with Google</a></li></ul></body></html>"
	};
	
	//resp.body = JSON.stringify(req);
	
	return [null, resp];
}

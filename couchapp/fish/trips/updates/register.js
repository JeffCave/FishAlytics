function updatesRegister(doc,req){
	var resp =  {
		"headers" : {
			"Content-Type" : "text/html"
		},
		"body" : "<html><body>Login with: <ul><li><a href=''>Login with Google</a></li></ul></body></html>"
	};
	
	//resp.body = JSON.stringify(req);
	
	return [null, resp];
}

function(head, req) {
	provides("html", function() {
		html = "<html><body><form method='POST' action=''><textarea>{docs:[\n";
		var data = [];
		while (row = getRow()) {
			data.push(row.value);
		}
		data = JSON.stringify({docs:data});
		
		var html = "<html><body><script>(SCRIPTHERE)()</script></body></html>";
		html = html.replace('SCRIPTHERE',function(){
					var http = new XMLHttpRequest();
					http.open("POST", "/fishdev/_bulk_docs", true);
					http.setRequestHeader("Content-type", "application/json");
					http.onreadystatechange = function() {
						if(http.readyState == 4 && http.status == 200) {
							document.write(http.responseText);
						}
					};
					http.send(JSON.stringify(datahere));
				}.toString()
			 );
		html = html.replace('datahere',data);
			 
		return html;
	});
}

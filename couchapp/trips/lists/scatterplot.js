/* global provides */
/* global getRow */
function listsScatterplot(head, req) {
	var Mustache = require("lib/mustache");
	provides("html", function() {
		var doc = {
			data: {
				total_rows: 0
				,rows : []
			},
			PageTitle:"Standard Weight",
			userCtx : req.userCtx
		};
		for (var row = getRow(); row; row = getRow()) {
			doc.data.rows.push(row);
			doc.data.total_rows++;
		}
		doc.data = JSON.stringify(doc.data);
		var html = Mustache.render(this.templates.scatterplot, doc, this.templates.partials);
		return html;
	});
}

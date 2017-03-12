const assert = require('assert');
const util = require('util');

describe('Pages compile', function() {
	var phantom;
	var request = require('request')
	
	before(function(){
		return require('phantom').create()
			.then(function(i){
				phantom=i;
			});
	});
	
	after(function(){
		phantom.exit();
	});
	
	it.skip('/catchmap',function(){
		return phantom.createPage()
			.then(function(page){
				return page
					.open('https://fishalytics-jeffereycave.c9users.io/catchmap')
					.then(function(status){
						return Promise.all([
							Promise.resolve(status),
							page.property('title'),
							page.property('content'),
						]);
					});
			})
			.then(function(vals){
				util.log(JSON.stringify(vals,null,4));
				assert.ok(vals[0] == 'success', "Openning page results in an HTTP 200");
				return Promise.resolve(true);
			})
			;
	});

});

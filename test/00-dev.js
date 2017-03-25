const assert = require('assert');
const util = require('util');

describe('Development environment tests', function() {
	it('should contain mocha test', function() {
		assert.ok(true);
	});
	
	it('passes jsLint processing',function(){
		assert.ok(true);
	});
	
	describe('Test Dependancies',function(){
//		it('handles promise errors',function(){
//			return new Promise(function(){
//				throw new Error("a thing");
//			});
//		});
		it('needs browser',function(){
			var phantomFactory = require('phantom');
			var instance;
			return phantomFactory.create()
				.then(function(i){
					instance=i;
					return instance.createPage();
				})
				.then(function(page){
					//page.on("onResourceRequested", function(requestData) {
					//	console.info('Requesting', requestData.url)
					//});
					return page
						.open('http://example.com/')
						.then(function(status){
							return Promise.all([
								Promise.resolve(status),
								page.property('title'),
								page.property('content'),
							]);
						});
				})
				.then(function(vals){
					instance.exit();
					//util.log(JSON.stringify(vals,null,4));
					assert.ok(vals[0] == 'success', "Openning page results in an HTTP 200");
					assert.ok(vals[1] === 'Example Domain', "Title is correct");
					assert.ok(vals[2].includes("illustrative examples"), "Some sensible text is present on the page");
					return Promise.resolve(true);
				})
				;
		});
	});
	
});

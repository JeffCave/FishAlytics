/**
 * 
 * 
 * https://code.tutsplus.com/tutorials/an-introduction-to-webdriver-using-the-javascript-bindings--cms-21855
 * https://code.tutsplus.com/tutorials/headless-functional-testing-with-selenium-and-phantomjs--net-30545
 */

var assert = require("assert");

var phantom;
var page;

describe('Test example.com', function() {
	before(function(done) {
		require('phantom').create([], { 
			//phantomPath: '/path/to/phantomjs', 
			//logger: yourCustomLogger, 
			//logLevel: 'debug',
		}).then((instance)=>{
			phantom = instance;
			return instance.createPage();
		}).then(p=>{
			page = p;
			done();
		}).catch(()=>{
			phantom.exit();
			done();
		});
	});
	
	after(function(done) {
		phantom.exit();
		done();
	});
	
	describe.skip('Check homepage', function() {
		
		it('should see the correct title', function(fin) {
			page.open('http://phantomjs.org/', function (status) { 
				assert.ok(page.property('title').includes("Phantom"));
				fin();
			});
		});
		
		it('should see the body', function(done) {
			page.open('http://phantomjs.org/', function (status) { 
				assert.ok(page.property('plainText').includes("Headless WebKit"));
				done();
			});
		});
	});

});

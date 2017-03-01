/**
 * 
 * 
 * https://code.tutsplus.com/tutorials/an-introduction-to-webdriver-using-the-javascript-bindings--cms-21855
 * https://code.tutsplus.com/tutorials/headless-functional-testing-with-selenium-and-phantomjs--net-30545
 */

var assert = require("assert");

var client = require("webdriverjs").remote({
	desiredCapabilities:{
		browserName:'phantomjs'
	},
	logLevel:'silent/'
});

describe('Test example.com', function() {
	before(function(done) {
		client.init().url('http://example.com', done);
	});

	describe('Check homepage', function() {
		it('should see the correct title', function(done) {
			client.getTitle(function(err, title) {
				assert.ok(!err, 'Did not receive and errror');
				assert.ok(title.includes('Example'));
				done();
			});
		});

		it('should see the body', function(done) {
			client.getText('p', function(err, p) {
				assert.ok(!err);
				assert.ok(p.include('for illustrative examples in documents.'));
				done();
			});
		});
	});

	after(function(done) {
		client.end();
		done();
	});
});

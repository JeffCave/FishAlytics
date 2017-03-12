describe('Pages compile', function() {
	const request = require('request-promise-native');
	const url = 'http://localhost:8080';
	//const url = 'https://fishalytics-jeffereycave.c9users.io/catchmap';
	
	var chai = require("chai");
	var chaiAsPromised = require("chai-as-promised");
	chai.use(chaiAsPromised);
	
	
/*
	[
		'/',
		'/catchmap',
	].forEach(function(d){
		it(d,function(){ return request(url+d); });
	});
	
*/
	it('/',function(){ return request(url+'/'); });
	it('/catchmap',function(){ return request(url+'/catchmap'); });
});

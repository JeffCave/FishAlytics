describe('All the pages compile', function() {
	const request = require('request-promise-native');
	//const url = 'http://localhost:8080';
	//const url = 'https://fishalytics-jeffereycave.c9users.io';
	const url = 'http://fish.lvh.me:8080';
	
	var chai = require("chai");
	var chaiAsPromised = require("chai-as-promised");
	chai.use(chaiAsPromised);
	
	
/*
	[
		'/',
		'/catchmap',
	].sort().forEach(function(d){
		it(d,function(){ return request(url+d); });
	});
	
*/
	it('/',function(){ return request(url+'/'); });
	it('/intro/',function(){ return request(url+'/intro/'); });
	it('/catchmap',function(){ return request(url+'/catchmap'); });
});

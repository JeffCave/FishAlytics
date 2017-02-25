var assert = require('assert');

describe('Development environment tests', function() {
  it('should contain mocha test', function() {
    assert.ok(true);
  });
  
  it('passes jsLint processing',function(){
    assert.ok(true);
  });
  
  it.skip('needs selenium',function(){
    assert.ok(true);
  });
  
});

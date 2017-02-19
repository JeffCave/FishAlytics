/**
 * 
 * 
 * https://code.tutsplus.com/tutorials/an-introduction-to-webdriver-using-the-javascript-bindings--cms-21855
 * https://code.tutsplus.com/tutorials/headless-functional-testing-with-selenium-and-phantomjs--net-30545
 */

describe('Test example.com', function(){
    before(function(done) {
        client.init().url('http://example.com', done);
    });

    describe('Check homepage', function(){
        it('should see the correct title', function(done) {
            client.getTitle(function(err, title){
                expect(title).to.have.string('Example Domain');
                done();
            });
        });

        it('should see the body', function(done) {
            client.getText('p', function(err, p){
                expect(p).to.have.string(
                    'for illustrative examples in documents.'
                );
                done();
            })
        });
    });

    after(function(done) {
        client.end();
        done();
    });
});
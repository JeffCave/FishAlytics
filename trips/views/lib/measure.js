
var measures = {
	estimate: {
		weight: function(length,factor) {
			return factor.a * Math.pow(length,factor.b);
		},
		length: function(weight,factor) {
			return Math.pow( weight/factor.a , 1/factor.b );
		}
	}
};

// CommonJS bindings
if( typeof(exports) === 'object' ) {
	for($e in meaures){
		exports[$e] = measures[$e];
	}
};

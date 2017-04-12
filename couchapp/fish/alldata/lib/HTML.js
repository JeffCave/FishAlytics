/**
 * 
 */
var HTML = {
	stringify : function ($val){
		var $rtn = '';
		switch((Array.isArray($val)?'array':false) || typeof $val){
			case 'undefined':
				$rtn = '';
				break;
			case 'string':
				if(!isNaN(Date.parse($val))){
					$val = (new Date(Date.parse($val))).toISOString();
				} // jshint ignore : line
			case 'number': // jshint ignore : line
			case 'boolean': // jshint ignore : line
			default:
				$rtn = '<dd>{{val}}</dd>'
					.replace('{{val}}',$val);
				break;
			case 'array':
				$rtn = '';
				for(var $v=0; $v<$val.length;$v++){
					$rtn += '<dd>{{val}}</dd>'
						.replace(/{{val}}/g,HTML.stringify($val[$v]))
						// this makes me feel dirty
						.replace('<dd><dd>','<dd>')
						.replace('</dd></dd>','</dd>')
						;
				}
				break;
			case 'object':
				for(var $k in $val){
					var $shouldShow = $val.hasOwnProperty($k) && $k.startsWith('_') ;
					if($shouldShow){
						$rtn += '<dt>{{key}}</dt>{{val}}'
							.replace('{{key}}',$k)
							.replace('{{val}}',HTML.stringify($val[$k]))
							;
					}
				}
				$rtn = '<dl>' + $rtn + '</dl>';
				break;
		}
		return $rtn;
	}
};

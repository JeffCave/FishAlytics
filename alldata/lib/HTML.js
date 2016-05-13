/**
 * 
 */
var HTML = {
	stringify : function ($val){
		$rtn = ''
		switch((Array.isArray($val)?'array':false) || typeof $val){
			case 'undefined':
				$rtn = '';
				break;
			case 'string':
				if(!isNaN(Date.parse($val))){
					$val = (new Date(Date.parse($val))).toISOString();
				}
			case 'number':
			case 'boolean':
			default:
				$rtn = '<dd>{{val}}</dd>'
					.replace('{{val}}',$val);
				break;
			case 'array':
				$rtn = '';
				for($v=0; $v<$val.length;$v++){
					$rtn += '<dd>{{val}}</dd>'
						.replace(/{{val}}/g,Formatter.html($val[$v]))
						// this makes me feel dirty
						.replace('<dd><dd>','<dd>')
						.replace('</dd></dd>','</dd>')
						;
				}
				break;
			case 'object':
				for($k in $val){
					$shouldShow = 
						$val.hasOwnProperty($k)
						&& $k.substring(0,1) !== '_' 
						;
					if($shouldShow){
						$rtn += '<dt>{{key}}</dt>{{val}}'
							.replace('{{key}}',$k)
							.replace('{{val}}',Formatter.html($val[$k]))
							;
					}
				};
				$rtn = '<dl>' + $rtn + '</dl>';
				break;
		}
		return $rtn;
	}
};

function(doc, req) {  
	registerType('text-json', 'text/json');
	
//	provides('json', function(){
//		return {'json': doc};
//	});
	provides('text-json', function(){
		return toJSON(doc);
	});
	
	var Formatter = {
		//'escape': function(s){
		//	return s
		//		.replace(/>/g, '&gt;')
		//		.replace(/</g, '&lt;')
		//		.replace(/&/g, '&amp;');
		//},
		'xml': function(o){
			$tmpl = '<{{key}}>{{val}}</{{key}}>';
			$rtn = '';
			for($key in o){
				if(!o.hasOwnProperty($key)) continue;
				$val = o[$key];
				
				switch(typeof $val){
					case 'string':
					case 'number':
					default:
						$rtn += $tmpl
							.replace(/{{key}}/g,$key)
							.replace(/{{val}}/g,$val)
							;
						break;
					case 'array':
						for($v=0; $v<$val.length;$v++){
							$rtn += $tmpl
								.replace(/{{key}}/g,$key)
								.replace(/{{val}}/g,$val[$v]);
						}
						break;
					case 'object':
						$rtn += $tmpl
							.replace(/{{key}}/g,$key)
							.replace(/{{val}}/g,Formatter.xml($val));
						break;
				}
			}
			return $rtn;
		},
		'html': function($val){
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
		,'json' : function(o){
			return JSON.stringify(o, null, 4);
		}
	};
	
	provides('html', function(){
		return '<html><style>dl{margin-left:2em} dd{margin-left:2em}</style><body><article><h1>{{pk}}</h1>{{content}}</article><pre>{{raw}}</pre></body></html>'
			.replace('{{content}}', Formatter.html(doc))
			.replace('{{raw}}',Formatter.json(doc,null,4))
			.replace('{{pk}}', doc._id)
			;
	});
// 	
// 	provides('xml', function(){
// 		return {
// 			'headers': {'Content-Type': 'application/xml'}
// 			,'body' : ''.concat(
// 				'<?xml version="1.0" encoding="utf-8"?>\n',
// 				'<doc>',
// 				(function(){
// 					escape = function(s){
// 						return s.replace(/&quot;/g, '"')
// 								.replace(/&gt;/g, '>')
// 								.replace(/&lt;/g, '<')
// 								.replace(/&amp;/g, '&');
// 					};
// 					process = function(o){
// 						
// 						var content = '';
// 						for(var key in doc){
// 							if(!doc.hasOwnProperty(key)) continue;
// 							if(Array.isArray(doc[key]){
// 								doc[key].forEach(function(s){
// 									content += '<{key}>{value}</{key}>'
// 										.replace('{key}',escape(key))
// 										.replace('{value}',escape(process(value)))
// 								});
// 							}
// 							else if(Object.isObject(doc[key]){
// 								content += '<{key}>{value}</{key}>'
// 									.replace('{key}',escape(key))
// 									.replace('{value}',escape(process(value)))
// 									;
// 							}
// 							else{
// 								content += '<{key}>{value}</{key}>'
// 									.replace('{key}',escape(key))
// 									.replace('{value}',escape(value))
// 									;
// 							}
// 						}
// 						return content;
// 					}
// 					return process(doc);
// 				})(),
// 				'</doc>'
// 			)
// 		};
// 	});
}


var catchUtils = {
	asCatches: function(doc) {
		if(!this.isTrip(doc)){
			return [];
		}
		// waypoint min/max... to define the 
		// start and end of the trip
		$catches = (function(){
				$points = doc.waypoints;
				$min = $max = Date.parse($points[0].timestamp);
				for($i in $points){
					$cur = Date.parse($points[$i].timestamp);
					$max = ($max < $cur) ? $cur : $max;
					$min = ($min > $cur) ? $cur : $min;
				}
				return [$min,$max];
			})();
		
		//create mock catches out fo the waypoints
		$catches = [
			{
				key:'\u0001',
				val: {
					timestamp:'\u0001'
					,stats:{
						time: $catches[0]
						,span:0
						,lines:1
					}
				}
			},
			{
				key:'\uffff',
				val: {
					timestamp:'\uffff'
					,stats:{
						time: $catches[1]
						,span:0
						,lines:1
					}
				}
			}
		];
		
		//gather up the catches
		for($c in doc.catches){
			$catch = JSON.parse(JSON.stringify(doc.catches[$c]));
			
			//attach the license
			$catch['licenses'] = doc.licenses;
			//normalize data
			$catch.timestamp = (new Date($catch.timestamp)).toISOString();
			if(!Array.isArray($catch['coords'])){
				$catch['coords'] = [
					$catch['coords'].longitude || null
					,$catch['coords'].latitude || null
					,$catch['coords'].altitude || null
					,Date.parse($catch.timestamp) || null
				];
			}
			$catch.coords[0] = +$catch.coords[0] || null;
			$catch.coords[1] = +$catch.coords[1] || null;
			$catch.coords[2] = +$catch.coords[2] || null;
			$catch.coords[3] = +$catch.coords[3] || Date.parse($catch.timestamp);
			//prepare for calculation of statistics
			$catch['stats'] = {
				time:Date.parse($catch.timestamp)
				,span:0
				,lines:1
			};
			
			//determine the len/weigth ratio factor
			$len = $catch.fish.length || null;
			$weight = $catch.fish.weight || null;
			if($len && $weight){
				$catch.stats['lwFactor'] = Math.log($weight) / Math.log($len);
			}
		
			//calculate the lookup key
			$key = (new Date(Date.parse($catch.timestamp))).toISOString();
			$catches.push({key:$key,val:$catch});
			
		};
		
		//calculate the time spent catching the fish
		$catches.sort(function(a,b){
			if( a.val.timestamp > b.val.timestamp){return 1;}
			if( a.val.timestamp < b.val.timestamp){return -1;}
			return 0;
		});
		
		//average time before/after spent 
		for($i=$catches.length-2; $i>0; $i--){
			$catches[$i].val.stats.span = ($catches[$i+1].val.stats.time-$catches[$i-1].val.stats.time)/2000;
		}
		
		// get rid of the first and end
		// they only existed as markers for calculation
		$catches.pop();
		$catches.shift();
		
		
		// push the work out to the world
		for($c in $catches){
			$catch = $catches[$c];
			//clean it up as we go
			$catch.val.stats.fef = $catch.val.stats.span / $catch.val.stats.lines;
			delete $catch.val.stats.time;
			$catch.key = [$catch.key].concat($catch.val.coords);
		}
		return $catches;
	},
	docType: function(doc){
	   return doc._id.split('.')[0];
	},
	isTrip: function(doc){
	   return (this.docType(doc) === 'trip');
	}
};

// CommonJS bindings
if( typeof(exports) === 'object' ) {
	for($e in catchUtils){
		exports[$e] = catchUtils[$e];
	}
};

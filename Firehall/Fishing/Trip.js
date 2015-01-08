/// <remarks>
/// 
/// <pre>
/// $Id$
/// $URL$ 
/// </pre>
/// </remarks

var timer = null;


/// <summary>
/// Starts the timer that updates the duration
/// </summary>
function StartTimer(){
	//if the timer is already active, we are already done
	if(timer != null){
		return;
	}
	//set it to run every 30 seconds
	timer = window.setInterval(function(){UpdateDuration();},30000);
	//update it the first time
	UpdateDuration();
}


/// <summary>
/// Stops the timer that updates the duration
/// </summary>
function StopTimer(){
	window.clearInterval(timer);
	timer = null;
}


/// <summary>
/// Recalculates the length of time associated with the trip
/// </summary>
function UpdateDuration()
{
	var eTime = null;
	var sTime = null;
	var lbl = txtDuration;
	
	//bail out if the start time hasn't been set
	try{
		sTime = new Date(tripdata.Start);
	} 
	catch (ex){
		sTime = "Invalid Date";
	}
	if(sTime == "Invalid Date"){
		lbl.value = "";
		return;
	}
	
	//fetch the end time
	if(tripdata.End != null){
		eTime = tripdata.End;
	}
	// if no end time has been entered, we assume we are fishing right now
	else{
		eTime = new Date();
	}
	
	//update
	var dur = new Date(eTime - sTime);
	lbl.value = "";
	if(dur.valueOf() >=86400000){
		lbl.value += Math.floor(dur.valueOf()/86400000) + "days ";
	}
	lbl.value = 
		("00" + dur.getHours()).slice(-2) + 
		":" + 
		("00" + dur.getMinutes()).slice(-2)
		;
}

/// <summary>
/// Handles a user changing the start date
/// </summary>
function HandleStartChange()
{
	var date = tripdata.Start;
	try{
		date = 
			new Date(txtStartDate.value) + 
			new Date(txtStartTime.value) ;
	}
	catch(ex){
	}
	if(date != trip.Start){
		tripdata.Start = date;
	}
}

/// <summary>
/// Handles a user changing the end date
/// </summary>
function HandleEndChange()
{
	var date = tripdata.Finish
	try{
		date = new Date(txtEnd.value);
	}
	catch(ex){
	}
	if(date != trip.Finish){
		tripdata.Finish = date;
	}
}

/// <summary>
/// Handles a change to the "start" field
/// </summary>
function UpdateStart(id, oldval, newval)
{
	var str;
	str = newval.toDateString();
	if(str != txtStartDate.value){
		txtStartDate.value = str;
	}
	str = newval.toTimeString();
	if(str != txtStartTime.value){
		txtStartTime.value = str;
	}
}

/// <summary>
/// Renders the End Date controls
/// </summary>
function RenderEndDate()
{
	var MAXDATE = new Date("01 January, 9000 UTC");
	var val = null;
	//end button needs to set the end value initially
	alert("HERE:"+MAXDATE);
	val = 
		(tripdata.Finish >= MAXDATE)
		?"none"
		:"inline";
	alert("THERE:"+val);
	txtEnd.style.display = val;
	alert("EVERYWHERE");
	val = 
		(txtEnd.style.display=="none")
		?"inline"
		:"none";
	btnTripFin.style.display = val;
}

/// <summary>
/// Initialiazes the objects on the page
/// </summary>
function RunPage(){
	tripdata.watch("Start",UpdateDuration);	
	tripdata.watch("Finish",UpdateDuration);
	
	tripdata.watch("Start",UpdateStart);
	tripdata.watch("Finish",function(id, oldval, newval){
		var str = "";
		try{
			str = newval.toLocaleString();
		} 
		catch(ex){
		
		}
		if(str != txtEnd.value){
			txtEnd.value = str;
			txtEnd.style.display = "inline";
			btnTripFin.style.display = "none";
		}
		if(str == '31/12/9999 19:59:59'){
			txtEnd.value = "";
			txtEnd.style.display = "none";
			btnTripFin.style.display = "inline";
		}
	});
	
	txtStartDate.addEventListener('blur', HandleStartChange);
	txtStartTime.addEventListener('blur', HandleStartChange);
	txtEnd.addEventListener('blur', HandleEndChange);
	
	RenderEndDate();
	
	btnTripFin.addEventListener('click', function(){
		var dte = new Date();
		tripdata.Finish = dte;
		txtEnd.style.display='inline';
		btnTripFin.style.display='none';
	});
	
	StartTimer();
}

/// <summary>
/// Binds the given HTML element to the given object.property
/// </summary>
function DataBind(element, obj, property)
{
	obj.watch(property,function(){
		if(obj[property] == element.value){
			return;
		}
		element.value = obj[property];
	});
	element.addEventListener('blur',function(){
		if(obj[property] == element.value){
			return;
		}
		obj[property] = element.value;
	});
}

/**
 * 
 * <p><pre>
 * $Id$
 * $URL$ 
 * </pre></p>
 */

var timer = null;


/**
 *
 */
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


/**
 *
 */
function StopTimer(){
	window.clearInterval(timer);
	timer = null;
}


/**
 *
 */
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


/**
 *
 */
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
	tripdata.Start = date;
}


/**
 *
 *
 */
function HandleEndChange()
{
}


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

/**
 *
 */
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
	
	//end button needs to set the end value initially
	txtEnd.style.display = 
		(tripdata.Finish == new Date("31/12/9999 19:59:59"))
		?"none"
		:"inline";
	btnTripFin.style.display = 
		(txtEnd.style.display!="none")
		?"none"
		:"inline";
	btnTripFin.addEventListener('click', function(){
		var dte = new Date();
		tripdata.Finish = dte;
		txtEnd.style.display='inline';
		btnTripFin.style.display='none';
	});
	
	StartTimer();
}


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

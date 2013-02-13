window.onload = function () {
    var status = document.getElementById("status");

    if (!window.WebSocket) {
        status.innerHTML = "Your browser does not support web sockets!";
        return;
    }

    status.innerHTML = "Connecting to server...";

    // Initialize a new web socket.
    var socket = new WebSocket("ws://localhost:8181/KinectHtml5");

    // Connection established.
    socket.onopen = function () {
        status.innerHTML = "Connection successful.";
    };

    // Connection closed.
    socket.onclose = function () {
        status.innerHTML = "Connection closed.";
    }

    // Receive data FROM the server!
    socket.onmessage = function (evt) {
        status.innerHTML = "Kinect data received.";

        // Get the data in JSON format.
        var jsonObject = eval('(' + evt.data + ')');

        /*
        possible values for joint.name:
        joint name = hipcenter
        joint name = spine
        joint name = shouldercenter
        joint name = head
        joint name = shoulderleft
        joint name = elbowleft
        joint name = wristleft
        joint name = handleft
        joint name = shoulderright
        joint name = elbowright
        joint name = wristright
        joint name = handright
        joint name = hipleft
        joint name = kneeleft
        joint name = ankleleft
        joint name = footleft
        joint name = hipright
        joint name = kneeright
        joint name = ankleright
        joint name = footright
        */
		
		// save all joints in a dictionary (associative array) with key = name; value = actual joint object
		var allJoints = new Array();
		
		// assume 1 skeleton
		if(jsonObject.skeletons.length == 1)
		{
			for (var j = 0; j < jsonObject.skeletons[0].joints.length; j++) 
			{
                var joint = jsonObject.skeletons[0].joints[j];			
				allJoints[joint.name] = joint;
            }
		}
		
		// PROBLEM 1: get the right canvas element / graph element
		// PROBLEM 2: interact with graph element depending on kinect sensor data
			// PROBLEM 2.1: how to trigger selection event on graph node
		
		// TRY: get the same canvas jit is drawing into?
        //var canvas = document.getElementById("ns-avgl-facetgraph-infovis-canvas");
		// ORIGINAL:
		var canvas = document.getElementById("canvas");
        var ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height);
		
		// TRY: get overlay to draw on top of jit canvas? - note: facetgraph overlay is a div; 
		// is it possible to have two canvas elements on top of each other
		// and both drawings are visible (i.e. canvas itself is transparent)?
		var overlay = document.getElementById("ns-avgl-facetgraph-overlay");

		// instead of drawing kinect joints as below...
        var rightHandJoint = allJoints["handright"];
			// draw ellipsis at right hand position        
			ctx.save();
			ctx.scale(0.75, 1);
			ctx.beginPath();
			ctx.arc(parseFloat(rightHandJoint.x), parseFloat(rightHandJoint.y), 10, 0, Math.PI * 2, false);
			ctx.stroke();
			ctx.closePath();
			ctx.restore();
		
		// ... select nodes in the graph depending on joint data
		// for example: if right hand is extended more than 30cm from hip, change selection




        // Inform the server about the update.
        socket.send("Skeleton updated on: " + (new Date()).toDateString() + ", " + (new Date()).toTimeString());
    };
};
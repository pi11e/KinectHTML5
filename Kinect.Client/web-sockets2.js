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
        var canvas = document.getElementById("canvas");
        var ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        for (var i = 0; i < jsonObject.skeletons.length; i++) {
            for (var j = 0; j < jsonObject.skeletons[i].joints.length; j++) {

                var joint = jsonObject.skeletons[i].joints[j];
                if (joint.name == "handright") {
                    ctx.save();
                    ctx.scale(0.75, 1);
                    ctx.beginPath();
                    ctx.arc(parseFloat(joint.x), parseFloat(joint.y), 10, 0, Math.PI * 2, false);
                    ctx.stroke();
                    ctx.closePath();
                    ctx.restore();
                }
            }
        }




        // Inform the server about the update.
        socket.send("Skeleton updated on: " + (new Date()).toDateString() + ", " + (new Date()).toTimeString());
    };
};
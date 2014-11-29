// EpgDataCap_Bon Web API
var WebAPI = {
    WebSocketUrl: "/ws",
    SocketConnection: undefined,
    JSONSupported: (function () {
        try {
            var x = JSON;
            return true;
        } catch (e) { }
        return false;
    })(),
    opened: function (e) {
        WebAPI.SocketConnection.send("L-CHECK");
    },
    onboot: function (e) { },
    _onclose: function (e) {
        WebAPI.isOpen = false;
        WebAPI.SocketConnection = undefined;
        setTimeout(WebAPI.onclose, 0);
    },
    onclose: function (e) { },
    onreqauth: function () { },
    isOpen: false,
    login: function (key) {
        if (arguments.length == 2) {
            WebAPI.SocketConnection.send("LOGIN " + arguments[0] + " " + arguments[1]);
        } else {
            WebAPI.SocketConnection.send("LOGIN " + arguments[0]);
        }
    },
    open: function () {
        WebAPI.sessKey = "";
        var error = false;
        if (!WebAPI.JSONSupported) {
            console.error("JSON is not supported");
            return;
        }
        if (WebAPI.SocketConnection == undefined) {
            WebAPI.SocketConnection = new WebSocket("ws://" + location.host + WebAPI.WebSocketUrl);
            var isWSOpen = false;
            WebAPI.SocketConnection.onopen = WebAPI.opened;
            WebAPI.SocketConnection.onclose = WebAPI._onclose;
            WebAPI.SocketConnection.onmessage = WebAPI.receiveWS;
            return 2;
        }
        return 0;
    },
    call: function (name, param, callback) {
        WebAPI.open(WebAPI.socketSupported);
        WebAPI.callWS(name, param, callback);
    },
    sessKey: "",
    onevent: function(e){},
    receiveWS: function (e) {
        var msg = e.data;
        if (msg.substring(0, 3) == "+OK") {
            if (msg.split(" ").length < 2) return;
            //console.log("API: WS Receive OK");
            var cb = msg.substring(3, msg.indexOf(" "));
            console.log("API: Received " + cb);
            var json = msg.substring(msg.indexOf(" ") + 1);
            Callbacks[cb](json);
        } else if (msg.substring(0, 4) == "+LOK") {
            WebAPI.sessKey = msg.substring(5);
            console.log("API: auth success " + WebAPI.sessKey);
            setTimeout(WebAPI.onboot, 0);
        } else if (msg.substring(0, 4) == "LERR") {
            console.warn("API: auth fail");
            setTimeout(WebAPI.onreqauth, 0);
        } else if (msg.substring(0, 5) == "EVENT") {
            console.log("API: Received Event" + msg.substring(5));
            WebAPI.onevent(JSON.parse(msg.substring(5)));
        } else if (msg.substring(0,3) == "-LA") {
            console.log("API: request auth");
            setTimeout(WebAPI.onreqauth, 0);
        } else if (msg.substring(0, 3) == "-LN") {
            console.log("API: not request auth");
            setTimeout(WebAPI.onboot, 0);
        } else {
            //console.error("API: WS Receive [Invalid Command]" + msg);
        }
    },
    callWS: function (name, param, callback) {
        var cbName = name + new Date().getMilliseconds();
        cbName = WebAPI.addCB(cbName, callback, true);
        WebAPI.SocketConnection.send("RUNCMD " + cbName + " " + name + WebAPI.genaratePDir(param));
    },
    addCB: function (name, main, isJSON) {
        if (Callbacks[name] != undefined) {
            name += "0";
            WebAPI.addCB(name.main, isJSON);
        }
        Callbacks[name + "_main"] = main;
        Callbacks[name] = function (data) {
            if (Callbacks[name + "_main"] == undefined) return;
            Callbacks[name + "_main"](isJSON ? JSON.parse(data) : data);
        };
        return name;
    },
    genaratePDir: function (param) {
        var dir = "";
        for (var name in param) {
            dir += "/" + name + "=" + param[name];
        }
        return dir;
    },
    close: function () {
        if (WebAPI.SocketConnection != undefined) {
            if (WebAPI.ReqAuth) WebAPI.SocketConnection.send("LOGOUT");
            WebAPI.SocketConnection.close();
        }
        WebAPI.SocketConnection = undefined;
        WebAPI.isOpen = false;
        WebAPI.isAuth = false;
    }
};
var Callbacks = {};

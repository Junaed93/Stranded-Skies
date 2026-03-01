var GameWebSocketPlugin = {
  GameWSConnect: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    console.log("[GameWS] Connecting to " + url);

    try {
      var ws = new WebSocket(url);

      ws.onopen = function () {
        console.log("[GameWS] Connected!");
        if (typeof SendMessage === "function") {
          SendMessage("SocketClient", "OnWSOpen", "1");
        } else if (window.unityInstance) {
          window.unityInstance.SendMessage("SocketClient", "OnWSOpen", "1");
        }
      };

      ws.onmessage = function (event) {
        if (typeof event.data === "string") {
          if (typeof SendMessage === "function") {
            SendMessage("SocketClient", "OnWSMessage", event.data);
          } else if (window.unityInstance) {
            window.unityInstance.SendMessage(
              "SocketClient",
              "OnWSMessage",
              event.data,
            );
          }
        }
      };

      ws.onerror = function () {
        console.error("[GameWS] Connection error");
      };

      ws.onclose = function () {
        console.log("[GameWS] Disconnected");
      };

      window._gameWebSocket = ws;
    } catch (e) {
      console.error("[GameWS] Failed: " + e.message);
    }
  },

  GameWSSend: function (msgPtr) {
    var msg = UTF8ToString(msgPtr);
    if (window._gameWebSocket && window._gameWebSocket.readyState === 1) {
      window._gameWebSocket.send(msg);
    }
  },

  GameWSClose: function () {
    if (window._gameWebSocket) {
      window._gameWebSocket.close();
      window._gameWebSocket = null;
    }
  },

  GameWSGetUrlParam: function (keyPtr) {
    var key = UTF8ToString(keyPtr);
    var params = new URLSearchParams(window.location.search);
    var value = params.get(key) || "";
    var bufferSize = lengthBytesUTF8(value) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(value, buffer, bufferSize);
    return buffer;
  },
};

mergeInto(LibraryManager.library, GameWebSocketPlugin);

using UnityEngine;
using System;
using System.Collections;

using MsgPack;
using WebSocketSharp;

public class Socket : MonoBehaviour {

  // WebSocket Status Code
  // see : https://triple-underscore.github.io/RFC6455-ja.html
  // see : https://developer.mozilla.org/ja/docs/Web/API/CloseEvent

  // ping check count
  private const int PingErrorMaxCount = 3;

  // socket 
  private WebSocket ws;

  // keep alive
  private Coroutine keepAliveCor;

  // default callback send
  private Action<bool> cbSendDefault;

  // callbacks
  // TODO : 

  void Start() {
    // make callback
    cbSendDefault = (r) => {/* do nothing */};

    // ping routine
    Action cbError = () => {
      Debug.LogError("keep alive failed!! close connection.");
      closeAtOnce(CloseStatusCode.Abnormal, "keep alive failed");
    };
    keepAliveCor = StartCoroutine(keepAlive(cbError));
  }

  public void Connect(string url, Action cb) {
    if (ws != null) {
      Debug.LogWarning("socket still exists!!");
      return;
    }

    ws = new WebSocket(url);
    ws.OnOpen    += onOpen;
    ws.OnMessage += onMessage;
    ws.OnClose   += onClose;
    ws.OnError   += onError;

    // connect async
    ws.ConnectAsync();
  }

  public void Send(byte[] sendData, Action<bool> cb = null) {
    if (!ws.IsAlive) {
      Debug.LogWarning("socket is not connected !! canceled data sending.");
      return;
    }

    // if callback is not defined
    if (cb == null) cb = cbSendDefault;
    // send async
    ws.SendAsync(sendData, cb);
  }

  public void Close(Action cb) {
    if (ws != null && ws.IsAlive) {
      var d = BitConverter.GetBytes(true);
      var result = makeData(1, d);

      // leave and close
      Action<bool> cbClose = (b) => {
        ws.CloseAsync(CloseStatusCode.Normal, "socket is unneccesary");
      };
      ws.SendAsync(result, cbClose);
    }
  }


  private void onOpen(object obj, EventArgs e) {
    Debug.Log("onOpen!!");
  }

  private void onClose(object obj, CloseEventArgs e) {
    Debug.Log("onClose!!");
    if (e.Code != (ushort)CloseStatusCode.Normal) {
      Debug.LogError("[ERROR] session refused : " + e.Reason);
      // TODO : error callback
    }
    Debug.Log("close state  : " + e.Code);
    Debug.Log("close reason : " + e.Reason);
    ws = null;
  }

  private void onError(object obj, ErrorEventArgs e) {
    Debug.Log("recieve error!! " + e.Message);
  }

  private void onMessage(object obj, MessageEventArgs e) {
    if (e.IsBinary) {
      byte[] cmdId = new byte[4];
      byte[] data = new byte[e.RawData.Length - 4];


      Buffer.BlockCopy(e.RawData, 4, data, 0, data.Length);

      var unpack = new ObjectPacker();
      var message = unpack.Unpack<string>(data);
      Debug.Log("message length : " + e.RawData.Length);
      Debug.Log("message : " + message);
    } else if (e.IsText) {
    } else if (e.IsPing) {
    }
  }

  private void closeAtOnce(CloseStatusCode code, string reason = "") {
    // close socket
    if (ws != null) {     
      ws.Close(code, reason);
    }
  }

  private IEnumerator keepAlive(Action cbError) {
    int count = 0;

    while (true) {
      if (ws != null && ws.IsAlive) {
        bool ok = ws.Ping();
        if (!ok) {
          Debug.Log("ping ng...");
          count++;
          // error if count is over.
          if (count >= PingErrorMaxCount) {
            // callback
            cbError();
            yield break;
          }
        } else {
          count = 0;
          Debug.Log("ping ok...");
        }
      }
      yield return new WaitForSeconds(3);
    }
  }

  private byte[] makeData(int no, object obj) {

    ObjectPacker packer = new MsgPack.ObjectPacker();
    var data = packer.Pack(obj);
    var info = BitConverter.GetBytes(no);

    byte[] result = new byte[info.Length + data.Length];

    // first : command
    Buffer.BlockCopy(info, 0, result, 0, info.Length);

    // second : data
    Buffer.BlockCopy(data, 0, result, info.Length, data.Length);

    return result;
  }

  void OnDestroy() {
    if (ws != null) {
      var d = BitConverter.GetBytes(true);
      var result = makeData(1, d); 
      ws.Send(result);

      closeAtOnce(CloseStatusCode.Normal, "destory client");
    }

    // stop keepAlive
    StopCoroutine(keepAliveCor);
  }
}

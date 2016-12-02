﻿#define DEBUG_PRINT_SOCKET

using UnityEngine;
using System;
using System.Collections;

using MsgPack;
using WebSocketSharp;

namespace Network {
  namespace Socket {
    public class SocketManager : MonoBehaviour {

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

      public bool CanUse { get { return (ws != null && ws.IsAlive); } }

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
        if (ws == null || !ws.IsAlive) {
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
          ws.CloseAsync(CloseStatusCode.Normal, "socket is unneccesary");
        }
      }


      private void onOpen(object obj, EventArgs e) {
        _log("onOpen!!");
      }

      private void onClose(object obj, CloseEventArgs e) {
        _log("onClose!!");
        if (e.Code != (ushort)CloseStatusCode.Normal) {
          Debug.LogError("[ERROR] session refused : " + e.Reason);
          // TODO : error callback
        }
        _log("close state  : " + e.Code);
        _log("close reason : " + e.Reason);
        ws = null;
      }

      private void onError(object obj, ErrorEventArgs e) {
        Debug.LogError("recieve error!! " + e.Message);
      }

      private void onMessage(object obj, MessageEventArgs e) {
        if (e.IsBinary) {
          byte[] cmdId = new byte[4];
          byte[] data = new byte[e.RawData.Length - 4];


          Buffer.BlockCopy(e.RawData, 4, data, 0, data.Length);

          var unpack = new ObjectPacker();
          var message = unpack.Unpack<string>(data);
          _log("message length : " + e.RawData.Length);
          _log("message : " + message);
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
              _log("ping ng...");
              count++;
              // error if count is over.
              if (count >= PingErrorMaxCount) {
                // callback
                cbError();
                yield break;
              }
            } else {
              count = 0;
              _log("ping ok...");
            }
          }
          yield return new WaitForSeconds(3);
        }
      }

      void OnDestroy() {
        if (ws != null) {
          closeAtOnce(CloseStatusCode.Normal, "destory client");
        }

        // stop keepAlive
        StopCoroutine(keepAliveCor);
      }

      private void _log(object message) {
        #if DEBUG_PRINT_SOCKET
        Debug.Log(message);
        #endif
      }
    }
  }
}
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MsgPack;
using Network.Socket;

public class Manager : MonoBehaviour
{
  // room proc
  //private const string Url       = "ws://188.166.255.127:8080/";
  private const string Host     = "ws://localhost:8080/";
  private const string UrlJoin = Host + "get";

  private enum typeStep {
    Entry,
    Connecting,
    Idle,
    Leaving
  }
  private typeStep curStep;

  [Header("Window")]
  [SerializeField]
  private WindowEntryName windowEntryName;
  [SerializeField]
  private WindowWaiting   windowWaiting;
  [SerializeField]
  private WindowChat      windowChat;

  // online menber
  private Dictionary<int, string> mapMember = new Dictionary<int, string>();

  // callback map
  private Dictionary<int, Action<byte[]>> mapReceiveFunc = new Dictionary<int, Action<byte[]>>();

  private SocketManager sock;

  private int userId;

	// Use this for initialization
	void Start () {
    GameObject obj = new GameObject("Socket");
    sock = obj.AddComponent<SocketManager>();

    // decide user id randomly
    userId = UnityEngine.Random.Range(1, int.MaxValue);

    // set callback to this.
    sock.cbOpen          = callbackSocketOpen;
    sock.cbClose         = callbackSocketClose;
    sock.cbMessageString = callbackMessageString;
    sock.cbMessage       = callbackMessage;

    windowChat.cbLeaveRoom   = callbackLeaveRoom;
    windowChat.cbSendMessage = callbackSendMessage;

	  // register onMessage callback map
    mapReceiveFunc[1] = receiveChat;
    mapReceiveFunc[2] = receiveStatus;
    mapReceiveFunc[3] = receiveMemberInfo;
	}

  void Update() {
    switch (curStep) {
    case typeStep.Entry:
      if (windowEntryName.isStepComplete) {
        windowEntryName.SetActive(false);
        windowWaiting.SetActive(true, curStep.ToString());
        // connect
        string url = UrlJoin + "?uid=" + userId.ToString() + "&gid=" + windowEntryName.groupId.ToString() + "&name=" + windowEntryName.userName;
        Debug.Log(url);
        sock.Connect(url);
        curStep = typeStep.Connecting;
      }
      break;

    case typeStep.Connecting:
      if (sock.CanUse) {
        windowWaiting.SetActive(false);
        windowChat.SetActive(true);
        windowChat.ClearRecievedMessage();
        curStep = typeStep.Idle;
      }
      break;

    case typeStep.Idle:
      // do nothing
      // NOTE : step is changed by leave button.
      break;

    case typeStep.Leaving:
      // if socket close
      if (sock.IsUnavaiable) {
        windowWaiting.SetActive(false);
        windowEntryName.Reset();
        windowEntryName.SetActive(true);
        curStep = typeStep.Entry;
      }
      break;

    default:
      break;
    }
  }

  private void sendStatus(int status, Action<bool> cb = null) {
    Data.StatusInfo info = new Data.StatusInfo();
    info.UserId   = userId;
    info.UserName = windowEntryName.userName;
    info.Status   = status;
    byte[] result = makeData(2, info);
    sock.Send(result, cb);
  }

  private void callbackSendMessage(WindowChat.TypeChat rangeType, int rangeId, string message) {
    if (message.Length > 0) {
      Data.ChatInfo c = new Data.ChatInfo();
      c.FromId    = userId;
      c.Name      = windowEntryName.userName;
      c.Message   = message;

      c.RangeType = (int)rangeType;

      switch (rangeType) {
      case WindowChat.TypeChat.World:
        c.RangeId   = -1;
        break;
      
      case WindowChat.TypeChat.Group:
        c.RangeId = windowEntryName.groupId;
        break;

      case WindowChat.TypeChat.Private:
        c.RangeId = rangeId;
        break;
      }

      var result = makeData(1, c);
      sock.Send(result);
    }
  }

  private void callbackLeaveRoom() {
    /*
    byte[] d = BitConverter.GetBytes(true);
    byte[] result = makeData(0, d);
    sock.Send(result, callbackLeave);
    */

    // leave and close
    Action<bool> cb = (b) => {
      sock.Close();
    };
    sendStatus(0, cb);


    curStep = typeStep.Leaving;

    windowChat.SetActive(false);
    windowWaiting.SetActive(true, curStep.ToString());
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

  private void callbackSocketOpen() {
    Debug.Log("open");

    // get member info
    Data.SendMemberInfo info = new Data.SendMemberInfo();
    info.UserId = userId;
    byte[] data = makeData(3, info);
    sock.Send(data);

    // login notice
    sendStatus(1);
  }

  private void callbackSocketClose(bool wasCloseSafe) {
    if (wasCloseSafe) {
      Debug.Log("close connection safety.");
    }
  }

  private void callbackMessage(byte[] raw) {
    int cmdId = BitConverter.ToInt32(raw, 0);
    byte[] data = new byte[raw.Length - 4];
    Buffer.BlockCopy(raw, 4, data, 0, data.Length);

    // call function on receive
    Action<byte[]> rf = null; 
    mapReceiveFunc.TryGetValue(cmdId, out rf);
    if (rf != null) {
      rf(data);
    }
  }

  private void callbackMessageString(string str) {
  }

  private void receiveChat(byte[] data) {
    ObjectPacker unpack = new ObjectPacker();
    Data.ChatInfo info = unpack.Unpack<Data.ChatInfo>(data);
    windowChat.SetMessage(info, userId);
  }

  private void receiveStatus(byte[] data) {
    ObjectPacker unpack = new ObjectPacker();
    Data.StatusInfo info = unpack.Unpack<Data.StatusInfo>(data);
    updateMember(info);
  }

  private void receiveMemberInfo(byte[] data) {
    ObjectPacker unpack = new ObjectPacker();
    Data.StatusInfo[] infos = unpack.Unpack<Data.StatusInfo[]>(data);

    foreach (Data.StatusInfo info in infos) {
      updateMember(info);
    }
  }

  private void updateMember(Data.StatusInfo info) {
    // not regist myself
    if (info.UserId == userId) return;

    if (info.Status < 1 && mapMember.ContainsKey(info.UserId)) {
      Debug.Log("remove!!" + info.UserId.ToString() + " : " + info.UserName);
      mapMember.Remove(info.UserId);
    }
    else {
      Debug.Log("regist!!" + info.UserId.ToString() + " : " + info.UserName);
      mapMember[info.UserId] = info.UserName;
    }
    windowChat.UpdateMemberList(mapMember);
  }
}

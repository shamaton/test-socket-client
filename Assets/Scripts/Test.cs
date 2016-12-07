using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

using MsgPack;
using Network.Socket;

public class Test : MonoBehaviour
{
  // room proc
  private const string Url       = "ws://localhost:8080/";
  private const string UrlCreate = Url + "get_and_create";
  private const string UrlJoin   = Url + "get";

  [SerializeField] private InputField _inputChat;
  [SerializeField] private InputField _inputRoomNo;
  [SerializeField] private Button buttonChat;
  [SerializeField] private Button buttonRoomNo;
  [SerializeField] private Button buttonCreate;
  [SerializeField] private Button buttonLeave;

  private SocketManager sock;

	// Use this for initialization
	void Start () {
    GameObject obj = new GameObject("Socket");
    sock = obj.AddComponent<SocketManager>();

    setActiveChat(false);
    setActiveRoomNo(true);

    // set callback to this.
    sock.cbOpen  = callbackSocketOpen;
    sock.cbClose = callbackSocketClose;

	  // データやり取り登録
	}

  public void OnButtonChat() {
    if (_inputChat.text.Length > 0) {
      //var result = makeData(2, _inputChat.text);
      //sock.Send(result);
      sock.Send(_inputChat.text);
    }
  }

  public void OnButtonCreate() {
    setActiveRoomNo(false);
    sock.Connect(UrlCreate);
  }

  public void OnButtonEnter() {
    if (_inputRoomNo.text.Length < 1) return;

    string roomId = _inputRoomNo.text;
    string url = UrlJoin + "?room_id=" + roomId;
    setActiveRoomNo(false);
    sock.Connect(url);
  }

  public void OnButtonLeave() {
    byte[] d = BitConverter.GetBytes(true);
    byte[] result = makeData(1, d);
    setActiveChat(false);
    sock.Send(result, callbackLeave);
  }

  private void callbackLeave(bool b) {
    Debug.Log("callback lievae");
    setActiveRoomNo(true);
    sock.Close();
  }

  private void setActiveChat(bool isActive) {
    _inputChat.interactable = isActive;
    buttonChat.interactable = isActive;
    _inputChat.text = "";
    buttonLeave.interactable = isActive;
  }

  private void setActiveRoomNo(bool isActive) {
    _inputRoomNo.interactable = isActive;
    buttonRoomNo.interactable = isActive;
    if (isActive) {
      _inputRoomNo.text = "";
    }
    buttonCreate.interactable = isActive;
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
    Debug.Log("ooooooooooooopen");
    setActiveChat(true);
  }

  private void callbackSocketClose(bool wasCloseSafe) {
    if (wasCloseSafe) {
      Debug.Log("close connection safety.");
    }
    setActiveRoomNo(true);
  }
}

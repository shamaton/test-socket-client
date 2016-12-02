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

  private SocketManager sock;

	// Use this for initialization
	void Start () {
    GameObject obj = new GameObject("Socket");
    sock = obj.AddComponent<SocketManager>();
	  // データやり取り登録
	}

  public void OnButtonChat() {
    if (_inputChat.text.Length > 0) {
      var result = makeData(2, _inputChat.text);
      sock.Send(result);
    }
  }

  public void OnButtonCreate() {
    Action cb = () => Debug.Log("Socket connect!!");
    sock.Connect(UrlCreate, cb);
  }

  public void OnButtonEnter() {
    if (_inputRoomNo.text.Length < 1) return;

    string roomId = _inputRoomNo.text;
    string url = UrlJoin + "?room_id=" + roomId;
    Action cb = () => Debug.Log("Socket connect!!");
    sock.Connect(url, cb);
  }

  public void OnButtonLeave() {
    byte[] d = BitConverter.GetBytes(true);
    byte[] result = makeData(1, d);
    sock.Send(result, callbackLeave);
  }

  private void callbackLeave(bool b) {
    Action cb = () => Debug.Log("close connetion safety.");
    sock.Close(cb);
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
}

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
  [SerializeField] private ToggleGroup toggleGroupGroupId;
  [SerializeField] private Text ReceiveMessage;

  // callback map
  private Dictionary<int, Action<byte[]>> mapReceiveFunc = new Dictionary<int, Action<byte[]>>();

  private SocketManager sock;

  private int userId;

  public struct chatInfo {
    public int    RangeType;
    public int    RangeId;
    public int    UserId;
    public string Name;
    public string Message;
  }

	// Use this for initialization
	void Start () {
    GameObject obj = new GameObject("Socket");
    sock = obj.AddComponent<SocketManager>();

    setActiveChat(false);
    setActiveRoomNo(true);

    // decide user id randomly
    userId = UnityEngine.Random.Range(1, int.MaxValue);

    // set callback to this.
    sock.cbOpen          = callbackSocketOpen;
    sock.cbClose         = callbackSocketClose;
    sock.cbMessageString = callbackMessageString;
    sock.cbMessage       = callbackMessage;

	  // register onMessage callback map
    mapReceiveFunc[1] = receiveChat;
	}

  public void OnButtonChat() {
    if (_inputChat.text.Length > 0) {
      chatInfo c = new chatInfo();
      c.UserId = userId;
      c.Name = "username";
      c.RangeType = 1;
      c.RangeId = -1;
      c.Message = _inputChat.text;
      var result = makeData(1, c);
      sock.Send(result);
    }
  }

  public void OnButtonCreate() {
    setActiveRoomNo(false);
    sock.Connect(UrlCreate);
  }

  public void OnButtonEnter() {
    if (_inputRoomNo.text.Length < 1) return;

    // group id from toggle
    string label = toggleGroupGroupId.ActiveToggles().First().
    GetComponentsInChildren<Text>().First(t => t.name == "Label").text;
    string groupId = (label == "GROUP 1") ? "1" : "2";

    string url = UrlJoin + "?uid=" + userId.ToString() + "&gid=" + groupId;
    setActiveRoomNo(false);
    sock.Connect(url);
  }

  public void OnButtonLeave() {
    byte[] d = BitConverter.GetBytes(true);
    byte[] result = makeData(0, d);
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
    Debug.Log("open");
    setActiveChat(true);
  }

  private void callbackSocketClose(bool wasCloseSafe) {
    if (wasCloseSafe) {
      Debug.Log("close connection safety.");
    }
    setActiveRoomNo(true);
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
    ReceiveMessage.text = str;
  }

  private void receiveChat(byte[] data) {
    ObjectPacker unpack = new ObjectPacker();
    chatInfo info = unpack.Unpack<chatInfo>(data);
    Debug.Log(info.RangeType);
    Debug.Log(info.RangeId);
    Debug.Log(info.UserId);
    Debug.Log(info.Name);
    Debug.Log(info.Message);
  }
}

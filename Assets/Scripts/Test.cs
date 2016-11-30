using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using WebSocketSharp;

using MsgPack;

public class Test : MonoBehaviour
{
  // room proc
  private const string Url       = "ws://localhost:8080/";
  private const string UrlCreate = Url + "get_and_create";
  private const string UrlJoin   = Url + "get";

  // socket status
  private enum socketStatus {
    Ready,
    Connecting,
    Connect,
    Closing,
    Close = Ready
  }
  private socketStatus wsStatus = socketStatus.Ready;

  private WebSocket ws;

  [SerializeField] private InputField _inputChat;
  [SerializeField] private InputField _inputRoomNo;

  private Socket sock;

	// Use this for initialization
	void Start () {
    GameObject obj = new GameObject("Socket");
    sock = obj.AddComponent<Socket>();
	  // データやり取り登録
	}

  public void OnButtonChat() {
    if (_inputChat.text.Length > 0) {
      var result = makeData(2, _inputChat.text);
      //ws.Send(result);
      sock.Send(result);
    }
  }

  public void OnButtonCreate() {
    Action cb = () => Debug.Log("Socket connect!!");
    sock.Connect(UrlCreate, cb);
    //initializeSocket(UrlCreate);
  }

  public void OnButtonEnter() {
    if (_inputRoomNo.text.Length < 1) return;

    string roomId = _inputRoomNo.text;
    string url = UrlJoin + "?room_id=" + roomId;
    Action cb = () => Debug.Log("Socket connect!!");
    sock.Connect(url, cb);
    //initializeSocket(url);
  }

  public void OnButtonLeave() {
    //var result = makeData(1, "d"); 
    //ws.Send(result);

    Action cb = () => Debug.Log("close connetion safety.");
    sock.Close(cb);
    //StartCoroutine(closeProc(cb));
  }

  /*
  private void initializeSocket(string url) {
    if (ws != null) return;

    ws = new WebSocket(UrlCreate);
    ws.OnMessage += onMessage;

    // status ready
    wsStatus = socketStatus.Ready;

    Action cb = () => Debug.Log("Socket connect!!");
    StartCoroutine(connectProc(cb));
  }

  private IEnumerator connectProc(Action cb)
  {
    float timer = 0f;
    float waitTime = 10f;
    // start connect
    ws.ConnectAsync();
    wsStatus = socketStatus.Connecting;

    // waiting
    while (!ws.IsAlive)
    {
      timer += Time.deltaTime;
      if (timer > waitTime) {
        Debug.Log("connect time over!!");
        closeAtOnce();
        // TODO : callback error
        yield break;
      }
      yield return 0;
    }

    // callback
    cb();
    wsStatus = socketStatus.Connect;

    // start ping
    Action cbPing = () => {
      Debug.Log("ping error!!");
      closeAtOnce();
    };
    StartCoroutine(ping(cbPing));
  }

  private IEnumerator closeProc(Action cb)
  {
    // close connect
    ws.CloseAsync();
    wsStatus = socketStatus.Closing;

    // waiting
    while (ws.IsAlive)
    {
      yield return new WaitForSeconds(0.1f);
    }

    // callback
    cb();

    ws = null;
    wsStatus = socketStatus.Close;
  }

  private void closeAtOnce() {
    if (ws != null) {
      ws.Close();
      ws = null;
      wsStatus = socketStatus.Close;
    }
  }

  private void onMessage(object obj, MessageEventArgs e) {
    byte[] cmdId = new byte[4];
    byte[] data = new byte[e.RawData.Length - 4];


    Buffer.BlockCopy(e.RawData, 4, data, 0, data.Length);

    var unpack = new ObjectPacker();
    var message = unpack.Unpack<string>(data);
    Debug.Log("message length : " + e.RawData.Length);
    Debug.Log("message : " + message);
  }

  private IEnumerator ping(Action cbError) {
    int count = 0;
    int maxCount = 3;

    while (true) {
      var ok = ws.Ping();
      if (!ok) {
        Debug.Log("ping ng...");
        // callback
        count++;
        if (count >= maxCount) {
          cbError();
          yield break;
        }
      } else {
        count = 0;
        Debug.Log("ping ok...");
      }
      yield return new WaitForSeconds(3);
    }
  }
  */

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

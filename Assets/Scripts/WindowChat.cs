using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowChat : MonoBehaviour {

  public enum TypeChat {
    World,
    Group,
    Private,
    NrTypeChat
  }
  private TypeChat curDispChat;

  [SerializeField]
  private ChatController[] chatControllers;

  [Header("UI")]
  [SerializeField]
  private InputField   inputFieldMessage;
  [SerializeField]
  private Button       buttonSendMessage;
  [SerializeField]
  private Button[]     buttonTabs;
  [SerializeField]
  private GameObject[] imageNotices;

  // save input
  private Dictionary<string, string> inputSaver = new Dictionary<string, string>();

  // callback
  public Action cbLeaveRoom;
  public Action<TypeChat, int, string> cbSendMessage;

  void Start() {
    for (int i = 0; i < chatControllers.Length; i++) {
      chatControllers[i].Initalize();
      chatControllers[i].SetActive(i == 0);
      buttonTabs[i].interactable = (i != 0);
      imageNotices[i].SetActive(false);
    }
    userListBox.SetActive(false);
  }

  public void SetMessage(Data.ChatInfo info, int myUserId) {
    switch ((TypeChat)info.RangeType) {
    case TypeChat.World:
    case TypeChat.Group:
      chatControllers[info.RangeType].UpdateMessageList(info.Name, info.Message);
      break;

    case TypeChat.Private:
      ChatControllerPrivate ccp = (ChatControllerPrivate)chatControllers[info.RangeType];
      if (info.FromId == myUserId) {
        ccp.UpdateMessageList(info.RangeId, info.Name, info.Message, (curUserId == info.RangeId));
      } else {
        ccp.UpdateMessageList(info.FromId, info.Name, info.Message, (curUserId == info.FromId));
      }
      // todo
      break;

    default:
      Debug.LogError("unkwoun type!! " + info.RangeType);
      break;
    }

    if (!chatControllers[info.RangeType].isDispEnable) {
      imageNotices[info.RangeType].SetActive(true);
    }
  }
    
  public void OnChangeValueMessage(string message) {

    message = message.Replace("\r", "").Replace("\n", "");
    inputFieldMessage.text = message;

    // save text
    setInputSaver(message);

    // button interaction
    buttonSendMessage.interactable = (message.Length > 0);
  }

  public void OnButtonSendMessage() {
    string message = inputFieldMessage.text;

    // if type is private, set user_id
    int rangeId = (curDispChat == TypeChat.Private) ? curUserId : -1;

    // callback
    cbSendMessage(curDispChat, rangeId, message);

    // clear text saved.
    inputFieldMessage.text = string.Empty;
    setInputSaver(string.Empty);
  }

  public void OnButtonLeaveRoom() {
    //callback
    cbLeaveRoom();
  }

  public void OnChangeTab(string type) {
    TypeChat t = (TypeChat)Enum.Parse(typeof(TypeChat), type);

    chatControllers[(int)curDispChat].SetActive(false);
    chatControllers[(int)t].SetActive(true);
    buttonTabs[(int)curDispChat].interactable = true;
    buttonTabs[(int)t].interactable = false;
    // set text
    string message = getInputSaver();
    // update
    curDispChat = t;
    OnChangeValueMessage(message);

    // notice
    imageNotices[(int)t].SetActive(false);

    // private only
    userListBox.SetActive(t == TypeChat.Private);
    if (t == TypeChat.Private) {
      inputFieldMessage.interactable = (curUserId > 0);
    } else {
      inputFieldMessage.interactable = true;
    }
  }

  private void setInputSaver(string message) {
    switch (curDispChat) {
    case TypeChat.World:
    case TypeChat.Group:
      inputSaver[curDispChat.ToString()] = message;
      break;

    case TypeChat.Private:
      inputSaver[curDispChat.ToString() + "_" + curUserId.ToString()] = message;
      break;

    default:
      // do nothing
      break;
    }
  }

  private string getInputSaver() {
    string message = string.Empty;
    string key = string.Empty;

    switch (curDispChat) {
    case TypeChat.World:
    case TypeChat.Group:
      key = curDispChat.ToString();
      if (inputSaver.ContainsKey(key)) {
        message = inputSaver[key];
      }
      break;

    case TypeChat.Private:
      key = curDispChat.ToString() + "_" + curUserId.ToString();
      if (inputSaver.ContainsKey(key)) {
        message = inputSaver[key];
      }
      break;

    default:
      // do nothing
      break;
    }
    return message;
  }

  public void ClearRecievedMessage() {
    foreach (ChatController cc in chatControllers) {
      cc.ClearMessage();
    }
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }

  /////////////////////////////////////////////////////////////////////////////////////////////////
  /// for private chat
  /////////////////////////////////////////////////////////////////////////////////////////////////
  [SerializeField]
  private GameObject userListBox;
  [SerializeField]
  private Button buttonLeft;
  [SerializeField]
  private Button buttonRight;
  [SerializeField]
  private Text textUserName;

  private Dictionary<int, string> memberMap = new Dictionary<int, string>();
  private List<int> memberList = new List<int>();

  private int curUserId = -1;
  private int curMemberListIndex = 0;

  public void OnButtonPrivateLeft() {
    updateCurrent(-1);
  }

  public void OnButtonPrivateRight() {
    updateCurrent(1);
  }

  private void updateCurrent(int add) {
    if (memberList.Count < 1) return;

    curMemberListIndex = (curMemberListIndex + memberList.Count + add) % memberList.Count;
    curUserId = memberList[curMemberListIndex];
    textUserName.text = memberMap[curUserId];

    ChatControllerPrivate ccp = (ChatControllerPrivate)chatControllers[(int)TypeChat.Private];
    ccp.SwitchDispMessage(curUserId);

    inputFieldMessage.interactable = true;
  }

  public void UpdateMemberList(Dictionary<int, string> map) {
    memberList.Clear();
    memberMap = map;
    memberList.AddRange(map.Keys);

    // current check
    if (memberMap.ContainsKey(curUserId)) {
      curMemberListIndex = memberList.IndexOf(curUserId);
      inputFieldMessage.interactable = true;
    } else {
      curMemberListIndex = 0;
      curUserId = -1;
      textUserName.text = string.Empty;

      ChatControllerPrivate ccp = (ChatControllerPrivate)chatControllers[(int)TypeChat.Private];
      ccp.ClearMessageDispOnly();

      inputFieldMessage.interactable = false;
    }

    // update button
    buttonLeft.interactable  = (memberList.Count > 0);
    buttonRight.interactable = (memberList.Count > 0);
  }

}

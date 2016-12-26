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

  private TypeChat curDispChat;

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
  }

  public void SetMessage(Manager.ChatInfo info) {
    switch ((TypeChat)info.RangeType) {
    case TypeChat.World:
    case TypeChat.Group:
      chatControllers[info.RangeType].UpdateMessageList(info.Name, info.Message);
      break;

    case TypeChat.Private:
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
    // callback
    cbSendMessage(curDispChat, -1, message);

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
    string message = string.Empty;
    if (inputSaver.ContainsKey(t.ToString())) {
      message = inputSaver[t.ToString()];
    }
    // update
    curDispChat = t;
    OnChangeValueMessage(message);

    // notice
    imageNotices[(int)t].SetActive(false);
  }

  private void setInputSaver(string message) {
    switch (curDispChat) {
    case TypeChat.World:
    case TypeChat.Group:
      inputSaver[curDispChat.ToString()] = message;
      break;

    case TypeChat.Private:
      // todo
      break;

    default:
      // do nothing
      break;
    }
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }
}

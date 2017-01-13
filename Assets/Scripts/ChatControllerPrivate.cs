using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChatControllerPrivate : ChatController {

  private Dictionary<int, List<string>> userMessageMap = new Dictionary<int, List<string>>();
  private Dictionary<int, List<string>> userNameMap    = new Dictionary<int, List<string>>();

  public void UpdateMessageList(int userId, string userName, string message, bool isDisp) {
    checkList(userId);

    List<string> messages = userMessageMap[userId];
    List<string> names    = userNameMap[userId];
    if (messages.Count < listMessage.Count) {
      messages.Add(message);
      names.Add(userName);
    }
    else {
      // old
      for (int i = 0; i < listMessage.Count - 1; i++) {
        messages[i] = messages[i + 1];
        names   [i] = names   [i + 1];
      }

      // new 
      messages[listMessage.Count - 1] = message;
      names   [listMessage.Count - 1] = userName;
    }
    // update
    userMessageMap[userId] = messages;
    userNameMap   [userId] = names;

    // if disp
    if (isDisp) {
      SwitchDispMessage(userId);
    }
  }

  public void SwitchDispMessage(int userId) {
    // check
    checkList(userId);

    List<string> messages = userMessageMap[userId];
    List<string> names = userNameMap[userId];
    for (int i = 0; i < messages.Count; i++) {
      listMessage[i].SetMessage(names[i], messages[i]);
    }
  }

  private void checkList(int userId) {
    if (!userMessageMap.ContainsKey(userId)) {
      userMessageMap[userId] = new List<string>();
      userNameMap   [userId] = new List<string>();
    }
  }

  public override void ClearMessage() {
    base.ClearMessage();
    userMessageMap.Clear();
    userNameMap.Clear();
  }

  public void ClearMessageDispOnly() {
    // UGLY...
    base.ClearMessage();
  }
}

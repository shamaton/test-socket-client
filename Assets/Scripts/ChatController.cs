using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour {

  private const float IntervalHeight = 20f;
  private const int ListSize = 12;
  private int curIndex = 0;

  [SerializeField]
  private GameObject messageBoxBase;

  private List<MessageBox> listMessage = new List<MessageBox>();

  public bool isDispEnable { get { return gameObject.activeSelf; } }

  public void Initalize () {
    // create list
    Transform parent = messageBoxBase.transform.parent;
    Vector3 pos = messageBoxBase.transform.localPosition;
    for (int i = 0; i < ListSize; i++) {
      GameObject o = Instantiate<GameObject>(messageBoxBase);
      o.name = "mb_" + (i + 1).ToString();

      Transform t = o.transform;
      t.SetParent(parent);
      t.localPosition = new Vector3(pos.x, pos.y - IntervalHeight * i, pos.z);

      MessageBox mb = o.GetComponent<MessageBox>();
      mb.SetMessage(string.Empty, string.Empty);
      listMessage.Add(mb);
    }

    // disable base
    messageBoxBase.SetActive(false);
  }

  public void UpdateMessageList(string name, string message) {

    if (curIndex < listMessage.Count) {
      listMessage[curIndex].SetMessage(name, message);
      curIndex++;
    }
    else { // text scroll if text put on all messagebox
      // old
      for (int i = 0; i < listMessage.Count - 1; i++) {
        listMessage[i].SetMessage(listMessage[i + 1].userName, listMessage[i + 1].message);
      }

      // new 
      listMessage[listMessage.Count - 1].SetMessage(name, message);  
    }

  }

  public void ClearMessage() {
    foreach (var m in listMessage) {
      m.SetMessage(string.Empty, string.Empty);
    }
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }
}

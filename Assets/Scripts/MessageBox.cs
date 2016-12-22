using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

  [SerializeField]
  private Text textName;
  [SerializeField]
  private Text textMessage;
  [SerializeField]
  private GameObject colon;

  public string userName { get { return textName.text; } }
  public string message  { get { return textMessage.text; } }

  void Start() {
    // clear text
    //textName.text = string.Empty;
    //textMessage.text = string.Empty;
    //colon.SetActive(false);
  }

  public void SetMessage(string name, string message) {
    textName.text = name;
    textMessage.text = message;
    colon.SetActive(true);
  }
}

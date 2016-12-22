using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowChat : MonoBehaviour {
  [SerializeField]
  private ChatController[] chatControllers;

  public void SetMessage(string name, string message) {
    chatControllers[0].UpdateMessageList(name, message);
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }
}

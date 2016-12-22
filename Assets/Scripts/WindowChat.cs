using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowChat : MonoBehaviour {
  [SerializeField]
  private Text textMessage; // tmp

  public void SetMessage(string message) {
    textMessage.text = message;
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }
}

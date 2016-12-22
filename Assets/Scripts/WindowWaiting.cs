using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowWaiting : MonoBehaviour {

  [SerializeField]
  private Text textStatus;
  [SerializeField]
  private Text textDot;

  private Coroutine cor;

  public void SetActive(bool isActive, string statusStr = "") {
    if (!isActive && cor != null) {
      StopCoroutine(cor);
    }

    gameObject.SetActive(isActive);

    if (isActive) {
      if (statusStr.Length > 0) {
        textStatus.text = statusStr;
      }
      cor = StartCoroutine(updateDot());
    }
  }

  private IEnumerator updateDot() {
    while (true) {
      if (textDot.text.Length < 3) {
        textDot.text += ".";
      }
      else {
        textDot.text = string.Empty;
      }
      yield return new WaitForSeconds(0.5f);
    }
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepWaiting : MonoBehaviour {

  [SerializeField]
  private Text textStatus;
  [SerializeField]
  private Text textDot;

  private Coroutine cor;

  public void SetActive(bool isActive) {
    if (!isActive && cor != null) {
      StopCoroutine(cor);
    }

    gameObject.SetActive(isActive);

    if (isActive) {
      cor = StartCoroutine(updateDot());
    }
  }

  public void SetTextStatus(string statusStr) {
    textStatus.text = statusStr;
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

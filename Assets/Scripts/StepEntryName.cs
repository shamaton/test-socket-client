using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StepEntryName : MonoBehaviour {
  [SerializeField]
  private InputField inputFieldName;
  [SerializeField]
  private ToggleGroup toggleGroupGroupId;
  [SerializeField]
  private Button buttonEntry;

  public string userName       { get { return inputFieldName.text; } }
  public int    groupId        { get; private set; }
  public bool   isStepComplete { get; private set; }

  public void OnChangeValueName(string name) {
    // update button interaction
    bool enableButton = (name.Length > 0);
    buttonEntry.interactable = enableButton;
  }

  public void OnButtonEntry() {
    // group id from toggle
    string label = toggleGroupGroupId.ActiveToggles().First().
      GetComponentsInChildren<Text>().First(t => t.name == "Label").text;
    groupId = (label == "GROUP 1") ? 1 : 2;

    // flag on
    isStepComplete = true;
  }

  public void Reset() {
    isStepComplete = false;
    groupId = 0;
  }

  public void SetActive(bool isActive) {
    gameObject.SetActive(isActive);
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data {

  public struct ChatInfo {
    public int    RangeType;
    public int    RangeId;
    public int    FromId;
    public string Name;
    public string Message;
  }

  public struct StatusInfo {
    public int    UserId;
    public string UserName;
    public int    Status;
  }

  public struct SendMemberInfo {
    public int UserId;
  }
}
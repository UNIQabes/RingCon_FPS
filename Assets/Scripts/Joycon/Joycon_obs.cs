using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joycon_obs
{
    //1フレーム間にJoycon_subjが受け取ったInputReportがまとめて渡される
    public virtual void OnReadReport(List<byte[]> reports)
    {

    }
    //1フレーム間にJoycon_subjが受け取ったInputReportがまとめて渡される
    public virtual void OnDisConnect()
    {

    }
}

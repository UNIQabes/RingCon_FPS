using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeAttackRecordDisp : MonoBehaviour
{
    public string RecordRank="C";
    public float RecordTime=999;
    TextMeshProUGUI _tmp;

    // Start is called before the first frame update
    void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        string colorStr = RecordRank == "S" ? "yellow" : RecordRank == "A" ? "red" : RecordRank == "B" ? "blue" : "green";
        _tmp.text = $"Rank:<color={colorStr}>{RecordRank}</color>\nTime:{(int)RecordTime/60}:{((int)RecordTime % 60).ToString("D2")}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

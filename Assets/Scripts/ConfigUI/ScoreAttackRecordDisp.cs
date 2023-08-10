using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreAttackRecordDisp : MonoBehaviour
{
    public string RecordRank="C";
    public float RecordScore=999;
    TextMeshProUGUI _tmp;

    // Start is called before the first frame update
    void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        string colorStr = RecordRank == "S" ? "yellow" : RecordRank == "A" ? "red" : RecordRank == "B" ? "blue" : "green";
        _tmp.text = $"Rank:<color={colorStr}>{RecordRank}</color>\nScore:{RecordScore}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

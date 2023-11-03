using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreRanking : MonoBehaviour
{
    public List<int> ScoreList;
    TextMeshProUGUI _scoreText;
    // Start is called before the first frame update
    void Start()
    {
        _scoreText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        string DispText="";
        for (int i=0;i<5;i++)
        {
            int DispScore= ScoreList.Count>i? ScoreList [i]: 0;
            DispText += $"<color={(i==0?"yellow":i==1?"red":i==2?"blue":"white")}>{i+1}:{DispScore:0000}</color>\n";
        }
        _scoreText.text = DispText;
    }
}

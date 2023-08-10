using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class HittingGameRuler : MonoBehaviour
{
    public int waveNum;
    public List<GameObject> Waves;
    private List<GameObject>[] marks;
    private List<GameObject> destroyedMarks;
    public TextMeshProUGUI CountDownText;
    public PlayerSettingSetter _playerSettingSetter;
    private float timer;
    public ResultPanel ResultPanel_p;
    public float BRankMaxTime;
    public float ARankMaxTime;
    public float SRankMaxTime;
    public int StageNum;


    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        //destroyedMarks = new List<GameObject>();
        waveNum = 0;
        marks = new List<GameObject>[Waves.Count];
        for (int i=0;i< Waves.Count;i++)
        {
            marks[i] = new List<GameObject>();
            for (int v = 0; v < Waves[i].transform.childCount; v++)
            {
                marks[i].Add(Waves[i].transform.GetChild(v).gameObject);
            }
            Waves[i].SetActive(false);
        }
        FixedUpdateAsync().Forget();
    }

    

    async UniTaskVoid FixedUpdateAsync()
    {
        CancellationToken cToken = this.gameObject.GetCancellationTokenOnDestroy();
        while (waveNum < Waves.Count)
        {
            
            
            Waves[waveNum].SetActive(true);
            
            await UniTask.WaitUntil(() =>
            {
                timer += Time.fixedDeltaTime;
                CountDownText.text = $"TIME:{(int)timer/60}:{((int)timer%60).ToString("D2")}";
                bool clearWave = true;
                foreach (GameObject aMark in marks[waveNum])
                {
                    if (aMark != null)
                    {
                        clearWave = false;
                        break;
                    }
                }
                return clearWave;
            }, PlayerLoopTiming.FixedUpdate, cToken);
            waveNum++;
        }
        ResultPanel_p.resultDetails.Add($"TIME:{(int)timer / 60}:{((int)timer % 60).ToString("D2")}");
        string rank;
        if (SRankMaxTime > timer)
        {
            rank = "S";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=yellow>S</color>");
        }
        else if(ARankMaxTime > timer)
        {
            rank = "A";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=red>A</color>");
        }
        else if (BRankMaxTime > timer)
        {
            rank = "B";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=blue>B</color>");
        }
        else 
        {
            rank = "C";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=green>C</color>");
        }
        if (GetResult(StageNum).recordTime > timer)
        {
            SetResult(StageNum, rank, timer);
        }
        

        ResultPanel_p.gameObject.SetActive(true);
        ResultPanel_p.DispResultDetails();
        Debug.Log("おわりだよ~");
    }

    private (string recordRank,float recordTime) GetResult(int stageNum)
    {
        if (stageNum == 1)
        {
            return (_playerSettingSetter.GetSetting().Rank_TimeAttack1, _playerSettingSetter.GetSetting().Time_TimeAttack1);
        }
        else
        {
            return (_playerSettingSetter.GetSetting().Rank_TimeAttack2, _playerSettingSetter.GetSetting().Time_TimeAttack2);
        }
    }

    private void SetResult(int stageNum,string recordRank,float recordTime)
    {
        if (stageNum == 1)
        {
            _playerSettingSetter.GetSetting().Rank_TimeAttack1 = recordRank;
            _playerSettingSetter.GetSetting().Time_TimeAttack1 = recordTime;
        }
        else
        {
            _playerSettingSetter.GetSetting().Rank_TimeAttack2 = recordRank;
            _playerSettingSetter.GetSetting().Time_TimeAttack2 = recordTime;
        }
    }

    public void OnMarkDestroyed(GameObject destroyedMark)
    {
        Debug.Log("Destroyed");
    }
}

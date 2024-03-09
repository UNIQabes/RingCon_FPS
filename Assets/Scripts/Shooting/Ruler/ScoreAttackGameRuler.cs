using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ScoreAttackGameRuler : MonoBehaviour
{
    public float Score;
    public TextMeshProUGUI ScoreText;
    [HideInInspector]public bool isGameFinished;

    public TextMeshProUGUI CountDownText;
    public PlayerSettingSetter _playerSettingSetter;

    public ResultPanel ResultPanel_p;
    public float BRankMinScore;
    public float ARankMinScore;
    public float SRankMinScore;
    public int StageNum;


    void Start()
    {
        Score = 0;
        Debug.Log($"ScoreText==null:{ScoreText==null}");
        ScoreText.text = $"Score:{(int)Score}";
        CountDownText.gameObject.SetActive(false);
        FixedUpdateAsync().Forget();

    }



    async UniTaskVoid FixedUpdateAsync()
    {
        CancellationToken cToken = this.gameObject.GetCancellationTokenOnDestroy();
        await UniTask.Delay(1000, cancellationToken: cToken);
        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "3";
        await UniTask.Delay(1000, cancellationToken: cToken);
        CountDownText.text = "2";
        await UniTask.Delay(1000, cancellationToken: cToken);
        CountDownText.text = "1";
        await UniTask.Delay(1000, cancellationToken: cToken);
        CountDownText.gameObject.SetActive(false);
        await UniTask.WaitUntil(() =>
        {

            ScoreText.text = $"Score:{(int)Score}";

            return isGameFinished;
        }, PlayerLoopTiming.FixedUpdate, cToken);
        ResultPanel_p.resultDetails.Add($"Score:{(int)Score}");
        string rank;
        if (SRankMinScore < Score)
        {
            rank = "S";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=yellow>S</color>");
        }
        else if (ARankMinScore < Score)
        {
            rank = "A";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=red>A</color>");
        }
        else if (BRankMinScore < Score)
        {
            rank = "B";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=blue>B</color>");
        }
        else
        {
            rank = "C";
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=green>C</color>");
        }
        if (GetResult(StageNum).recordScore < Score)
        {
            SetResult(StageNum, rank, Score);
            ResultPanel_p.resultDetails.Add($"<color=orange>Rank In!!</color>");
        }

        

        ResultPanel_p.gameObject.SetActive(true);
        ResultPanel_p.DispResultDetails();
        Debug.Log("おわりだよ~");
    }

    private (string recordRank, float recordScore) GetResult(int stageNum)
    {
        if (stageNum == 1)
        {
            return (_playerSettingSetter.GetSetting().Rank_ScoreAttack1, _playerSettingSetter.GetSetting().Score_ScoreAttack1);
        }
        else if (stageNum == 2)
        {
            return (_playerSettingSetter.GetSetting().Rank_ScoreAttack2, _playerSettingSetter.GetSetting().Score_ScoreAttack2);
        }
        else if (StageNum == 3) //5番目に高いスコアを返す
        {
            return ("A", _playerSettingSetter.GetSetting().ScoreRanking1[4]);
        }
        else if (StageNum == 4)
        {
            return ("A", _playerSettingSetter.GetSetting().ScoreRanking2[4]);
        }
        return ("X", -10000);
    }

    private void SetResult(int stageNum, string recordRank, float recordScore)
    {
        if (stageNum == 1)
        {
            _playerSettingSetter.GetSetting().Rank_ScoreAttack1 = recordRank;
            _playerSettingSetter.GetSetting().Score_ScoreAttack1 = recordScore;
        }
        else if(stageNum == 2)
        {
            _playerSettingSetter.GetSetting().Rank_ScoreAttack2 = recordRank;
            _playerSettingSetter.GetSetting().Score_ScoreAttack2 = recordScore;
        }
        else if (stageNum == 3)
        {
            int NewRecordRank=6;
            for (int i = 0; i < 5 ; i++)
            {
                if (recordScore >= _playerSettingSetter.GetSetting().ScoreRanking1[i])
                {
                    NewRecordRank = i + 1;
                    break;
                }
            }

            List<int> newRank = new List<int>(_playerSettingSetter.GetSetting().ScoreRanking1);
            newRank.Insert(NewRecordRank - 1, (int)recordScore);
            _playerSettingSetter.GetSetting().ScoreRanking1 = newRank.GetRange(0, 5).ToArray();
        }
        else if (stageNum == 4)
        {
            int NewRecordRank = 6;
            for (int i = 0; i < 5; i++)
            {
                if (recordScore >= _playerSettingSetter.GetSetting().ScoreRanking2[i])
                {
                    NewRecordRank = i + 1;
                    break;
                }
            }

            List<int> newRank = new List<int>(_playerSettingSetter.GetSetting().ScoreRanking2);
            newRank.Insert(NewRecordRank - 1, (int)recordScore);
            _playerSettingSetter.GetSetting().ScoreRanking2 = newRank.GetRange(0, 5).ToArray();
        }
    }

    public void OnMarkDestroyed(GameObject destroyedMark)
    {
        Debug.Log("Destroyed");
    }


}

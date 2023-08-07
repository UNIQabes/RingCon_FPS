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
    private float timer;
    public ResultPanel ResultPanel_p;
    public float BRankMaxTime;
    public float ARankMaxTime;
    public float SRankMaxTime;

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
        if (SRankMaxTime > timer)
        {
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=yellow>S</color>");
        }
        else if(ARankMaxTime > timer)
        {
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=red>A</color>");
        }
        else if (BRankMaxTime > timer)
        {
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=blue>B</color>");
        }
        else 
        {
            ResultPanel_p.resultDetails.Add($"RANK:" + "<color=green>C</color>");
        }

        ResultPanel_p.gameObject.SetActive(true);
        ResultPanel_p.DispResultDetails();
        Debug.Log("おわりだよ~");
    } 

    public void OnMarkDestroyed(GameObject destroyedMark)
    {
        Debug.Log("Destroyed");
    }
}

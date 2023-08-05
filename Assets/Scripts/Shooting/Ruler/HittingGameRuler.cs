using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public class HittingGameRuler : MonoBehaviour
{
    public int waveNum;
    public List<GameObject> Waves;
    private List<GameObject>[] marks;
    private List<GameObject> destroyedMarks;
    // Start is called before the first frame update
    void Start()
    {
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
        UpdateAsync().Forget();
    }

    

    async UniTaskVoid UpdateAsync()
    {
        CancellationToken cToken = this.gameObject.GetCancellationTokenOnDestroy();
        while (waveNum < Waves.Count)
        {
            Waves[waveNum].SetActive(true);

            await UniTask.WaitUntil(() =>
            {
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
            }, PlayerLoopTiming.Update, cToken);
            waveNum++;
        }
        Debug.Log("おわりだよ~");
    } 

    public void OnMarkDestroyed(GameObject destroyedMark)
    {
        Debug.Log("Destroyed");
    }
}

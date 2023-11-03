using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class HardStage : MonoBehaviour
{
    public ScoreAttackGameRuler scoreAttackGameRuler;
    public GameObject testObject;
    public ScoreAttackMark hardMark_SAM;
    public ScoreAttackMark softMark_SAM;
    FallingMover hardMark_FM;
    FallingMover softMark_FM;

    public TextMeshProUGUI CountDownText;

    // Start is called before the first frame update
    void Start()
    {
        hardMark_FM = hardMark_SAM.gameObject.GetComponent<FallingMover>();
        softMark_FM= softMark_SAM.gameObject.GetComponent<FallingMover>();
        hardMark_SAM.scoreAttackGameRuler = scoreAttackGameRuler;
        softMark_SAM.scoreAttackGameRuler = scoreAttackGameRuler;

        FixedUpdateAsync().Forget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async UniTaskVoid FixedUpdateAsync()
    {
        CancellationToken cToken = this.gameObject.GetCancellationTokenOnDestroy();
        await UniTask.Delay(4000, false, PlayerLoopTiming.FixedUpdate, cancellationToken: cToken);
        int markTimeLimit=4000;
        List<GameObject> instantiatedMarks = new List<GameObject>();

        

        hardMark_SAM.DissapearTime = markTimeLimit;
        softMark_SAM.DissapearTime = markTimeLimit;
        for (int i=0;i<5;i++)
        {

            //markPrefab.DissapearTime = markTimeLimit;
            //Vector3 prefabPos= buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
            Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
            instantiatedMarks.Add(Instantiate(softMark_SAM.gameObject, prefabPos, Quaternion.identity));


            //await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cToken);
            await UniTask.WaitUntil(()=> { instantiatedMarks.RemoveAll((o)=>o==null);return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);
        }

        
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "3";
        await UniTask.Delay(1000);
        CountDownText.text = "2";
        await UniTask.Delay(1000);
        CountDownText.text = "1";
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(false);

        markTimeLimit = 10000;
        float markScore = 220;
        hardMark_SAM.DissapearTime = markTimeLimit;
        softMark_SAM.DissapearTime = markTimeLimit;
        hardMark_SAM.score = markScore;
        softMark_SAM.score = markScore;

        for (int i = 0; i < 5; i++)
        {

            //markPrefab.score = markScore;
            //markPrefab.DissapearTime = markTimeLimit;
            for (int v = 0; v < 3; v++)
            {
                //Vector3 prefabPos = buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
                Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
                instantiatedMarks.Add(Instantiate(softMark_SAM.gameObject, prefabPos, Quaternion.identity));
            }
            //await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);
        }

        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "3";
        await UniTask.Delay(1000);
        CountDownText.text = "2";
        await UniTask.Delay(1000);
        CountDownText.text = "1";
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(false);


        markTimeLimit = 4000;
        markScore = 200;
        softMark_SAM.DissapearTime = markTimeLimit;
        softMark_SAM.score = markScore;
        hardMark_SAM.DissapearTime = markTimeLimit;
        hardMark_SAM.score = markScore;
        for (int i = 0; i < 5; i++)
        {

            //markPrefab.DissapearTime = markTimeLimit;
            //markPrefab.score = markScore;
            //Vector3 prefabPos = buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
            Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
            instantiatedMarks.Add(Instantiate(softMark_SAM.gameObject, prefabPos, Quaternion.identity));

            //await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);
            markTimeLimit -= 250;
            markScore += 20;
        }

        //markPrefab.DissapearTime = markTimeLimit;
        //markPrefab.score = markScore;
        //Vector3 LastPrefabPos = buildingWinPosProviders[0].GetWinPos(3, 2);
        Vector3 LastPrefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
        instantiatedMarks.Add(Instantiate(softMark_SAM.gameObject, LastPrefabPos, Quaternion.identity));
        //await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
        await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);

        scoreAttackGameRuler.isGameFinished = true;
    }
    
}

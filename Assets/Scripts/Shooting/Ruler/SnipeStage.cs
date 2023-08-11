using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class SnipeStage : MonoBehaviour
{
    public ScoreAttackGameRuler scoreAttackGameRuler;
    public List<BuildingWinPosProvider> buildingWinPosProviders;
    public GameObject testObject;
    public ScoreAttackMark markPrefab;
    public TextMeshProUGUI CountDownText;

    // Start is called before the first frame update
    void Start()
    {
        markPrefab.scoreAttackGameRuler = scoreAttackGameRuler;
        
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
        markPrefab.DissapearTime = markTimeLimit;
        for (int i=0;i<5;i++)
        {
            
            if (buildingWinPosProviders.Count!=0)
            {
                markPrefab.DissapearTime = markTimeLimit;
                Vector3 prefabPos= buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
                Instantiate(markPrefab, prefabPos,Quaternion.identity);
            }

            await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cToken);
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
        markPrefab.DissapearTime = markTimeLimit;
        for (int i = 0; i < 5; i++)
        {

            if (buildingWinPosProviders.Count != 0)
            {
                markPrefab.score = markScore;
                markPrefab.DissapearTime = markTimeLimit;
                for (int v = 0; v < 3; v++)
                {
                    Vector3 prefabPos = buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
                    Instantiate(markPrefab, prefabPos, Quaternion.identity);
                }
            }
            await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
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
        for (int i = 0; i < 5; i++)
        {

            if (buildingWinPosProviders.Count != 0)
            {
                markPrefab.DissapearTime = markTimeLimit;
                markPrefab.score = markScore;
                Vector3 prefabPos = buildingWinPosProviders[Random.Range(0, buildingWinPosProviders.Count)].GetWinPos(Random.Range(0, 4), Random.Range(0, 6));
                Instantiate(markPrefab, prefabPos, Quaternion.identity);
            }
            await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            markTimeLimit -= 250;
            markScore += 20;
        }
        if (buildingWinPosProviders.Count != 0)
        {
            markPrefab.DissapearTime = markTimeLimit;
            markPrefab.score = markScore;
            Vector3 LastPrefabPos = buildingWinPosProviders[0].GetWinPos(3, 2);
            Instantiate(markPrefab, LastPrefabPos, Quaternion.identity);
            await UniTask.Delay(markTimeLimit, false, PlayerLoopTiming.FixedUpdate, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
        }
            
        scoreAttackGameRuler.isGameFinished = true;
    }
    
}

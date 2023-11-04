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


        //wave1---------
        int SoftMarkScore = 30;
        int HardMarkScore = 100;
        float fallingSpeed = 1;
        int markTimeLimit = (int)(6000/fallingSpeed);
        int MarkAppearInterval = 500;
        List<int> HardMarkAppear = new List<int>(new int[] { 4 ,9});
        List<GameObject> instantiatedMarks = new List<GameObject>();

        softMark_SAM.DissapearTime=hardMark_SAM.DissapearTime = markTimeLimit;
        softMark_FM.FallingSpeed = hardMark_FM.FallingSpeed = fallingSpeed;

        softMark_SAM.score = SoftMarkScore;
        hardMark_SAM.score = HardMarkScore;

        for (int i=0;i<10;i++)
        {
            Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
            instantiatedMarks.Add(Instantiate(HardMarkAppear.Contains(i)?hardMark_SAM.gameObject: softMark_SAM.gameObject, prefabPos, Quaternion.identity));
            await UniTask.Delay(MarkAppearInterval);
        }
        await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);

        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "Speed Up!!";
        await UniTask.Delay(1000);
        CountDownText.text = "3";
        await UniTask.Delay(1000);
        CountDownText.text = "2";
        await UniTask.Delay(1000);
        CountDownText.text = "1";
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(false);


        //wave2---------
        SoftMarkScore = 45;
        HardMarkScore = 150;
        fallingSpeed = 2;
        markTimeLimit = (int)(6000 / fallingSpeed);
        MarkAppearInterval = 500;
        HardMarkAppear = new List<int>(new int[] { 4 });
        instantiatedMarks = new List<GameObject>();

        softMark_SAM.DissapearTime = hardMark_SAM.DissapearTime = markTimeLimit;
        softMark_FM.FallingSpeed = hardMark_FM.FallingSpeed = fallingSpeed;

        softMark_SAM.score = SoftMarkScore;
        hardMark_SAM.score = HardMarkScore;


        for (int i = 0; i < 15; i++)
        {
            Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
            instantiatedMarks.Add(Instantiate(HardMarkAppear.Contains(i) ? hardMark_SAM.gameObject : softMark_SAM.gameObject, prefabPos, Quaternion.identity));
            await UniTask.Delay(MarkAppearInterval);
        }
        await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);

        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "Speed Up!!";
        await UniTask.Delay(1000);
        CountDownText.text = "3";
        await UniTask.Delay(1000);
        CountDownText.text = "2";
        await UniTask.Delay(1000);
        CountDownText.text = "1";
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(false);


        //wave3---------
        SoftMarkScore = 60;
        HardMarkScore = 200;
        fallingSpeed = 3;
        markTimeLimit = (int)(6000 / fallingSpeed);
        MarkAppearInterval = 500;
        HardMarkAppear = new List<int>(new int[] { 4,10,18,29 });
        instantiatedMarks = new List<GameObject>();

        softMark_SAM.DissapearTime = hardMark_SAM.DissapearTime = markTimeLimit;
        softMark_FM.FallingSpeed = hardMark_FM.FallingSpeed = fallingSpeed;

        softMark_SAM.score = SoftMarkScore;
        hardMark_SAM.score = HardMarkScore;


        for (int i = 0; i < 20; i++)
        {
            Vector3 prefabPos = new Vector3(Random.Range(-9, 9), Random.Range(9, 11), -5);
            instantiatedMarks.Add(Instantiate(HardMarkAppear.Contains(i) ? hardMark_SAM.gameObject : softMark_SAM.gameObject, prefabPos, Quaternion.identity));
            await UniTask.Delay(MarkAppearInterval);
        }
        await UniTask.WaitUntil(() => { instantiatedMarks.RemoveAll((o) => o == null); return instantiatedMarks.Count == 0; }, PlayerLoopTiming.FixedUpdate, cToken);

        CountDownText.gameObject.SetActive(true);
        CountDownText.text = "Finish!!";
        await UniTask.Delay(1000);
        CountDownText.gameObject.SetActive(false);

        scoreAttackGameRuler.isGameFinished = true;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ResultPanel : MonoBehaviour
{
    public TextMeshProUGUI ResultDetaileTMP;
    public List<string> resultDetails;
    public GameObject returnButtonGObj;
    public int Duration;

    // Start is called before the first frame update
    void Start()
    {
        resultDetails = new List<string>();
        returnButtonGObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DispResultDetails()
    {
        DispResultDetailsAsync().Forget();
    }
    private async UniTaskVoid DispResultDetailsAsync()
    {
        
        ResultDetaileTMP.text ="";
        foreach (string detail in resultDetails)
        {
            await UniTask.Delay(Duration, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            ResultDetaileTMP.text += detail+"\n";
            
        }
        await UniTask.Delay(Duration, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
        returnButtonGObj.SetActive(true);

    }
}

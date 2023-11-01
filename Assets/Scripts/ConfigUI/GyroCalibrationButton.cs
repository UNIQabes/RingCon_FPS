using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using TMPro;

public class GyroCalibrationButton : MonoBehaviour
{
    private static bool _isSettingCalibration=false;
    public TextMeshProUGUI ButtonText;
    public UnityEvent OnCalibrationFinished = new UnityEvent();

    public void OnClicked()
    {
        SetCalibrationAsync().Forget();
    }

    private async UniTaskVoid SetCalibrationAsync()
    {
        ButtonText.text = "Setting Now";
        if (_isSettingCalibration)//既にキャリブレーションを行っている最中の場合
        {
            await UniTask.WaitUntil(()=>!_isSettingCalibration);
        }
        else
        {
            _isSettingCalibration = true;
            await MainJoyconInput.SetCalibrationWhenStaticCondition();
            OnCalibrationFinished.Invoke();
        }
        ButtonText.text = "Calibration Finished";
        _isSettingCalibration = false;
        
    }


    
}

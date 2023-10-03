using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class GyroCalibrationButton : MonoBehaviour
{
    private static bool _isSettingCalibration=false;
    public TextMeshProUGUI ButtonText;

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
        }
        ButtonText.text = "Start Calibration";
        _isSettingCalibration = false;

    }
    
}

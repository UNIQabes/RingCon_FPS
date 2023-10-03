using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SetCalibrationButton : MonoBehaviour
{
    public void OnClicked()
    {
        MainJoyconInput.SetCalibrationWhenStaticCondition().Forget();
    }
}

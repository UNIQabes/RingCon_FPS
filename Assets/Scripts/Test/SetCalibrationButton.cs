using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCalibrationButton : MonoBehaviour
{
    public void OnClicked()
    {
        MainJoyconInput.SetCalibrationWhenStaticCondition().Forget();
    }
}

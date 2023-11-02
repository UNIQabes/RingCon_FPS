using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleSettingPanel : MonoBehaviour
{
    [SerializeField] PlayerSettingSetter _playerSettingSetter;
    public PlayerSettingSetter SettingSetter { get { return _playerSettingSetter; } }
    [SerializeField] Toggle _isUsingJoyconToggle;
    [SerializeField] Slider _maxXSlider;
    [SerializeField] Slider _maxYSlider;
    [SerializeField] Slider _xOffsetSlider;
    [SerializeField] Slider _gyroClibrationSlider;
    [SerializeField] GyroCalibrationButton _gyroClibrationButton;
    [SerializeField] Slider _xSensitivitySlider;
    [SerializeField] Slider _ySensitivitySlider;
    [SerializeField] GameObject JoyconSettingPanelGObj;
    [SerializeField] GameObject KeyMouseSettingPanelGObj;
    [SerializeField] Button _weak_RConStrengthButton;
    [SerializeField] Button _mod_RConStrengthButton;
    [SerializeField] Button _pow_RConStrengthButton;


    void Start()
    {
        _isUsingJoyconToggle.onValueChanged.AddListener(OnToggleChanged);
        _maxXSlider.onValueChanged.AddListener(OnMaxXChanged);
        _maxYSlider.onValueChanged.AddListener(OnMaxYChanged);
        _xOffsetSlider.onValueChanged.AddListener(OnXOffsetChanged);
        _gyroClibrationSlider.onValueChanged.AddListener(OnGyroClibrationChanged);
        _xSensitivitySlider.onValueChanged.AddListener(OnXSensitivityChanged);
        _ySensitivitySlider.onValueChanged.AddListener(OnYSensitivityChanged);
        _gyroClibrationButton.OnCalibrationFinished.AddListener(OnCalibrationDataChanged);
        _weak_RConStrengthButton.onClick.AddListener(OnWeakButtonPushed);
        _mod_RConStrengthButton.onClick.AddListener(OnModButtonPushed);
        _pow_RConStrengthButton.onClick.AddListener(OnPowButtonPushed);


        /*
        _isUsingJoyconToggle.isOn = (SettingSetter.GetSetting().ControllerMode == ShootingControllerMode.JoyCon);
        _maxXSlider.value = SettingSetter.GetSetting().MaxXRot_xyOrder_deg;
        _maxYSlider.value = SettingSetter.GetSetting().MaxYRot_xyOrder_deg;
        _xOffsetSlider.value = SettingSetter.GetSetting().XRotOffset_xyOrder_deg;
        _xSensitivitySlider.value = SettingSetter.GetSetting().XSensitivity;
        _ySensitivitySlider.value = SettingSetter.GetSetting().YSensitivity;

        JoyconSettingPanelGObj.SetActive(_isUsingJoyconToggle.isOn);
        KeyMouseSettingPanelGObj.SetActive(!_isUsingJoyconToggle.isOn);
        */
    }


    public void OnEnable()
    {
        _isUsingJoyconToggle.isOn = (SettingSetter.GetSetting().ControllerMode == ShootingControllerMode.JoyCon);
        _maxXSlider.value = SettingSetter.GetSetting().MaxXRot_xyOrder_deg;
        _maxYSlider.value = SettingSetter.GetSetting().MaxYRot_xyOrder_deg;
        _xOffsetSlider.value = SettingSetter.GetSetting().XRotOffset_xyOrder_deg;
        _gyroClibrationSlider.value = SettingSetter.GetSetting().GyroDriftCalibration_YRot_xyOrder;
        _xSensitivitySlider.value = SettingSetter.GetSetting().XSensitivity;
        _ySensitivitySlider.value = SettingSetter.GetSetting().YSensitivity;

        //ringconの押し引きの強さの設定をボタンの色を表示
        if (SettingSetter.GetSetting().RingconStrain_mod == 4330 &SettingSetter.GetSetting().RingconThreshold_Push == 5000 &SettingSetter.GetSetting().RingconThreshold_Pull == 3800)
        { SetRingconStrengthButtonColor(0); }
        else if (SettingSetter.GetSetting().RingconStrain_mod == 4330 &SettingSetter.GetSetting().RingconThreshold_Push == 4800 &SettingSetter.GetSetting().RingconThreshold_Pull == 3900)
        { SetRingconStrengthButtonColor(1); }
        else if (SettingSetter.GetSetting().RingconStrain_mod == 4330 &SettingSetter.GetSetting().RingconThreshold_Push == 4700 &SettingSetter.GetSetting().RingconThreshold_Pull == 4000)
        { SetRingconStrengthButtonColor(2); }
        
        JoyconSettingPanelGObj.SetActive(_isUsingJoyconToggle.isOn);
        KeyMouseSettingPanelGObj.SetActive(!_isUsingJoyconToggle.isOn);
    }

    public void OnToggleChanged(bool value)
    {
        SettingSetter.GetSetting().ControllerMode = value ? ShootingControllerMode.JoyCon : ShootingControllerMode.KeyMouse;
        _playerSettingSetter.SetSetting();
        JoyconSettingPanelGObj.SetActive(value);
        KeyMouseSettingPanelGObj.SetActive(!value);
        if (value)
        {
            MainJoyconInput.SetupAgain().Forget();
        }
    }

    public void OnMaxXChanged(float value)
    {
        
        SettingSetter.GetSetting().MaxXRot_xyOrder_deg = (int)value;
        _playerSettingSetter.SetSetting();
    }

    public void OnMaxYChanged(float value)
    {
        SettingSetter.GetSetting().MaxYRot_xyOrder_deg = (int)value;
        _playerSettingSetter.SetSetting();
    }

    public void OnXOffsetChanged(float value)
    {
        SettingSetter.GetSetting().XRotOffset_xyOrder_deg = (int)value;
        _playerSettingSetter.SetSetting();
    }

    public void OnGyroClibrationChanged(float value)
    {
        SettingSetter.GetSetting().GyroDriftCalibration_YRot_xyOrder = value;
        _playerSettingSetter.SetSetting();
    }

    public void OnXSensitivityChanged(float value)
    {
        SettingSetter.GetSetting().XSensitivity = (int)value;
        _playerSettingSetter.SetSetting();
    }

    public void OnYSensitivityChanged(float value)
    {
        SettingSetter.GetSetting().YSensitivity = (int)value;
        _playerSettingSetter.SetSetting();
    }
    public void OnCalibrationDataChanged()
    {
        SettingSetter.GetSetting().GyroXCalibration = MainJoyconInput.GyroXCalibration;
        SettingSetter.GetSetting().GyroYCalibration = MainJoyconInput.GyroYCalibration;
        SettingSetter.GetSetting().GyroZCalibration = MainJoyconInput.GyroZCalibration;
        _playerSettingSetter.SetSetting();
    }

    public void OnPowButtonPushed()
    {
        Debug.Log("Pow");
        SetRingconStrengthButtonColor(0);

        SettingSetter.GetSetting().RingconStrain_mod = 4330;
        SettingSetter.GetSetting().RingconThreshold_Push = 5000;
        SettingSetter.GetSetting().RingconThreshold_Pull = 3800;
        _playerSettingSetter.SetSetting();
    }
    public void OnModButtonPushed()
    {
        Debug.Log("Mod");
        SetRingconStrengthButtonColor(1);

        SettingSetter.GetSetting().RingconStrain_mod = 4330;
        SettingSetter.GetSetting().RingconThreshold_Push = 4800;
        SettingSetter.GetSetting().RingconThreshold_Pull = 3900;
        _playerSettingSetter.SetSetting();
    }
    public void OnWeakButtonPushed()
    {
        Debug.Log("Weak");
        SetRingconStrengthButtonColor(2);

        SettingSetter.GetSetting().RingconStrain_mod = 4330;
        SettingSetter.GetSetting().RingconThreshold_Push = 4700;
        SettingSetter.GetSetting().RingconThreshold_Pull = 4000;
        _playerSettingSetter.SetSetting();
    }


    //便利関数
    public void SetRingconStrengthButtonColor(int ButtonNum)
    {
        ColorBlock cb =　 _pow_RConStrengthButton.colors;
        cb.highlightedColor = (ButtonNum == 0) ? Color.white : Color.yellow;
        cb.normalColor = (ButtonNum ==0)? Color.yellow:Color.white;
        cb.selectedColor = (ButtonNum == 0) ? Color.yellow : Color.white;
        cb.pressedColor = (ButtonNum == 0) ? Color.white : Color.yellow;
        _pow_RConStrengthButton.colors = cb;

        cb = _mod_RConStrengthButton.colors;
        cb.highlightedColor = (ButtonNum == 1) ? Color.white : Color.yellow;
        cb.normalColor = (ButtonNum == 1) ? Color.yellow : Color.white;
        cb.selectedColor = (ButtonNum == 1) ? Color.yellow : Color.white;
        cb.pressedColor = (ButtonNum == 1) ? Color.white : Color.yellow;
        _mod_RConStrengthButton.colors = cb;

        cb = _weak_RConStrengthButton.colors;
        cb.highlightedColor = (ButtonNum == 2) ? Color.white : Color.yellow;
        cb.normalColor = (ButtonNum == 2) ? Color.yellow : Color.white;
        cb.selectedColor = (ButtonNum == 2) ? Color.yellow : Color.white;
        cb.pressedColor = (ButtonNum == 2) ? Color.white : Color.yellow;
        _weak_RConStrengthButton.colors = cb;
    }

}

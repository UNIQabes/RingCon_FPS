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
    [SerializeField] Slider _xSensitivitySlider;
    [SerializeField] Slider _ySensitivitySlider;
    [SerializeField] GameObject JoyconSettingPanelGObj;
    [SerializeField] GameObject KeyMouseSettingPanelGObj;


    void Start()
    {
        _isUsingJoyconToggle.onValueChanged.AddListener(OnToggleChanged);
        _maxXSlider.onValueChanged.AddListener(OnMaxXChanged);
        _maxYSlider.onValueChanged.AddListener(OnMaxYChanged);
        _xOffsetSlider.onValueChanged.AddListener(OnXOffsetChanged);
        _gyroClibrationSlider.onValueChanged.AddListener(OnGyroClibrationChanged);
        _xSensitivitySlider.onValueChanged.AddListener(OnXSensitivityChanged);
        _ySensitivitySlider.onValueChanged.AddListener(OnYSensitivityChanged);

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
}

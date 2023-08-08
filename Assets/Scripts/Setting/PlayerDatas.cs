using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerSetting", menuName = "ScriptableObjects/PlayerDatas")]
public class PlayerDatas : ScriptableObject
{
    //Scores
    public string Rank_HittingStage1 = "C";
    public float Score_HittingStage1 = 999;
    public string Rank_HittingStage2 = "C";
    public float Score_HittingStage2 = 999;
    public string Rank_HittingStage3 = "C";
    public float Score_HittingStage3 = 999;
    public ShootingControllerMode ControllerMode = ShootingControllerMode.KeyMouse;



    //JoyConSettings
    public int MaxXRot_xyOrder_deg = 30;
    public int MaxYRot_xyOrder_deg = 45;
    public int XRotOffset_xyOrder_deg = 0;

    //KeyMouseSettings
    public int XSensitivity=0;
    public int YSensitivity=0;

    [ContextMenu("LoadFromPlayerPrefs")]
    public void LoadFromPlayerPrefs()
    {
        Rank_HittingStage1=PlayerPrefs.GetString("Rank_HittingStage1","C");
        Rank_HittingStage2 = PlayerPrefs.GetString("Rank_HittingStage2", "C");
        Rank_HittingStage3 = PlayerPrefs.GetString("Rank_HittingStage3", "C");

        Score_HittingStage1= PlayerPrefs.GetFloat("Score_HittingStage1", 999);
        Score_HittingStage2 = PlayerPrefs.GetFloat("Score_HittingStage2", 999);
        Score_HittingStage3 = PlayerPrefs.GetFloat("Score_HittingStage3", 999);

        MaxXRot_xyOrder_deg= PlayerPrefs.GetInt("MaxXRot_xyOrder_deg", 30);
        MaxYRot_xyOrder_deg = PlayerPrefs.GetInt("MaxYRot_xyOrder_deg", 45);
        XRotOffset_xyOrder_deg = PlayerPrefs.GetInt("XRotOffset_xyOrder_deg", 0);
        ControllerMode = (ShootingControllerMode)PlayerPrefs.GetInt("ControllerMode", 0);

        XSensitivity = PlayerPrefs.GetInt("XSensitivty", 0);
        YSensitivity = PlayerPrefs.GetInt("YSensitivity", 0);
    }

    [ContextMenu("SaveToPlayerPrefs")]
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetString("Rank_HittingStage1", Rank_HittingStage1);
        PlayerPrefs.SetString("Rank_HittingStage2", Rank_HittingStage2);
        PlayerPrefs.SetString("Rank_HittingStage3", Rank_HittingStage3);

        PlayerPrefs.SetFloat("Score_HittingStage1", Score_HittingStage1);
        PlayerPrefs.SetFloat("Score_HittingStage2", Score_HittingStage2);
        PlayerPrefs.SetFloat("Score_HittingStage3", Score_HittingStage3);

        PlayerPrefs.SetInt("MaxXRot_xyOrder_deg", MaxXRot_xyOrder_deg);
        PlayerPrefs.SetInt("MaxYRot_xyOrder_deg", MaxYRot_xyOrder_deg);
        PlayerPrefs.SetInt("XRotOffset_xyOrder_deg", XRotOffset_xyOrder_deg);

        PlayerPrefs.SetInt("XSensitivty", XSensitivity);
        PlayerPrefs.SetInt("YSensitivity", YSensitivity);

        PlayerPrefs.SetInt("ControllerMode", (int)ControllerMode);

        PlayerPrefs.Save();
    }


}

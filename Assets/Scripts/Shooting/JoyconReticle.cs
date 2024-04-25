using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconReticle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsReady)
        {
            Vector3 ringconFrontV=MainJoyconInput.SmoothedPose_R_Arrow* new Vector3(1, 0, 0);
            Vector3 ringconFrontV_InGame = new Vector3(-ringconFrontV.z,-ringconFrontV.y, ringconFrontV.x);
            /*
            Vector3 ringconFrontV_InGame =V3_MyUtil.rotationWithMatrix(MainJoyconInput.SmoothedPose_R,
            new Vector3(0, 0, 1),
            new Vector3(0, -1, 0),
            new Vector3(-1, 0, 0))*new Vector3(0,0,1);
            */
            float xRot_xyOrder=Mathf.Atan2(ringconFrontV_InGame.y,Mathf.Sqrt(Mathf.Pow(ringconFrontV_InGame.x,2)+ Mathf.Pow(ringconFrontV_InGame.z, 2)));
            float yRot_xyOrder=Mathf.Atan2(ringconFrontV_InGame.x, ringconFrontV_InGame.z);
            /*
            Debug.Log(this.transform.position);
            Debug.Log(xRot_xyOrder*180/Mathf.PI);
            Debug.Log(yRot_xyOrder * 180 / Mathf.PI);
            */
            this.transform.position=new Vector3(604+600* (yRot_xyOrder), 320+ 400 * (xRot_xyOrder), 0);
            //this.transform.position
        }
        
    }
}

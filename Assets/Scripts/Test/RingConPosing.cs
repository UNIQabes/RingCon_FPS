using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingConPosing : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = V3_MyUtil.rotationWithMatrix(MainJoyconInput.SmoothedPose_R_Ring,
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1));
        float strainDelta=MainJoyconInput.ringconStrain-4500;
        this.transform.localScale = new Vector3(1-strainDelta/5000, 1 + strainDelta / 5000, 1);
    }

}

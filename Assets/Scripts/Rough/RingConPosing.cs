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
        this.transform.rotation = V3_MyUtil.rotationWithMatrix(MainJoyconInput.SmoothedPose_R,
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1));
    }
}

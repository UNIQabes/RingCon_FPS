using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetRotSC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (true)
        {
            //MainJoyconInput.ResetYRot_xyOrder();
        }
    }

    private void FixedUpdate()
    {
        //MainJoyconInput.ResetYRot_xyOrder();
    }
    public void OnClicked()
    {
        MainJoyconInput.ResetYRot_xyOrder();
    }
}

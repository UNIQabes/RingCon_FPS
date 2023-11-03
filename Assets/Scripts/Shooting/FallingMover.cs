using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingMover : MonoBehaviour
{
    public float FallingSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position += new Vector3(0, -FallingSpeed*Time.fixedDeltaTime, 0);
    }
}

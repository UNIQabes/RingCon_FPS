using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMover : MonoBehaviour
{
    public float LifeTime;
    public float BulletSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position += this.transform.rotation * new Vector3(0,0,1)* Time.fixedDeltaTime*BulletSpeed;
        LifeTime -= Time.fixedDeltaTime;
        if (LifeTime < 0)
        {
            //Debug.Log("破壊!");
            GameObject.Destroy(this.gameObject);
        }
    }
}

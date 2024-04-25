using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingWinPosProvider : MonoBehaviour
{
    public float XInterval;
    public float YInterval;
    public Vector3 OrigLocalPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetWinPos(int x,int y)
    {
        return this.gameObject.transform.position+ OrigLocalPos + new Vector3(XInterval*x,YInterval*y,0);
    }
}

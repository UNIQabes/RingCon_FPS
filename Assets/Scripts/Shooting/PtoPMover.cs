using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PtoPMover : MonoBehaviour
{
    
    [SerializeField] List<Vector3> _wayPoints;
    [SerializeField] float moveSpeed;
    private int _nextwayPoint=0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 nextWayPoint = _wayPoints[_nextwayPoint];
        float thisFrameDelta = Time.fixedDeltaTime * moveSpeed;
        Vector3 nextWayPointDirection = nextWayPoint-this.transform.position ;
        if (nextWayPointDirection.magnitude < thisFrameDelta)
        {
            this.transform.position = nextWayPoint;
            _nextwayPoint = (_nextwayPoint + 1) % _wayPoints.Count;
        }
        else
        {
            this.transform.position += nextWayPointDirection.normalized* thisFrameDelta;

        }

    }
}

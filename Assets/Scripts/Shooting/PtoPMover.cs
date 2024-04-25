using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PtoPMover : MonoBehaviour
{
    
    [SerializeField] List<Vector3> _wayPoints;
    [SerializeField] float moveSpeed;
    [SerializeField] int NextwayPoint=0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 nextWayPoint = _wayPoints[NextwayPoint];
        float thisFrameDelta = Time.fixedDeltaTime * moveSpeed;
        Vector3 nextWayPointDirection = nextWayPoint-this.transform.position ;
        if (nextWayPointDirection.magnitude < thisFrameDelta)
        {
            this.transform.position = nextWayPoint;
            NextwayPoint = (NextwayPoint + 1) % _wayPoints.Count;
        }
        else
        {
            this.transform.position += nextWayPointDirection.normalized* thisFrameDelta;

        }

    }
}

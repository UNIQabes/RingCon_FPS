using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeCursor : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{collision.gameObject.name}にとりが-!");   
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        
    }
}

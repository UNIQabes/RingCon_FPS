using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AlternativeCursor : MonoBehaviour
{
    protected bool isClicked = false;
    protected bool prevIsClicked = false;

    private void Update()
    {
        if (MainJoyconInput.ConnectInfo==JoyConConnectInfo.JoyConIsReady)
        {
            prevIsClicked = isClicked;
            isClicked = (MainJoyconInput.ringconStrain < 5000);
            
        }
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Button button=collision.gameObject.GetComponent<Button>();

        if (isClicked & !prevIsClicked& button)
        {
            button.onClick.Invoke();
            Debug.Log("invoke");
        }

    }
}

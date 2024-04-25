using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectActiveButton : MonoBehaviour
{

    [SerializeField] GameObject ActivatedObject;

    public void OnClickrd()
    {
        ActivatedObject.SetActive(true);
    }


}

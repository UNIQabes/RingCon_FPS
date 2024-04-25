using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleGraphic : MonoBehaviour
{
    private float prevFilledRate=0;
    private Image _image;
    [SerializeField] private Image _chargeCircle_Image;

    private void Start()
    {
        _image = this.GetComponent<Image>();
    }

    public void SetfilledRate(float setValue)
    {
        _chargeCircle_Image.fillAmount = setValue;
        prevFilledRate = setValue;
    }
    public void SetIsRockedOn(bool setValue)
    {
        if (setValue)
        {
            _image.color = new Color(0, 0, 0, 1);
            _chargeCircle_Image.color = new Color(0.5f, 0, 0, 1);
        }
        else
        {
            _image.color = new Color(0, 0, 0, 0.3f);
            _chargeCircle_Image.color = new Color(0.5f, 0, 0, 0.3f);
        }
    }
    public void SetIsReticleFixed(bool setValue)
    {
        this.transform.localScale = setValue? new Vector3(0.5f, 0.5f, 0.5f) : new Vector3(1,1,1);
    }

}

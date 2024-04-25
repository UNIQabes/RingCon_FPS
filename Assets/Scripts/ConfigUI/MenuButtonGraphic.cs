using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MenuButtonGraphic : MonoBehaviour
{
    private TextMeshProUGUI _tmp;
    private Outline _outline;
    private Image _image;
    private bool isMenuOpening;
    



    // Start is called before the first frame update
    void Start()
    {
        _outline = this.gameObject.GetComponent<Outline>();
        _image = this.gameObject.GetComponent<Image>();
        _tmp = this.transform.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();
        OnPointerExit();
    }

    public void OnPointerEnter()
    {
        _tmp.alpha = 1;
        Color imageColor = _image.color;
        imageColor.a = 1;
        _image.color = imageColor;
        Color outLineColor = _outline.effectColor;
        outLineColor.a = 1;
        _outline.effectColor = outLineColor;
    }
    public void OnPointerExit()
    {
        _tmp.alpha = 0.5f;
        Color imageColor = _image.color;
        imageColor.a = 0.5f;
        _image.color = imageColor;
        Color outLineColor = _outline.effectColor;
        outLineColor.a = 0.5f;
        _outline.effectColor = outLineColor;
    }

   

}

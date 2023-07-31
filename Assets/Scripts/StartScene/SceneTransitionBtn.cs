using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionBtn : MonoBehaviour
{
    public string NextScene;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = this.gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnPushedButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPushedButton()
    {
        SceneManager.LoadScene(NextScene);
    }
}

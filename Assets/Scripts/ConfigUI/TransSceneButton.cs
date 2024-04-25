using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransSceneButton : MonoBehaviour
{
    [SerializeField] string _nextSceneName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClicked()
    {
        SceneManager.LoadScene(_nextSceneName);
        Time.timeScale = 1;
    }
}

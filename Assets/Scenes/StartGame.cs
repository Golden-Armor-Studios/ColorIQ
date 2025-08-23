using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StartGame : MonoBehaviour
{


    void Start()
    {
    }

    
    void Update()
    {
        if (Input.touchCount >= 1) {
            if( Input.touches[0].phase == TouchPhase.Began ){
                StartCoroutine(LoadScene());
            }
        }
    }

    IEnumerator LoadScene() {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync("GameScene");
        while(!loadScene.isDone) {
            yield return null;
        }
    }
}

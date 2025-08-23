using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SlashScreen : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "StartScreen";

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
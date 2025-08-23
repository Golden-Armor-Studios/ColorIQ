
using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    GameBoard gameBoard;
    GameObject ScoreIQ;
    GameObject PlayAgain;
    GameObject Share;
    GameObject ShareIQCanvas;
    Canvas ShareIQCanvasSR;
    Text ScoreIQText;
    float TimeLeft = 60f;

    GameObject timer;
    Text text;

    GameObject score;

    Text scoreText;

    GameObject background;

    SpriteRenderer backgroundSR;

    float lerpCount;

    Color lerpOneColor;
    Color lerpTwoColor;
    
    void Start()
    {
        ScoreIQ = GameObject.Find("IQScore");
        PlayAgain = GameObject.Find("PlayAgain");
        Share = GameObject.Find("Share");
        ShareIQCanvas = GameObject.Find("ShareIQCanvas");
        ScoreIQText = ScoreIQ.GetComponent<Text>();
        Button PlayAgainButton = PlayAgain.GetComponent<Button>();
        Button ShareSRButton = Share.GetComponent<Button>();
        ShareSRButton.enabled = false;
        ShareIQCanvasSR = ShareIQCanvas.GetComponent<Canvas>();
        PlayAgainButton.onClick.AddListener(delegate { playAgain(); });
        ShareSRButton.onClick.AddListener(delegate { StartCoroutine(LoadScene()); });
        gameBoard = new GameBoard(this);
        gameBoard.buildGameBoard();
        timer = GameObject.Find("Timer");
        score = GameObject.Find("Score");
        text = timer.GetComponent<Text>();
        scoreText = score.GetComponent<Text>();
        background = GameObject.Find("Background");
        backgroundSR = background.GetComponent<SpriteRenderer>();
        lerpCount = 0;
        lerpOneColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
        lerpTwoColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
    }

    
    void Update()
    {   
        
        TimeLeft -= Time.deltaTime;
        gameBoard.setTime(TimeLeft);
        text.text = Math.Floor(TimeLeft).ToString();
        lerpCount += Time.deltaTime * 20;
        if (TimeLeft <= 0.0f) {
            ShareIQCanvasSR.enabled = true;
            gameBoard.removeGamePieces();
        }
        TimeLeft = gameBoard.getTime();
        scoreText.text = gameBoard.getScore().ToString();
        ScoreIQText.text = gameBoard.getScore().ToString();
        // Background flashy effec t on level up
        // if (gameBoard.getScore() % 10 == 0 && lerpCount >= 1) {
        //     lerpOneColor = lerpTwoColor;
        //     lerpTwoColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
        //     lerpCount = 0;
        // } else if( gameBoard.getScore() % 10 == 0 && lerpCount <= 1 && gameBoard.getScore() != 0) {
        //     backgroundSR.color = Color.Lerp(lerpOneColor, lerpTwoColor, lerpCount);
        // } else {
        //     backgroundSR.color = Color.clear;
        // }
        if (Input.touchCount >= 1) {
            if( Input.touches[0].phase == TouchPhase.Began ){

                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero);
                if(hit.collider) {
                    gameBoard.addPoints(hit.collider.gameObject.name, hit);
                }  
            }
        }
    }

    void playAgain() {
        ShareIQCanvasSR.enabled = false;
        TimeLeft = 60f;
        scoreText.text = "0";
        ScoreIQText.text = "0";
        gameBoard = new GameBoard(this);
        gameBoard.buildGameBoard();
    }

    IEnumerator LoadScene() {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync("ShareIQScene");
        while(!loadScene.isDone) {
            yield return null;
        }
    }
    
}

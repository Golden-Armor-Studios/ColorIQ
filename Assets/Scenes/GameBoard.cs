using NUnit.Framework.Constraints;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameBoard
{
    public int GameBoardHeight;
    public int GameBoardWidth;

    private MonoBehaviour _coroutineHost;
    
    public List<Color> colorList = new List<Color>();

    public Color selectedColor;

    private int selectedColorId;

    public GameObject SelectedColorIndicator;

    private Color tempColor;

    public int score;

    private float time;

    private int level;

    private Color levelColor;
    private GameObject CorrectPointObject;
    private GameObject InccorectPointObject;
    private GameObject LevelUpObject;
    private AudioSource CorrectPoint;
    private AudioSource IncorrectPoint;
    private AudioSource LevelUp;
    private GameObject VideoBackground;
    private SpriteRenderer VideoBackgroundSR;
    private GameObject SpawnPoint;
    private float gamePieceSize;
    private int gamePieceCount;
    private float gamePieceRadius;
    

    private GameObject[] GamePieces;
    public GameBoard (MonoBehaviour host) {
        _coroutineHost = host;
        GameBoardHeight = 800;
        GameBoardWidth = 600;
        level = 2;
        CorrectPointObject = GameObject.Find("DTSound-Correct_Point");
        InccorectPointObject = GameObject.Find("DTSound-Incorrect_Point");
        LevelUpObject = GameObject.Find("DTSound-Level-Up");
        CorrectPoint = CorrectPointObject.GetComponent<AudioSource>();
        IncorrectPoint = InccorectPointObject.GetComponent<AudioSource>();
        LevelUp = LevelUpObject.GetComponent<AudioSource>();
        VideoBackground = GameObject.Find("Video_Background");
        VideoBackgroundSR = VideoBackground.GetComponent<SpriteRenderer>();
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        float spriteWidth = VideoBackgroundSR.sprite.bounds.size.x;
        float spriteHeight = VideoBackgroundSR.sprite.bounds.size.y;
        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;
        VideoBackgroundSR.transform.localScale = new Vector3(scaleX, scaleY, 1);
        VideoBackgroundSR.transform.position = new Vector3(0, -5, 1);
        SpawnPoint = GameObject.Find("SpawnPoint");
    }

    public void buildGameBoard(bool isTheSameColor = false) {
        colorList = new List<Color>();
        int currentX = 170;
        levelColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);

        switch(level) {
            case 1: {
                gamePieceSize = 0.7f;
                gamePieceCount = 40;
                gamePieceRadius = 0.4f;
                break;
            }
            case 2: {
                gamePieceSize = 0.6f;
                gamePieceCount = 50;
                gamePieceRadius = 0.4f;
                break;
            }
            case 3: {
                gamePieceSize = 0.5f;
                gamePieceCount = 70;
                gamePieceRadius = 0.34f;
                break;
            }
            case 4: {
                gamePieceSize = 0.4f;
                gamePieceCount = 80;
                gamePieceRadius = 0.32f;
                break;
            }
            case 5: {
                gamePieceSize = 0.3f;
                gamePieceCount = 120;
                gamePieceRadius = 0.28f;
                break;
            }
            case 6: {
                gamePieceSize = 0.2f;
                gamePieceCount = 160;
                gamePieceRadius = 0.23f;
                break;
            }
            default: {
                gamePieceSize = 0.1f;
                gamePieceCount = 170;
                gamePieceRadius = 0.19f;
                break;
            }
        }


        for (int i = 0; i < gamePieceCount; i++)
        {   
           Vector3 position = SpawnPoint.transform.position;
           tempColor = isTheSameColor == false ? UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f) : levelColor;
            colorList.Add(tempColor);
            new ColorGamePiece(position, tempColor, level, gamePieceSize, gamePieceRadius).Render();
        }


        selectedColorId = UnityEngine.Random.Range(0, colorList.Count);
        selectedColor = colorList[selectedColorId];
        SelectedColorIndicator = GameObject.Find("SelectedColor");
        SpriteRenderer SelectedColorSR = SelectedColorIndicator.GetComponent<SpriteRenderer>();
        SelectedColorSR.color = selectedColor;
    }

    public void ShotGunEffect(string name, RaycastHit2D hit) {
        float radius = 1f;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(hit.point, radius);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag == "GamePiece") {
                    collider.gameObject.SetActive(false);
                }
            }
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag == "GamePiece") {
                    if (selectedColor.ToHexString() == collider.gameObject.name) {
                        score++;
                        // time isnt adding yet.
                        time += 10.00f;
                        CorrectPoint.Play();
                        if (score % 10 == 0) {
                            level++;
                            LevelUp.Play();
                            VideoBackgroundSR.transform.position = new Vector3(0, -5, 1);
                            removeGamePieces();
                            buildGameBoard(true);
                        } else {
                            VideoBackgroundSR.transform.position = new Vector3(0, VideoBackgroundSR.transform.position.y + 0.5f , 1);
                            removeGamePieces();
                            buildGameBoard();
                        }
                    }
                }
            }
            IncorrectPoint.Play();
    }

    public void addPoints(string name, RaycastHit2D hit) {
            if (level >= 2) {
                ShotGunEffect(name, hit);
            }
            if (level == 1) {
                if (selectedColor.ToHexString() == name) {
                    CorrectPoint.Play();
                    score++;
                    time += 10.00f;
                    if (score % 10 == 0) {
                        level++;
                        LevelUp.Play();
                        VideoBackgroundSR.transform.position = new Vector3(0, -5, 1);
                        removeGamePieces();
                        buildGameBoard(true);
                    } else {
                        VideoBackgroundSR.transform.position = new Vector3(0, VideoBackgroundSR.transform.position.y + 0.5f , 1);
                        removeGamePieces();
                        buildGameBoard();
                    }
                } else {
                    IncorrectPoint.Play();
                    removeGamePieces();
                    buildGameBoard();
                }
            }
        }
        


    public void removeGamePieces() {
        GamePieces = GameObject.FindGameObjectsWithTag("GamePiece");
        foreach( GameObject gamePiece in GamePieces) {
            GameObject.Destroy(gamePiece);
        }
    }

    public void setTime(float timer){
        time = timer;
    }

    public float getTime(){
        return time;
    }

    public int getScore() {
        return score;
    }
}

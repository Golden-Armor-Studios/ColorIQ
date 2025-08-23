using System;
using System.Drawing;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class ColorGamePiece
{
    public Sprite colorSprite;
    private Texture2D texture;
    private Rect size;
    private Vector2 pivot;
    GameObject srGameObject;
    private SpriteRenderer sr;
    private PhysicsMaterial2D material;
    public ColorGamePiece (Vector3 position, UnityEngine.Color selectedColor, int level, float gamePieceSize, float gamePieceRadius) {
        
        texture = Resources.Load<Texture2D>("Circle");
        material = Resources.Load<PhysicsMaterial2D>("Bounce");
        size = new Rect(0.0f, 0.0f, 256.0f, 256.0f);
        pivot = new Vector2(0.5f, 0.5f);
        colorSprite = Sprite.Create(texture, size, pivot);
        colorSprite.name = "circle";
        srGameObject = new GameObject();
        srGameObject.name = selectedColor.ToHexString();
        srGameObject.tag = "GamePiece";
        SpriteRenderer sr = srGameObject.AddComponent<SpriteRenderer>();
        sr.color = selectedColor;
        sr.sprite = colorSprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(gamePieceSize, gamePieceSize);
        sr.transform.Translate(position);
        CircleCollider2D collider = srGameObject.AddComponent<CircleCollider2D>();
        collider.enabled = true;
        collider.isTrigger = false;
        collider.radius = gamePieceRadius;
        Rigidbody2D rigidBody = srGameObject.AddComponent<Rigidbody2D>();
        rigidBody.gravityScale = 1f;
        rigidBody.angularDamping = 0.0f;
        rigidBody.linearDamping = 0.0f;
        rigidBody.AddForce(Vector2.down * 60.0f);
        rigidBody.mass = 0.5f;
        rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidBody.sharedMaterial = material;
        collider.sharedMaterial = material;
    }
    public GameObject Render() {
        return srGameObject;
    }

    void clickColor() {

    }

    void DestroyColor(int delay) {

    }

}

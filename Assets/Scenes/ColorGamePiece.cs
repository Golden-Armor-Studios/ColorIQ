using UnityEngine;

public class ColorGamePiece
{
    private const string CircleSpritePath = "Circle";
    private const string Level2TilePath = "Sprites/level-2-tile";

    private readonly GameObject srGameObject;

    public ColorGamePiece(Vector3 position, Color selectedColor, int level, float gamePieceSize, float gamePieceRadius, bool isLevelTile)
    {
        if (!isLevelTile)
        {
            Texture2D texture = Resources.Load<Texture2D>(CircleSpritePath);
            if (texture == null)
            {
                Debug.LogError($"Failed to load circle texture at Resources/{CircleSpritePath}");
                srGameObject = new GameObject("MissingCircleTexture");
                srGameObject.transform.position = position;
                return;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            sprite.name = "circle";

            srGameObject = new GameObject(selectedColor.ToColorKey());
            srGameObject.tag = "GamePiece";

            SpriteRenderer renderer = srGameObject.AddComponent<SpriteRenderer>();
            renderer.color = selectedColor;
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(gamePieceSize, gamePieceSize);

            srGameObject.transform.position = position;

            CircleCollider2D collider = srGameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = false;
            collider.radius = gamePieceRadius;
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(Level2TilePath);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load level 2 tile prefab at Resources/{Level2TilePath}");
                srGameObject = new GameObject("MissingLevel2Tile");
                srGameObject.transform.position = position;
                return;
            }

            srGameObject = Object.Instantiate(prefab);
            srGameObject.name = selectedColor.ToColorKey();
            srGameObject.tag = "GamePiece_Level_2";
            srGameObject.transform.position = position;
            srGameObject.transform.rotation = Quaternion.identity;
            srGameObject.transform.localScale = Vector3.one;

            SpriteRenderer renderer = srGameObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = selectedColor;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(gamePieceSize, gamePieceSize);
                renderer.sortingOrder = 1;
            }

            CircleCollider2D collider = srGameObject.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = srGameObject.AddComponent<CircleCollider2D>();
            }
            collider.isTrigger = false;
            collider.radius = gamePieceRadius;

            Rigidbody2D rigidBody = srGameObject.GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                rigidBody.linearVelocity = Vector2.zero;
                rigidBody.angularVelocity = 0f;
                rigidBody.gravityScale = 0f;
                rigidBody.bodyType = RigidbodyType2D.Kinematic;
                rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    public GameObject Render()
    {
        return srGameObject;
    }
}

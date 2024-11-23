using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking;

public class TextureLoad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //StartCoroutine(DownloadImage($"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png"));
    }

    public int zoom;
    public int x;
    public int y;

    public float scaleX;
    public float scaleY;

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DownloadImage(string MediaUrl) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error downloading image: {request.error}");
        }
        else {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            // Create a Sprite from the texture
            float textureAspect = texture.width / (float)texture.height;
            float objectAspect = scaleX / scaleY;
            float xScale;
            float yScale;
            if (textureAspect > objectAspect) {
                // Texture is wider; scale Y to fit
                xScale = 1f;
                yScale = objectAspect / textureAspect;
            } else {
                // Texture is taller; scale X to fit
                xScale = textureAspect / objectAspect;
                yScale = 1f;
            }
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f) // Pivot at the center
            );
            SpriteRenderer m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_SpriteRenderer.sprite = sprite;
            //transform.localScale = new Vector3(texture.width, texture.height, 1);
            // Assign the sprite to the SpriteRenderer
        }
    } 
}

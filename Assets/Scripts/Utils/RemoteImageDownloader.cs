using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RemoteImageDownloader : MonoBehaviour
{
    // Public method to download image and set it to the UI Image component
    public void LoadImage(string url, Image imageComponent)
    {
        if (imageComponent == null)
        {
            Debug.LogError("Image component is null. Please assign a valid Image component.");
            return;
        }

        StartCoroutine(DownloadAndDisplayImage(url, imageComponent));
    }

    private IEnumerator DownloadAndDisplayImage(string url, Image imageComponent)
    {
        // Start downloading the image
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            Debug.Log("Downloading image: " + url);
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading image from URL: {url}. Error: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Response Headers: {webRequest.GetResponseHeaders()}");
                yield break;
            }
            else
            {
                // Get the downloaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                if (texture == null)
                {
                    Debug.LogError("Downloaded texture is null.");
                    yield break;
                }

                // Set the texture to the UI Image component
                imageComponent.sprite = TextureToSprite(texture);
            }
        }
    }

    // Helper method to convert Texture2D to Sprite
    private Sprite TextureToSprite(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null. Cannot convert to sprite.");
            return null;
        }

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

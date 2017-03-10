using UnityEngine;

public static class TextureGenerator {

	public static Texture2D GenerateTexture (int width, int height, Tile [,] tiles)
    {
        Texture2D texture = new Texture2D (width, height);
        Color [] pixels = new Color [width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = tiles [x, y].heightValue;
                pixels [x + y * width] = Color.Lerp (Color.black, Color.white, value);
            }
        }

        texture.SetPixels (pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply ();

        return texture;
    }

}

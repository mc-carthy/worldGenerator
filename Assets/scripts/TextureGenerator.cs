using UnityEngine;

public static class TextureGenerator {

	// Height Map Colors
	private static Color DeepColor = new Color (0f, 0f, 0.5f, 1f);
	private static Color ShallowColor = new Color (25f / 255f, 25f / 255f, 150f / 255f, 1f);
	private static Color SandColor = new Color (240f / 255f, 240f / 255f, 64f / 255f, 1f);
	private static Color GrassColor = new Color (50f / 255f, 220f / 255f, 20f / 255f, 1f);
	private static Color ForestColor = new Color (16f / 255f, 160f / 255f, 0f, 1f);
	private static Color RockColor = new Color (0.5f, 0.5f, 0.5f, 1f);
	private static Color SnowColor = new Color (1f, 1f, 1f, 1f);

	public static Texture2D GenerateHeightMapTexture (int width, int height, Tile [,] tiles)
    {
        Texture2D texture = new Texture2D (width, height);
        Color [] pixels = new Color [width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // float value = tiles [x, y].heightValue;
                // pixels [x + y * width] = Color.Lerp (Color.black, Color.white, value);
				switch (tiles [x, y].heightType)
				{
				case HeightType.DeepWater:
					pixels[x + y * width] = DeepColor;
					break;
				case HeightType.ShallowWater:
					pixels[x + y * width] = ShallowColor;
					break;
				case HeightType.Sand:
					pixels[x + y * width] = SandColor;
					break;
				case HeightType.Grass:
					pixels[x + y * width] = GrassColor;
					break;
				case HeightType.Forest:
					pixels[x + y * width] = ForestColor;
					break;
				case HeightType.Rock:
					pixels[x + y * width] = RockColor;
					break;
				case HeightType.Snow:
					pixels[x + y * width] = SnowColor;
					break;
				}

                // Darken edges
                if (tiles[x,y].bitmask != 15)
                {
					pixels [x + y * width] = Color.Lerp (pixels[x + y * width], Color.black, 0.4f);
                }

                // pixels [x + y * width] = (tiles [x, y].isCollidable) ? Color.green : Color.blue;
            }
        }

        texture.SetPixels (pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply ();

        return texture;
    }

    public static Texture2D GenerateHeatMapTexture (int width, int height, Tile [,] tiles)
    {
        Texture2D texture = new Texture2D (width, height);
        Color [] pixels = new Color [width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels [x + width * y] = Color.Lerp (Color.blue, Color.red, tiles [x, y].heatValue);

                // Darken edges
                if (tiles[x,y].bitmask != 15)
                {
					pixels [x + y * width] = Color.Lerp (pixels[x + y * width], Color.black, 0.4f);
                }
            }
        }

        texture.SetPixels (pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply ();

        return texture;
    }

}

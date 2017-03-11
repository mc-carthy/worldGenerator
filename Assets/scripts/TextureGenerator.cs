using UnityEngine;

public static class TextureGenerator {

	// Height Map Colors
	private static Color DeepColour = new Color (0f, 0f, 0.5f, 1f);
	private static Color ShallowColour = new Color (25f / 255f, 25f / 255f, 150f / 255f, 1f);
	private static Color SandColour = new Color (240f / 255f, 240f / 255f, 64f / 255f, 1f);
	private static Color GrassColour = new Color (50f / 255f, 220f / 255f, 20f / 255f, 1f);
	private static Color ForestColour = new Color (16f / 255f, 160f / 255f, 0f, 1f);
	private static Color RockColour = new Color (0.5f, 0.5f, 0.5f, 1f);
	private static Color SnowColour = new Color (1f, 1f, 1f, 1f);

    // Heat Map Colors
    private static Color ColdestColour = new Color (0f, 1f, 1f, 1f);
    private static Color ColderColour = new Color (170f / 255f, 1f, 1f, 1f);
    private static Color ColdColour = new Color (0f, 229f / 255f, 133f / 255f, 1f);
    private static Color WarmColour = new Color (1f, 1f, 100f / 255f, 1f);
    private static Color WarmerColour = new Color (1f, 100f / 255f, 0f, 1f);
    private static Color WarmestColour = new Color (241f / 255f, 12f / 255f, 0f, 1f);

	public static Texture2D GenerateHeightMapTexture (int width, int height, Tile [,] tiles)
    {
        Texture2D texture = new Texture2D (width, height);
        Color [] pixels = new Color [width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // pixels [x + y * width] = Color.Lerp (Color.black, Color.white, tiles [x, y].heightValue);

				switch (tiles [x, y].heightType)
				{
				case HeightType.DeepWater:
					pixels[x + y * width] = DeepColour;
					break;
				case HeightType.ShallowWater:
					pixels[x + y * width] = ShallowColour;
					break;
				case HeightType.Sand:
					pixels[x + y * width] = SandColour;
					break;
				case HeightType.Grass:
					pixels[x + y * width] = GrassColour;
					break;
				case HeightType.Forest:
					pixels[x + y * width] = ForestColour;
					break;
				case HeightType.Rock:
					pixels[x + y * width] = RockColour;
					break;
				case HeightType.Snow:
					pixels[x + y * width] = SnowColour;
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
                // pixels [x + width * y] = Color.Lerp (Color.blue, Color.red, tiles [x, y].heatValue);

                switch (tiles [x, y].heatType)
                {
                    case HeatType.Coldest:
                        pixels [x + y * width] = ColdestColour;
                        break;
                    case HeatType.Colder:
                        pixels [x + y * width] = ColderColour;
                        break;
                    case HeatType.Cold:
                        pixels [x + y * width] = ColdColour;
                        break;
                    case HeatType.Warm:
                        pixels [x + y * width] = WarmColour;
                        break;
                    case HeatType.Warmer:
                        pixels [x + y * width] = WarmerColour;
                        break;
                    case HeatType.Warmest:
                        pixels [x + y * width] = WarmestColour;
                        break;
                }

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

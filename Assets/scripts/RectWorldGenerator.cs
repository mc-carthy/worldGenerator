using UnityEngine;
using AccidentalNoise;

public class RectWorldGenerator : Generator  {
		
	protected ImplicitFractal heightMap;
	protected ImplicitCombiner heatMap;
	protected ImplicitFractal moistureMap;

	protected override void Initialise()
	{

		if (useRandomSeed)
		{
			seed = (int) System.DateTime.Now.Ticks;
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());
        
        // HeightMap
        heightMap = new ImplicitFractal (
            FractalType.MULTI, 
            BasisType.SIMPLEX,
            InterpolationType.QUINTIC, 
            terrainOcatves, 
            terrainFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );
				
        // Heat Map
		ImplicitGradient gradient  = new ImplicitGradient (1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
		ImplicitFractal heatFractal = new ImplicitFractal(
            FractalType.MULTI, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            heatOctaves, 
            heatFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );
		
		heatMap = new ImplicitCombiner (CombinerType.MULTIPLY);
		heatMap.AddSource (gradient);
		heatMap.AddSource (heatFractal);
		
		// Moisture Map
		moistureMap = new ImplicitFractal (
            FractalType.MULTI, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            moistureOctaves, 
            moistureFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );	
	}

	protected override void GetData()
    {
        heightData = new MapData (width, height);
        heatData = new MapData (width, height);
        moistureData = new MapData (width, height);

        // Get height data
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Noise range
                float x1 = 0, x2 = 1;
                float y1 = 0, y2 = 1;

                float dx = x2 - x1;
                float dy = y2 - y1;

                // Get samples at smaller intervals
                float s = x / (float) width;
                float t = y / (float) height;

                // Calculate 4D coords
                float nx = x1 + Mathf.Cos (s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float ny = y1 + Mathf.Cos (t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                float nz = x1 + Mathf.Sin (s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float nw = y1 + Mathf.Sin (t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);

                float heightValue = (float) heightMap.Get (nx, ny, nz, nw);
                float heatValue = (float) heatMap.Get (nx, ny, nz, nw);
				float moistureValue = (float) moistureMap.Get (nx, ny, nz, nw);

                // Keep track of the min/max values
                heightData.max = (heightValue > heightData.max) ? heightValue : heightData.max;
                heightData.min = (heightValue < heightData.min) ? heightValue : heightData.min;

                heatData.max = (heatValue > heatData.max) ? heatValue : heatData.max;
                heatData.min = (heatValue < heatData.min) ? heatValue : heatData.min;

                moistureData.max = (moistureValue > moistureData.max) ? moistureValue : moistureData.max;
                moistureData.min = (moistureValue < moistureData.min) ? moistureValue : moistureData.min;

                heightData.data [x, y] = heightValue;
                heatData.data [x, y] = heatValue;
                moistureData.data [x, y] = moistureValue;
            }
        }
    }
	
    protected override Tile GetRight (Tile t)
	{
		return tiles [MathHelper.Mod (t.x + 1, width), t.y];
	}
	protected override Tile GetTop (Tile t)
	{
		return tiles [t.x, MathHelper.Mod (t.y - 1, height)];
	}
	protected override Tile GetLeft (Tile t)
	{
		return tiles [MathHelper.Mod (t.x - 1, width), t.y];
	}
	protected override Tile GetBottom (Tile t)
	{
		return tiles [t.x, MathHelper.Mod (t.y + 1, height)];
	}
}
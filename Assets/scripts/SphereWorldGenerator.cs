using UnityEngine;
using AccidentalNoise;

public class SphereWorldGenerator : Generator {
		
	private MeshRenderer sphere;
    private MeshRenderer atmosphere1;
    private MeshRenderer atmosphere2;
    private MeshRenderer bumpTexture;
    private MeshRenderer paletteTexture;

    protected ImplicitFractal heightMap;
	protected ImplicitFractal heatMap;
	protected ImplicitFractal moistureMap;
    protected ImplicitFractal cloud1Map;
    protected ImplicitFractal cloud2Map;

	protected override void Instantiate()
	{
		base.Instantiate ();
		sphere = transform.Find ("globe").Find ("sphere").GetComponent<MeshRenderer> ();
        atmosphere1 = transform.Find ("globe").Find ("atmosphere1").GetComponent<MeshRenderer> ();
        atmosphere2 = transform.Find ("globe").Find ("atmosphere2").GetComponent<MeshRenderer> ();

        bumpTexture = transform.Find ("bumpTexture").GetComponent<MeshRenderer>();
        paletteTexture = transform.Find ("paletteTexture").GetComponent<MeshRenderer>();
    }

	protected override void Generate()
	{
		base.Generate ();

        Texture2D bumpTexture = TextureGenerator.GetBumpMap (width, height, tiles);
		Texture2D normal = TextureGenerator.CalculateNormalMap (bumpTexture, 3);

		sphere.materials [0].mainTexture = biomeMapRenderer.materials[0].mainTexture;
		sphere.GetComponent<MeshRenderer> ().materials [0].SetTexture ("_BumpMap", normal);
		sphere.GetComponent<MeshRenderer> ().materials [0].SetTexture ("_ParallaxMap", heightMapRenderer.materials[0].mainTexture);

        atmosphere1.materials[0].mainTexture = TextureGenerator.GetCloud1Texture (width, height, tiles);
        atmosphere2.materials [0].mainTexture = TextureGenerator.GetCloud2Texture (width, height, tiles); 

        this.bumpTexture.materials[0].mainTexture = atmosphere1.materials[0].mainTexture;
        paletteTexture.materials[0].mainTexture = atmosphere2.materials[0].mainTexture;
    }

	protected override void Initialise()
	{

        if (useRandomSeed)
		{
			seed = (int) System.DateTime.Now.Ticks;
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());

		heightMap = new ImplicitFractal (
            FractalType.MULTI, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            terrainOcatves, 
            terrainFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );		
		
		heatMap = new ImplicitFractal(
            FractalType.MULTI, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            heatOctaves, 
            heatFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );
		
		moistureMap = new ImplicitFractal (
            FractalType.MULTI, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            moistureOctaves, 
            moistureFrequency, 
            pseudoRandom.Next (0, int.MaxValue)
        );

        cloud1Map = new ImplicitFractal(
            FractalType.BILLOW,
            BasisType.SIMPLEX,
            InterpolationType.QUINTIC,
            4,
            1.55f,
            pseudoRandom.Next (0, int.MaxValue)
        );

        cloud2Map = new ImplicitFractal (
            FractalType.BILLOW, 
            BasisType.SIMPLEX, 
            InterpolationType.QUINTIC, 
            5, 
            1.75f, 
            pseudoRandom.Next (0, int.MaxValue)
        );
	}

	protected override void GetData()
	{
		heightData = new MapData (width, height);
		heatData = new MapData (width, height);
		moistureData = new MapData (width, height);
		clouds1 = new MapData (width, height);
        clouds2 = new MapData (width, height);

        // Define our map area in latitude/longitude
        float southLatBound = -180;
		float northLatBound = 180;
		float westLonBound = -90;
		float eastLonBound = 90; 
		
		float lonExtent = eastLonBound - westLonBound;
		float latExtent = northLatBound - southLatBound;
		
		float xDelta = lonExtent / (float) width;
		float yDelta = latExtent / (float) height;
		
		float curLon = westLonBound;
		float curLat = southLatBound;
		
        // Loop through each tile using its lat/long coordinates
		for (var x = 0; x < width; x++)
        {	
			curLon = westLonBound;
			for (var y = 0; y < height; y++)
            {
				float x1 = 0, y1 = 0, z1 = 0;
				
                // Convert this lat/lon to x/y/z
				LatLonToXYZ (curLat, curLon, ref x1, ref y1, ref z1);

                // Heat data
				float sphereValue = (float) heatMap.Get (x1, y1, z1);					
				if (sphereValue > heatData.max)
                {
					heatData.max = sphereValue;
                }
				if (sphereValue < heatData.min)
                {
					heatData.min = sphereValue;
                }
				heatData.data [x, y] = sphereValue;
				
				float coldness = Mathf.Abs (curLon) / 90f;
				float heat = 1 - Mathf.Abs (curLon) / 90f;				
				heatData.data [x, y] += heat;
				heatData.data [x, y] -= coldness;
				
                // Height Data
				float heightValue = (float) heightMap.Get (x1, y1, z1);
				if (heightValue > heightData.max)
                {
					heightData.max = heightValue;
                }
				if (heightValue < heightData.min)
                {
					heightData.min = heightValue;				
                }
				heightData.data [x, y] = heightValue;
				
				// Moisture Data
				float moistureValue = (float) moistureMap.Get (x1, y1, z1);
				if (moistureValue > moistureData.max)
                {
					moistureData.max = moistureValue;
                }
				if (moistureValue < moistureData.min)
                {
					moistureData.min = moistureValue;				
                }
				moistureData.data [x, y] = moistureValue;

                // Cloud Data
				clouds1.data [x,y] = (float) cloud1Map.Get (x1, y1, z1);
				if (clouds1.data [x,y] > clouds1.max)
                {
					clouds1.max = clouds1.data [x,y];
                }
				if (clouds1.data [x,y] < clouds1.min)
                {
					clouds1.min = clouds1.data [x,y];
                }

                clouds2.data [x, y] = (float) cloud2Map.Get (x1, y1, z1);
                if (clouds2.data [x, y] > clouds2.max)
                {
                    clouds2.max = clouds2.data [x, y];
                }
                if (clouds2.data [x, y] < clouds2.min)
                {
                    clouds2.min = clouds2.data [x, y];
                }

                curLon += xDelta;
			}			
			curLat += yDelta;
		}
	}
    
	// Convert Lat/Long coordinates to x/y/z for spherical mapping
	void LatLonToXYZ (float lat, float lon, ref float x, ref float y, ref float z)
	{
		float r = Mathf.Cos (Mathf.Deg2Rad * lon);
		x = r * Mathf.Cos (Mathf.Deg2Rad * lat);
		y = Mathf.Sin (Mathf.Deg2Rad * lon);
		z = r * Mathf.Sin (Mathf.Deg2Rad * lat);
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

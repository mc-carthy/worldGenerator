using UnityEngine;
using AccidentalNoise;

public class Generator : MonoBehaviour {

	// Adjustable variables
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    // TODO
    [SerializeField]
    private int terrainOcatves;
    // TODO
    [SerializeField]
    private double terrainFrequency;
	[SerializeField]
	private int seed;
	[SerializeField]
	private bool useRandomSeed;


    private ImplicitFractal heightMap;
    private MapData heightData;
    private Tile [,] tiles;
    private MeshRenderer heightMapRenderer;

    private void Start ()
    {
        heightMapRenderer = transform.Find ("heightTexture").GetComponent<MeshRenderer> ();
        Initialise ();
        GetData (heightMap, ref heightData);
        LoadTiles ();

        heightMapRenderer.materials [0].mainTexture = TextureGenerator.GenerateTexture (width, height, tiles);
    }

    private void Initialise ()
    {
		if (useRandomSeed)
		{
			seed = (int) Time.time;
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
    }

    // Get data from noise module
    private void GetData (ImplicitModuleBase module, ref MapData mapData)
    {
        mapData = new MapData (width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get samples at smaller intervals
                float x1 = x / (float) width;
                float y1 = y / (float) height;

                float value = (float) heightMap.Get (x1, y1);

                // Keep track of the min/max values
                mapData.max = (value > mapData.max) ? value : mapData.max;
                mapData.min = (value < mapData.min) ? value : mapData.min;

                mapData.data [x, y] = value;
            }
        }
    }

    // Build Tile array based on heightData
    private void LoadTiles ()
    {
        tiles = new Tile [width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = new Tile ();
                t.x = x;
                t.y = y;

                float value = heightData.data [x, y];

                // Normalize value between 0 and 1
                value = (value - heightData.min) / (heightData.max - heightData.min);

                t.heightValue = value;

                tiles [x, y] = t;
            }
        }
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
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

    [HeaderAttribute ("Biome Colours")]
	[SerializeField]
	private float DeepWater = 0.2f;
	[SerializeField]
	private float ShallowWater = 0.4f;	
	[SerializeField]
	private float Sand = 0.5f;
	[SerializeField]
	private float Grass = 0.7f;
	[SerializeField]
	private float Forest = 0.8f;
	[SerializeField]
	private float Rock = 0.9f;


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

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
        }
    }

    private void Initialise ()
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

				//HeightMap Analyze
				if (value < DeepWater)
                {
					t.heightType = HeightType.DeepWater;
				}
				else if (value < ShallowWater)
                {
					t.heightType = HeightType.ShallowWater;
				}
				else if (value < Sand)
                {
					t.heightType = HeightType.Sand;
				}
				else if (value < Grass)
                {
					t.heightType = HeightType.Grass;
				}
				else if (value < Forest)
                {
					t.heightType = HeightType.Forest;
				}
				else if (value < Rock)
                {
					t.heightType = HeightType.Rock;
				}
				else
                {
					t.heightType = HeightType.Snow;
				}

                tiles [x, y] = t;
            }
        }
    }

}

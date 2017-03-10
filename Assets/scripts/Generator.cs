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

                float value = (float) heightMap.Get (nx, ny, nz, nw);

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

    private void UpdateNeighbours ()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = tiles [x, y];

                t.right = GetRight (t);
                t.top = GetTop (t);
                t.left = GetLeft (t);
                t.bottom = GetBottom (t);
            }
        }
    }

    // Get Tile neighbours
    private Tile GetRight (Tile t)
    {
        return tiles [MathHelper.Mod (t.x + 1, width), t.y];
    }
    private Tile GetTop (Tile t)
    {
        return tiles [t.y, MathHelper.Mod (t.y - 1, height)];
    }
    private Tile GetLeft (Tile t)
    {
        return tiles [MathHelper.Mod (t.x - 1, width), t.y];
    }
    private Tile GetBottom (Tile t)
    {
        return tiles [t.y, MathHelper.Mod (t.y + 1, height)];
    }

}

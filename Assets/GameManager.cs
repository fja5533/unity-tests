using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance {
        get {
            if(_instance == null) {
                Debug.LogError("GameManager is null");
            }
            return _instance;
        }
    }
    void Awake() {
        if(_instance) {
            Destroy(gameObject);
        }
        else {
            _instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public GameObject tile;

    public GameObject player;

    public Vector2 tile_Coords;

    private int buffer = 1;
    private float tileSize = 1;

    private int zoom = 12;

    public int ViewDistance = 6;

    private List<GameObject> chunks = new List<GameObject>();
    Dictionary<Vector2, GameObject> chunkAddresses = new Dictionary<Vector2, GameObject>();

    private void GenerateGrid() {
        Color niceRed = new Color(255f/255,121f/255,86f/255, 1f);
        Color niceGreen = new Color(156f/255,255f/255,100f/255, 1f);
        Color niceBlue = new Color(124f/255,197f/255,255f/255, 1f);
        List<Color> colors = new List<Color>
        {
            niceBlue,
            niceGreen,
            niceRed
        };

        for(int row = (int)player.transform.position.y; row < (int)player.transform.position.y+buffer; row++) {
            for(int col = (int)player.transform.position.x; col < (int)player.transform.position.x+buffer; col++) {
                GameObject currentTile = Instantiate(tile, transform);
                float posX = col * tileSize - buffer/2;
                float posY = row * tileSize - buffer/2;
                currentTile.transform.position = new Vector2(posX, posY);
                SpriteRenderer m_SpriteRenderer = currentTile.GetComponent<SpriteRenderer>();
                m_SpriteRenderer.color = colors[UnityEngine.Random.Range(0,3)];
                chunkAddresses[currentTile.transform.position] = currentTile;
                chunks.Add(currentTile);
            }
        }
    }

    private void GenerateGridReal() {
        Color niceRed = new Color(255f/255,121f/255,86f/255, 1f);
        Color niceGreen = new Color(156f/255,255f/255,100f/255, 1f);
        Color niceBlue = new Color(124f/255,197f/255,255f/255, 1f);
        List<Color> colors = new List<Color>
        {
            niceBlue,
            niceGreen,
            niceRed
        };
        
        Vector2 playerPoint = convertGPSToPosition(player.transform.position.x/100, player.transform.position.y/100);
        tile_Coords = playerPoint;
        float scaleX = Math.Abs(Math.Abs(convertPositionToGPS((int)playerPoint.x, (int)playerPoint.y).x)-Math.Abs(convertPositionToGPS((int)playerPoint.x+1, (int)playerPoint.y).x));
        float scaleY = Math.Abs(Math.Abs(convertPositionToGPS((int)playerPoint.x, (int)playerPoint.y).y)-Math.Abs(convertPositionToGPS((int)playerPoint.x, (int)playerPoint.y+1).y));
        for(int row = (int)playerPoint.y; row < (int)playerPoint.y+buffer; row++) {
            for(int col = (int)playerPoint.x; col < (int)playerPoint.x+buffer; col++) {
                if(!chunkAddresses.ContainsKey(new Vector2(col, row))) {
                    GameObject currentTile = Instantiate(tile, transform);
                    TextureLoad textureLoader = currentTile.GetComponent<TextureLoad>();
                    textureLoader.zoom = zoom;
                    textureLoader.x = col;
                    textureLoader.y = row;
                    Vector2 gpsPoint = convertPositionToGPS(col, row);
                    SpriteRenderer m_SpriteRenderer = currentTile.GetComponent<SpriteRenderer>();
                    currentTile.transform.position = new Vector2(gpsPoint.x+scaleX/2, gpsPoint.y-scaleY/2);
                    currentTile.transform.localScale = new Vector3(scaleX, scaleY, 1);
                    textureLoader.scaleX = scaleX;
                    textureLoader.scaleY = scaleY;
                    Debug.Log("scale" + scaleX);
                    m_SpriteRenderer.color = colors[UnityEngine.Random.Range(0,3)];
                    Debug.Log("Add " + new Vector2(col,row));
                    chunkAddresses[new Vector2(col,row)] = currentTile;
                    chunks.Add(currentTile);
                }
            }
        }
    }

    Vector2 convertGPSToPosition(double lng,  double lat) {
        int x = (int)Math.Floor((lng + 180.0) / 360.0 * (1 << zoom));
        var latRad = lat / 180 * Math.PI;
        int y =  (int)Math.Floor((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << zoom));
        return new Vector2(x,y);
    }

    Vector2 convertPositionToGPS(int x,  int y) {
        float lng = (float)(x / (double)(1 << zoom) * 360.0 - 180);
        double n = Math.PI - (2.0 * Math.PI * y / Math.Pow(2.0, zoom));
        float lat = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        return new Vector2(lng*100,lat*100);
    }

    int long2tilex(double lon, int z)
    {
    return (int)Math.Floor((lon + 180.0) / 360.0 * (1 << z));
    }

    int lat2tiley(double lat, int z)
    {
        var latRad = lat / 180 * Math.PI;
        return (int)Math.Floor((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (1 << z));
    }

    double tilex2long(int x, int z)
    {
        return x / (double)(1 << z) * 360.0 - 180;
    }

    double tiley2lat(int y, int z)
    {
        double n = Math.PI - 2.0 * Math.PI * y / (1 << z);
        return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
    }

    double currentTileSize(double lat, int zoom) {
        double resolution = 156543.03 * Math.Cos(lat) / (2 ^ zoom);
        return resolution;
    }

    void PruneChunks() {
        List<GameObject> markedForDeletion = new List<GameObject>();

        foreach(GameObject chunk in chunks) {
            float distanceFromCenter = Vector3.Distance(player.transform.position, chunk.transform.position);
            if(distanceFromCenter > ViewDistance) {
                markedForDeletion.Add(chunk);
                chunkAddresses.Remove(convertGPSToPosition(chunk.transform.position.x/100, chunk.transform.position.y/100));
                Destroy(chunk);
            }
        }
        chunks = chunks.Except(markedForDeletion).ToList<GameObject>();
    }

    void loadChunks() {
        Color niceRed = new Color(255f/255,121f/255,86f/255, 1f);
        Color niceGreen = new Color(156f/255,255f/255,100f/255, 1f);
        Color niceBlue = new Color(124f/255,197f/255,255f/255, 1f);
        List<Color> colors = new List<Color>
        {
            niceBlue,
            niceGreen,
            niceRed
        };
         for(int row = (int)player.transform.position.y; row < (int)player.transform.position.y+buffer; row++) {
            for(int col = (int)player.transform.position.x; col < (int)player.transform.position.x+buffer; col++) {
                float posX = col * tileSize - buffer/2;
                float posY = row * tileSize - buffer/2;
                if(!chunkAddresses.ContainsKey(new Vector2(posX, posY))) {
                    GameObject currentTile = Instantiate(tile, transform);
                    SpriteRenderer m_SpriteRenderer = currentTile.GetComponent<SpriteRenderer>();
                    float half = m_SpriteRenderer.bounds.size.x/2;
                    currentTile.transform.position = new Vector2(posX+half, posY-half);
                    m_SpriteRenderer.color = colors[UnityEngine.Random.Range(0,3)];
                    chunkAddresses[new Vector2(posX, posY)] = currentTile;
                    chunks.Add(currentTile);
                }
            }
         }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // GenerateGridReal();
    }

    // Update is called once per frame
    void Update()
    {
        if(player.transform.hasChanged) {
            // loadChunks();
            GenerateGridReal();
            PruneChunks();
            //Debug.Log(convertGPSToPosition(player.transform.position.x/100, player.transform.position.y/100));
            player.transform.hasChanged = false;
        }
    }
}
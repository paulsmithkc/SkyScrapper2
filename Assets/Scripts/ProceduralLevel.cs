using UnityEngine;
using System.Collections;

public class ProceduralLevel : MonoBehaviour {

    public GameObject _spawnObject = null;
    public GameObject _goalPrefab = null;
    public ProceduralTile[] _tilePrefabs = new ProceduralTile[1];
    public int _levelLength = 1;
    
    // Use this for initialization
    void Start () {
        Vector3 curPosition = _spawnObject.transform.position;
        Quaternion curRotation = Quaternion.identity;
        Vector3 curForward = Vector3.forward;
        float lastTileHeight = 10.0f;
        
        int tilePrefabCount = _tilePrefabs.Length;
        for (int i = 0; i < _levelLength; ++i)
        {
            int tileIndex = Mathf.Clamp(Random.Range(0, tilePrefabCount), 0, tilePrefabCount - 1);
            var tile = _tilePrefabs[tileIndex];
            var tilePosition = curPosition + curForward * (0.5f * tile._depth);
            lastTileHeight = tile._height;

            GameObject.Instantiate(
                tile,
                tilePosition,
                curRotation
            );
            curPosition = tilePosition + curForward * (0.5f * tile._depth);
        }

        curPosition =
            curPosition +
            curForward * ( 0.5f * lastTileHeight + 5.0f) +
            Vector3.up * (-0.5f * lastTileHeight);
        GameObject.Instantiate(
            _goalPrefab,
            curPosition,
            curRotation
        );
    }
	
	// Update is called once per frame
	void Update () {
	}
}

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

            ProceduralTile tile = _tilePrefabs[tileIndex];
            float tileRotationY = tile._pathRotationY;
            Quaternion rotationChange = Quaternion.Euler(0.0f, tileRotationY, 0.0f);
            Vector3 nextForward = rotationChange * curForward;
            if (nextForward.z < 0.0f)
            {
                if (tile._mirrorPrefab != null)
                {
                    // Swap the tile out with its mirror image
                    tile = tile._mirrorPrefab;
                    tileRotationY = tile._pathRotationY;
                    rotationChange = Quaternion.Euler(0.0f, tileRotationY, 0.0f);
                    nextForward = rotationChange * curForward;
                }

                if (nextForward.z < 0.0f)
                {
                    // Reject this tile and choose another if it would cause us to loop back
                    Debug.LogFormat("I{0} Tile {1} Rejected", i, tile.name);
                    --i;
                    continue;
                }
            }

            var tilePosition = curPosition + curForward * (0.5f * tile._depth);
            lastTileHeight = tile._height;

            GameObject.Instantiate(
                tile,
                tilePosition,
                curRotation
            );

            curPosition = tilePosition + curRotation * new Vector3(
                0.5f * tile._width * Mathf.Sin(tileRotationY * Mathf.Deg2Rad),
                0.0f,
                0.5f * tile._depth * Mathf.Cos(tileRotationY * Mathf.Deg2Rad)
            );
            curRotation = rotationChange * curRotation;
            curForward = nextForward;
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

    void OnDrawGizmos()
    {
        Vector3 pos =
            _spawnObject != null ?
            _spawnObject.transform.position :
            transform.position;
        Gizmos.matrix = Matrix4x4.TRS(
            pos,
            Quaternion.identity,
            Vector3.one
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(Vector3.zero, 1.0f);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 10.0f);
    }
}

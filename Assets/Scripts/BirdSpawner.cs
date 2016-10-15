using UnityEngine;
using System.Collections;

public class BirdSpawner : MonoBehaviour {

    public GameObject _birdPrefab = null;
    public int _birdSpawnCount = 50;
    public float _birdSpawnRadius = 50.0f;
    public float _birdSpawnHeight = 30.0f;

    private ProceduralTile _tile = null;

    // Use this for initialization
    void Start () {
        _tile = GetComponent<ProceduralTile>();
        if (_tile != null)
        {
            int tileDifficulty = _tile._difficulty;
            int minDifficulty = ProceduralLevel.MIN_DIFFICULTY;
            int maxDifficulty = ProceduralLevel.MAX_DIFFICULTY;
            if (maxDifficulty > minDifficulty)
            {
                _birdSpawnCount =
                    _birdSpawnCount *
                    (tileDifficulty - minDifficulty) /
                    (maxDifficulty - minDifficulty);
            }
        }

        for (int i = 0; i < _birdSpawnCount; i++)
        {
            Vector2 a = Random.insideUnitCircle;
            float h = Random.Range(-0.5f * _birdSpawnHeight, 0.5f * _birdSpawnHeight);
            float r = Random.Range(0.0f, 360.0f);
            Vector3 b = 
                transform.position + 
                new Vector3(a.x, 0.0f, a.y) * _birdSpawnRadius +
                Vector3.up * h;
            Instantiate(_birdPrefab, b, Quaternion.Euler(0.0f, r, 0.0f));
        }
	}

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            Quaternion.identity,
            Vector3.one
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, _birdSpawnRadius);
        Gizmos.DrawSphere(Vector3.zero, 1.0f);
    }
}

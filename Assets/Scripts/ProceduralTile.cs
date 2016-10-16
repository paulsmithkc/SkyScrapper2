using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralTile : MonoBehaviour
{
    public float _width = 50.0f;
    public float _height = 50.0f;
    public float _depth = 50.0f;
    public float _pathRotationY = 0.0f;
    public ProceduralTile _mirrorPrefab = null;
    public int _difficulty = 5;
    
    public GameObject _hookerPrefab;
    public HookerSaveData[] _hookers = new HookerSaveData[0];
    public GameObject _coinPrefab;
    public CoinSaveData[] _coins = new CoinSaveData[0];
    public GameObject _laserWallPrefab;
    public LaserWallSaveData[] _laserWalls = new LaserWallSaveData[0];

    public GameObject[] _optionalObstacles = new GameObject[0];

    [System.Serializable]
    public class HookerSaveData {
        public Vector3 position = Vector3.zero;
    }

    [System.Serializable]
    public class CoinSaveData
    {
        public Vector3 position = Vector3.zero;
    }

    [System.Serializable]
    public class LaserWallSaveData
    {
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
    }

    // Use this for initialization
    void Start()
    {
        int minDifficulty = ProceduralLevel.MIN_DIFFICULTY;
        int maxDifficulty = ProceduralLevel.MAX_DIFFICULTY;
        _difficulty = Mathf.Clamp(_difficulty, minDifficulty, maxDifficulty);

        Vector3 pos = transform.position;
        List<GameObject> optionalObstacles = new List<GameObject>(_laserWalls.Length);

        if (_hookerPrefab != null && _hookers != null && _hookers.Length > 0)
        {
            foreach (var d in _hookers)
            {
                float ry = Random.Range(-180.0f, 180.0f);
                GameObject.Instantiate(
                    _hookerPrefab,
                    pos + d.position,
                    Quaternion.Euler(0.0f, ry, 0.0f),
                    transform
                );
            }
        }
        if (_coinPrefab != null && _coins != null && _coins.Length > 0)
        {
            foreach (var d in _coins)
            {
                float ry = Random.Range(-180.0f, 180.0f);
                GameObject.Instantiate(
                    _coinPrefab,
                    pos + d.position,
                    Quaternion.Euler(0.0f, ry, 0.0f),
                    transform
                );
            }
        }
        if (_laserWallPrefab != null && _laserWalls != null && _laserWalls.Length > 0)
        {
            foreach (var d in _laserWalls)
            {
                var obj = (GameObject)GameObject.Instantiate(
                    _laserWallPrefab,
                    pos + d.position,
                    Quaternion.Euler(d.rotation),
                    transform
                );
                obj.transform.localScale = d.scale;
                optionalObstacles.Add(obj);
            }
        }
        if (_optionalObstacles != null)
        {
            optionalObstacles.AddRange(_optionalObstacles);
        }

        int obstacleCount = optionalObstacles.Count;
        if (obstacleCount > 0)
        {
            int enabledObjects = 0;
            if (maxDifficulty > minDifficulty)
            {
                enabledObjects =
                    obstacleCount *
                    (_difficulty - minDifficulty) /
                    (maxDifficulty - minDifficulty);
            }
            enabledObjects = Mathf.Clamp(enabledObjects, 0, obstacleCount);

            if (enabledObjects <= (obstacleCount / 2))
            {
                foreach (var obj in optionalObstacles)
                {
                    obj.SetActive(false);
                }
                int loopLimit = obstacleCount * 2;
                while (enabledObjects > 0 && loopLimit > 0)
                {
                    --loopLimit;
                    int obstacleIndex = Random.Range(0, obstacleCount);
                    var obj = optionalObstacles[obstacleIndex];
                    if (!obj.activeSelf)
                    {
                        obj.SetActive(true);
                        --enabledObjects;
                    }
                }
            }
            else
            {
                int disabledObjects = obstacleCount - enabledObjects;
                foreach (var obj in optionalObstacles)
                {
                    obj.SetActive(true);
                }
                int loopLimit = obstacleCount * 2;
                while (disabledObjects > 0 && loopLimit > 0)
                {
                    --loopLimit;
                    int obstacleIndex = Random.Range(0, obstacleCount);
                    var obj = optionalObstacles[obstacleIndex];
                    if (obj.activeSelf)
                    {
                        obj.SetActive(false);
                        --disabledObjects;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            transform.localRotation,
            Vector3.one
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            Vector3.zero, new Vector3(_width, _height, _depth)
        );
        Gizmos.DrawLine(
            -0.5f * _depth * Vector3.forward, Vector3.zero
        );
        Gizmos.DrawLine(
            Vector3.zero,
            new Vector3(
                0.5f * _width * Mathf.Sin(_pathRotationY * Mathf.Deg2Rad), 
                0.0f,
                0.5f * _depth * Mathf.Cos(_pathRotationY * Mathf.Deg2Rad)
            )
        );

        //Gizmos.matrix = Matrix4x4.identity;

        if (_hookers != null && _hookers.Length > 0)
        {
            Gizmos.color = Color.green;
            foreach (var d in _hookers)
            {
                Gizmos.DrawWireSphere(d.position, 4.0f);
            }
        }
        if ( _coins != null && _coins.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var d in _coins)
            {
                Gizmos.DrawWireSphere(d.position, 2.0f);
            }
        }
        if (_laserWalls != null && _laserWalls.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (var d in _laserWalls)
            {
                Gizmos.DrawWireCube(
                    d.position, 
                    new Vector3(20.0f * d.scale.x, 20.0f * d.scale.y, d.scale.z)
                );
            }
        }
    }
}

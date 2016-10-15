using UnityEngine;
using System.Collections;

public class ProceduralTile : MonoBehaviour
{
    public float _width = 50.0f;
    public float _height = 50.0f;
    public float _depth = 50.0f;
    public float _pathRotationY = 0.0f;
    public ProceduralTile _mirrorPrefab = null;
    public int _difficulty = 5;
    public GameObject[] _optionalObstacles = new GameObject[0];

    // Use this for initialization
    void Start()
    {
        int minDifficulty = ProceduralLevel.MIN_DIFFICULTY;
        int maxDifficulty = ProceduralLevel.MAX_DIFFICULTY;
        _difficulty = Mathf.Clamp(_difficulty, minDifficulty, maxDifficulty);

        if (_optionalObstacles != null && _optionalObstacles.Length > 0)
        {
            int obstacleCount = _optionalObstacles.Length;
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
                foreach (var obj in _optionalObstacles)
                {
                    obj.SetActive(false);
                }
                int loopLimit = obstacleCount * 2;
                while (enabledObjects > 0 && loopLimit > 0)
                {
                    --loopLimit;
                    int obstacleIndex = Random.Range(0, obstacleCount);
                    var obj = _optionalObstacles[obstacleIndex];
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
                foreach (var obj in _optionalObstacles)
                {
                    obj.SetActive(true);
                }
                int loopLimit = obstacleCount * 2;
                while (disabledObjects > 0 && loopLimit > 0)
                {
                    --loopLimit;
                    int obstacleIndex = Random.Range(0, obstacleCount);
                    var obj = _optionalObstacles[obstacleIndex];
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
    }
}

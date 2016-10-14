using UnityEngine;
using System.Collections;

public class ProceduralTile : MonoBehaviour
{
    public float _width = 1.0f;
    public float _height = 1.0f;
    public float _depth = 1.0f;
    public float _pathRotationY = 0.0f;

    // Use this for initialization
    void Start()
    {
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

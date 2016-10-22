using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    public float _radius = 50.0f;
    public float _height = 200.0f;
    
    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            Quaternion.identity,
            Vector3.one
        );

        Gizmos.color = Color.magenta;
        float tau = 2.0f * Mathf.PI;
        float inc = tau / 8;
        for (int i = 0; i < 8; ++i)
        {
            Gizmos.DrawLine(
                new Vector3(_radius * Mathf.Cos(i * inc), -0.5f * _height, _radius * Mathf.Sin(i * inc)),
                new Vector3(_radius * Mathf.Cos(i * inc),  0.5f * _height, _radius * Mathf.Sin(i * inc))
            );
            Gizmos.DrawLine(
                new Vector3(_radius * Mathf.Cos(i * inc), -0.5f * _height, _radius * Mathf.Sin(i * inc)),
                new Vector3(_radius * Mathf.Cos(i * inc + inc), -0.5f * _height, _radius * Mathf.Sin(i * inc + inc))
            );
            Gizmos.DrawLine(
                new Vector3(_radius * Mathf.Cos(i * inc), 0.5f * _height, _radius * Mathf.Sin(i * inc)),
                new Vector3(_radius * Mathf.Cos(i * inc + inc), 0.5f * _height, _radius * Mathf.Sin(i * inc + inc))
            );
        }
    }
}

using UnityEngine;
using System.Collections;

public class LittleRobot : MonoBehaviour {

    private float _directionDuration = 0.0f;
    private float _rotate = 0.0f;
    private float _speed = 0.0f;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime > 0.0f)
        {
            if (_directionDuration <= 0)
            {
                _directionDuration = Random.Range(2.0f, 3.0f);
                _rotate = Random.Range(-60.0f, 60.0f);
                _speed = Random.Range(0.0f, 12.0f);
            }
            transform.position += transform.forward * _speed * deltaTime;
            transform.Rotate(transform.up, _rotate * deltaTime);
            _directionDuration -= deltaTime;
        }
    }
}

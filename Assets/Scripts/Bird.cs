using UnityEngine;
using System.Collections;


public class Bird : MonoBehaviour {

    private float _directionDuration = 0.0f;
    private float _rotate = 0.0f;
    
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float deltaTime = Time.deltaTime;
        if (deltaTime > 0.0f)
        {
            if (_directionDuration <= 0)
            {
                _directionDuration = Random.Range(2, 3);
                _rotate = Random.Range(-1F, 1F);
            }
            transform.position += transform.forward * -0.05f;
            transform.Rotate(transform.up, _rotate);
            _directionDuration -= deltaTime;
        }
    }
}

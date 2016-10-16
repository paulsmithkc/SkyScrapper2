using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour {

    public Player _player;
    public ProceduralLevel _level;
    public LineRenderer _lineRenderer;
    public GameObject _playerMapIcon;

    private Vector3 _playerPosition = Vector3.zero;
    private Vector3[] _waypoints = null;
    private Rect _bounds = new Rect();

    // Use this for initialization
    void Start() {
	}

    // Update is called once per frame
    void Update() {
        if (_player != null)
        {
            _playerPosition = _player.transform.position;
        }
        if (_level != null)
        {
            _waypoints = _level.GetWaypoints();
        }

        // Determine the bounds of the mimimap
        _bounds.Set(0.0f, 0.0f, 0.0f, 0.0f);
        if (_waypoints != null)
        {
            foreach (Vector3 p in _waypoints)
            {
                if (p.x < _bounds.xMin) { _bounds.xMin = p.x; }
                if (p.x > _bounds.xMax) { _bounds.xMax = p.x; }
                if (p.z < _bounds.yMin) { _bounds.yMin = p.z; }
                if (p.z > _bounds.yMax) { _bounds.yMax = p.z; }
            }
        }
        {
            Vector3 p = _playerPosition;
            if (p.x < _bounds.xMin) { _bounds.xMin = p.x; }
            if (p.x > _bounds.xMax) { _bounds.xMax = p.x; }
            if (p.z < _bounds.yMin) { _bounds.yMin = p.z; }
            if (p.z > _bounds.yMax) { _bounds.yMax = p.z; }
        }

        // Make the bounds slightly bigger
        _bounds.xMin -= 0.5f;
        _bounds.xMax += 0.5f;
        _bounds.yMin -= 0.5f;
        _bounds.yMax += 0.5f;

        // Make the bounds square
        if (_bounds.width <= _bounds.height)
        {
            float addWidth = 0.5f * (_bounds.height - _bounds.width);
            _bounds.xMin -= addWidth;
            _bounds.xMax += addWidth;
        }
        else
        {
            float addHeight = 0.5f * (_bounds.width - _bounds.height);
            _bounds.yMin -= addHeight;
            _bounds.yMax += addHeight;
        }

        // Add a margin
        {
            float m = _bounds.width * 0.1f;
            _bounds.xMin -= m;
            _bounds.xMax += m;
            _bounds.yMin -= m;
            _bounds.yMax += m;
        }

        // Draw the waypoint line
        Vector2 boundsCenter = _bounds.center;
        float scale = 1.0f / _bounds.width;
        if (_waypoints != null)
        {
            _lineRenderer.SetVertexCount(_waypoints.Length);
            for (int i = 0; i < _waypoints.Length; ++i)
            {
                Vector3 p = _waypoints[i];
                Vector3 v = new Vector3(
                    (p.x - boundsCenter.x) * scale,
                    (p.z - boundsCenter.y) * scale,
                    0.0f
                );
                _lineRenderer.SetPosition(i, v);
                //_lineRenderer.SetPosition(i * 2, v);
                //_lineRenderer.SetPosition(i * 2 + 1, v);
            }
            _lineRenderer.enabled = true;
        }
        else
        {
            _lineRenderer.enabled = false;
        }

        // Position the player icon
        _playerMapIcon.transform.localPosition = new Vector3(
            (_playerPosition.x - boundsCenter.x) * scale,
            (_playerPosition.z - boundsCenter.y) * scale,
            0.0f
        );
    }
}

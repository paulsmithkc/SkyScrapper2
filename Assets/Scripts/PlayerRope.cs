using System;
using UnityEngine;

public class PlayerRope : IDisposable
{
    // Physics Tuning
    public const float ROPE_MAX_LENGTH = 100.0f;
    public const float ROPE_ATTACH_DECELERATION = 0.0f;
    public const float ROPE_FORCE_NORMAL = 0.3f;
    public const float ROPE_FORCE_GOAL = 1.0f;
    public const float ROPE_RELAXED_LENGTH_NORMAL = 2.0f;
    public const float ROPE_RELAXED_LENGTH_GOAL = -10.0f;

    public int _id;
    public Player _player;
    public Material _ropeMaterial;
    public GameObject _grapple;
    public LineRenderer _ropeRenderer;
    public readonly float _ropeInitialLength;
    public readonly Vector3 _ropeInitialVector;
    public readonly float _ropeForce;
    public readonly float _ropeRelaxedLength;
    
    private Vector3 _centripetalAccel = Vector3.zero;
    private Vector3 _springAccel = Vector3.zero;
    private bool _attachedToGoal = false;
    private bool _detach = false;

    public PlayerRope(int id, Player player, RaycastHit hitInfo)
    {
        _id = id;
        _player = player;
        _ropeMaterial = _player.GetRopeMaterial(id);

        Vector3 ropeVector = (hitInfo.point - _player.transform.position);
        _ropeInitialLength = ropeVector.magnitude;
        _ropeInitialVector = ropeVector;

        // Create the grapple hook
        _grapple = (GameObject)GameObject.Instantiate(_player._grapple, hitInfo.point, Quaternion.identity);
        _grapple.transform.parent = hitInfo.collider.transform;
        _grapple.transform.position = hitInfo.point;
        _grapple.transform.forward = _ropeInitialVector.normalized;

        // Add a rope to the grapple hook
        _ropeRenderer = _grapple.AddComponent<LineRenderer>();
        _ropeRenderer.material = _ropeMaterial;
        _ropeRenderer.useWorldSpace = true;
        _ropeRenderer.enabled = true;
        _ropeRenderer.SetWidth(0.2f, 0.2f);
        _ropeRenderer.SetColors(Color.white, Color.white);

        _attachedToGoal = string.Equals(hitInfo.collider.tag, Player.GOAL_TAG);
        _ropeForce = _attachedToGoal ? 
            ROPE_FORCE_GOAL : ROPE_FORCE_NORMAL;
        _ropeRelaxedLength = _attachedToGoal ? 
            ROPE_RELAXED_LENGTH_GOAL : ROPE_RELAXED_LENGTH_NORMAL;
    }

    public void Update(float deltaTime)
    {
        if (_detach)
        {
            _grapple.SetActive(false);
            _ropeRenderer.enabled = false;
            return;
        }

        //var grappleScale = Vector3.one;
        //var grappleParent = _grapple.transform.parent;
        //while (grappleParent != null)
        //{
        //    grappleScale.x /= grappleParent.localScale.x;
        //    grappleScale.y /= grappleParent.localScale.y;
        //    grappleScale.z /= grappleParent.localScale.z;
        //    grappleParent = grappleParent.parent;
        //}
        //_grapple.transform.localScale = grappleScale;

        _grapple.SetActive(true);

        Vector3 playerPosition = _player.transform.position;
        Vector3 ropeStart = playerPosition + _player.transform.up * -2.0f;
        Vector3 ropeEnd = _grapple.transform.position;
        int segments = 1;
        _ropeRenderer.SetVertexCount(1 + segments);
        _ropeRenderer.SetPosition(0, ropeStart);
        for (int i = 1; i < segments; ++i)
        {
            float t = ((float)i) / ((float)segments);
            var a = ropeEnd * t;
            var b = ropeStart * (1 - t);
            _ropeRenderer.SetPosition(i, a + b);
        }
        _ropeRenderer.SetPosition(segments, ropeEnd);
        _ropeRenderer.enabled = true;

        float ropeLength = Vector3.Distance(ropeStart, ropeEnd);
        _ropeRenderer.material.mainTextureScale = new Vector2(
            ropeLength * 4.0f, 1.0f
        );
    }

    public void FixedUpdate(float deltaTime)
    {
        if (_detach) { return; }

        Rigidbody playerRigidBody = _player._rigidbody;
        Vector3 playerPosition = playerRigidBody.position;
        Vector3 playerVelocity = playerRigidBody.velocity;
        Vector3 ropeEnd = _grapple.transform.position;

        Vector3 ropeVector = (ropeEnd - playerPosition);
        Vector3 ropeVectorNormalized = ropeVector.normalized;
        Vector3 velocityNormal = Vector3.Project(playerVelocity, ropeVectorNormalized);
        Vector3 velocityTangential = playerVelocity - velocityNormal;
        float speedTangential = velocityTangential.magnitude;

        // Centripetal Acceleration
        _centripetalAccel = ropeVectorNormalized * (speedTangential * speedTangential / _ropeInitialLength);
        //playerRigidBody.AddForce(_centripetalAccel, ForceMode.Acceleration);

        // Spring Force
        _springAccel = (ropeVector - ropeVector.normalized * _ropeRelaxedLength) * _ropeForce;
        playerRigidBody.AddForce(_springAccel, ForceMode.Acceleration);

        //Vector3 n = ropeVector.normalized;
        //float nm = n.magnitude;
        //if (nm >= 0.5f && nm <= 1.5f)
        //{
        //    playerRigidBody.AddForce(n * 5.0f, ForceMode.Acceleration);
        //}

        if (!_attachedToGoal && _id < 3)
        {
            // Detach the rope once we have swung sufficiently past our attachment point
            // by check that the projection of the ropeVector onto the initial attachment vector
            // in the xz plane is less than -0.5 * initial rope length
            Vector3 u = ropeVector;
            Vector3 v = _ropeInitialVector;
            u.y = 0.0f;
            v.y = 0.0f;
            float um = u.magnitude;
            float vm = v.magnitude;
            if (um >= 1.0f && vm >= 1.0f)
            {
                float s = Vector3.Dot(u, v) / um;
                if (s <= -0.5f * vm)
                {
                    _detach = true;
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (_detach) { return; }

        Vector3 playerPosition = _player.transform.position;

        Gizmos.color = _player.GetRopeColor(_id);
        Vector3 ropeVector = (_grapple.transform.position - playerPosition);
        Gizmos.DrawRay(playerPosition, ropeVector);

        Gizmos.color = Color.gray;
        Gizmos.DrawRay(playerPosition, 2.0f * _centripetalAccel);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(playerPosition, 2.0f * _springAccel);
    }

    public void Dispose()
    {
        _grapple.SetActive(false);
        _ropeRenderer.enabled = false;
        GameObject.Destroy(_grapple);
    }
}

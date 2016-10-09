using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Layers
    private const int UI_LAYER = 5;
    private const int HOOKER_LAYER = 8;
    private const int PLAYER_LAYER = 9;

    // Tags
    private const string PLAYER_TAG = "Player";
    private const string HOOKER_TAG = "Hooker";
    private const string PLATFORM_TAG = "Platform";
    private const string GOAL_TAG = "Goal";
    private const string BIRD_TAG = "Bird";
    private const string NEAR_BIRD_TAG = "NearBird";
    private const string LASER_TAG = "Laser";

    // Inputs
    private const string MOUSE_X_INPUT = "Mouse X";
    private const string MOUSE_Y_INPUT = "Mouse Y";
    private const string FIRE1_INPUT = "Fire1";
    private const string FIRE2_INPUT = "Fire2";
    private const string FIRE3_INPUT = "Fire3";
    private const string QUIT_INPUT = "Quit";
    private const string RESTART_INPUT = "Restart";
    private const string PAUSE_INPUT = "Pause";

    // Physics Tuning
    private float _yawSensitivity = 360.0f;
    private float _pitch = 0.0f;
    private float _pitchSensitivity = 180.0f;
    private float _pitchMax = 90.0f;
    private float _ropeMaxLength = 50.0f;
    private float _ropeRelaxedLength = 10.0f;
    private float _ropeForceNormal = 0.3f;
    private float _ropeForceGoal = 1.0f;
    private float _ropeFireDecelerate = 0.25f;
    private float _instadeathHeight = 0.0f;
    private float _nearPlatformRadius = 3.0f;

    // Game State
    private bool _dead = false;
    private bool _goalReached = false;
    private float _loadLevelDelay = 2.0f;
    private List<Rope> _ropes = new List<Rope>();

    // Editor Fields

    private AudioSource _fxAudioSource = null;
    //private AudioSource _targetAudioSource = null;

    public Camera _camera;
    public Rigidbody _rigidbody;
    public GameObject _target;
    public AudioClip _fireSound1;
    public float _fireSound1Volume = 1.0f;
    //public AudioClip _fireSound2;
    //public float _fireSound2Volume = 1.0f;
    public AudioClip _nearBirdSound = null;
    public float _nearBirdSoundVolume = 1.0f;
    public AudioClip _collisionSound = null;
    public float _collisionSoundVolume = 1.0f;
    public Material _ropeMaterial1;
    public Material _ropeMaterial2;
    
    public HudFade _hudFade;
    public GameObject _grapple;
    public string _nextLevel;

    public bool dead
    {
        get { return _dead; }
    }
    public bool goalReached
    {
        get { return _goalReached; }
    }

    // Use this for initialization
    void Start()
    {
        //if (!Application.isEditor)
        //{
        //    Cursor.visible = false;
        //    Cursor.lockState = CursorLockMode.Locked;
        //}

        _pitchMax = Mathf.Clamp(_pitchMax, 0.0f, 90.0f);
        _fxAudioSource = gameObject.AddComponent<AudioSource>();
        //_targetAudioSource = _target.AddComponent<AudioSource>();

        if (!_camera)
        {
            _camera = Camera.main;
        }
        if (!_rigidbody)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        if (!_hudFade)
        {
            _hudFade = GetComponent<HudFade>();
        }
        if (_target)
        {
            _target.SetActive(false);
        }

        _rigidbody.drag = 0.0f;
        _rigidbody.angularDrag = 0.0f;
        _rigidbody.freezeRotation = true;
        _rigidbody.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!Application.isEditor)
        {
            bool paused = (Time.timeScale == 0.0f);
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Quit / Restart

        if (Input.GetButtonDown(QUIT_INPUT))
        {
            Time.timeScale = 0.0f;
            Application.Quit();
            return;
        }
        if (Input.GetButtonDown(RESTART_INPUT))
        {
            OnRestart();
            return;
        }
        if (Input.GetButtonDown(PAUSE_INPUT))
        {
            Time.timeScale = (Time.timeScale == 0.0f ? 1.0f : 0.0f);
        }

        float deltaTime = Time.deltaTime;
        if (_goalReached || _dead)
        {
            _loadLevelDelay -= deltaTime;
            if (_loadLevelDelay <= 0.0f)
            {
                if (_goalReached && !string.IsNullOrEmpty(_nextLevel))
                {
                    SceneManager.LoadScene(_nextLevel);
                }
                else
                {
                    OnRestart();
                }
            }
            return;
        }
        if (transform.position.y < _instadeathHeight)
        {
            OnDeath();
            return;
        }

        // Inputs
        
        float mouseX = Input.GetAxis(MOUSE_X_INPUT);
        float mouseY = Input.GetAxis(MOUSE_Y_INPUT);
        bool fire1 = Input.GetButtonDown(FIRE1_INPUT);
        bool fire2 = Input.GetButtonDown(FIRE2_INPUT);
        bool fire3 = Input.GetButtonDown(FIRE3_INPUT);

        // Camera

        float deltaYaw = mouseX * _yawSensitivity * deltaTime;
        float deltaPitch = -mouseY * _pitchSensitivity * deltaTime;

        _pitch = Mathf.Clamp(
            _pitch + deltaPitch,
            -_pitchMax, _pitchMax
        );
        transform.Rotate(0.0f, deltaYaw, 0.0f);
        _camera.transform.forward = transform.forward;
        _camera.transform.Rotate(_pitch, 0.0f, 0.0f);

        // Grapple Hooks

        int layerMask = ~(1 << PLAYER_LAYER | 1 << UI_LAYER);  // Ignore Player Layer
        RaycastHit hitInfo;
        bool wasHit = Physics.Raycast(
            _camera.transform.position, 
            _camera.transform.forward, 
            out hitInfo, 
            maxDistance: _ropeMaxLength, 
            layerMask: layerMask
        );

        if (wasHit)
        {
            _target.transform.parent = null;
            _target.transform.position = hitInfo.point;
        }
        _target.SetActive(wasHit);

        if (fire3)
        {
            FireRopeBomb();
        }
        else
        {
            if (fire1)
            {
                FireRope(1, wasHit, hitInfo);
            }
            if (fire2)
            {
                FireRope(2, wasHit, hitInfo);
            }
        }

        foreach (var r in _ropes)
        {
            r.Update();
        }
    }

    private void FireRope(int id, bool wasHit, RaycastHit hitInfo)
    {
        for (int i = _ropes.Count - 1; i >= 0; --i)
        {
            var r = _ropes[i];
            if (r._id == id || r._id == 3)
            {
                r.Dispose();
                _ropes.RemoveAt(i);
            }
        }
        if (wasHit)
        {
            var r = new Rope(id, this, hitInfo);
            _ropes.Add(r);

            // Play the firing sound
            _fxAudioSource.PlayOneShot(_fireSound1, _fireSound1Volume);

            // Slow the player down
            _rigidbody.AddForce(
                _rigidbody.velocity * -_ropeFireDecelerate,
                ForceMode.VelocityChange
            );

            // Jump up if near a roof
            int layerMask = ~(1 << PLAYER_LAYER | 1 << UI_LAYER);  // Ignore Player Layer
            var nearbyColliders = Physics.OverlapSphere(transform.position, _nearPlatformRadius, layerMask);
            var nearPlatform = false;
            foreach (var c in nearbyColliders)
            {
                if (c.gameObject.tag == PLATFORM_TAG)
                {
                    nearPlatform = true;
                    break;
                }
            }
            if (nearPlatform)
            {
                _rigidbody.AddForce(
                    transform.up * 2.0f,
                    ForceMode.VelocityChange
                );
            }
        }
    }

    private void FireRopeBomb()
    {
        // Remove all ropes
        foreach (var r in _ropes)
        {
            r.Dispose();
        }
        _ropes.Clear();

        // Spawn ropes
        Vector3 playerPosition = transform.position;
        int birdMask = (1 << HOOKER_LAYER);
        int playerMask = ~(1 << PLAYER_LAYER | 1 << UI_LAYER);  // Ignore Player Layer
        var nearbyColliders =
            from c in Physics.OverlapSphere(transform.position, _ropeMaxLength, birdMask)
            select new {
                c,
                c.transform.position,
                c.gameObject.tag,
                distance = (c.transform.position - playerPosition).magnitude
        };
        //Debug.LogFormat("{0} Colliders Nearby", nearbyColliders.Count());
        foreach (var x in nearbyColliders.OrderBy(x => x.distance).Take(16))
        {
            RaycastHit hitInfo;
            bool wasHit = Physics.Linecast(
                playerPosition,
                x.position,
                out hitInfo,
                layerMask: playerMask
            );
            if (wasHit && (
                hitInfo.collider.gameObject.tag == HOOKER_TAG ||
                hitInfo.collider.gameObject.tag == BIRD_TAG || 
                hitInfo.collider.gameObject.tag == NEAR_BIRD_TAG))
            {
                var r = new Rope(3, this, hitInfo);
                _ropes.Add(r);
                if (_ropes.Count >= 6) { break; }
            }
        }
        //Debug.LogFormat("{0} Ropes Attached", _ropes.Count);

        // Play the firing sound
        if (_ropes.Count > 0)
        {
            _fxAudioSource.PlayOneShot(_fireSound1, _fireSound1Volume);
        }
    }

    public class Rope : IDisposable
    {
        public int _id;
        public Player _player;
        public Material _ropeMaterial;
        public GameObject _grapple;
        public LineRenderer _ropeRenderer;
        public float _ropeForce;
        public float _ropeRelaxedLength;

        public Rope(int id, Player player, RaycastHit hitInfo)
        {
            _id = id;
            _player = player;
            _ropeMaterial = _player.GetRopeMaterial(id);

            // Create the grapple hook
            _grapple = (GameObject)GameObject.Instantiate(_player._grapple, hitInfo.point, Quaternion.identity);
            _grapple.transform.parent = hitInfo.collider.transform;
            _grapple.transform.position = hitInfo.point;
            _grapple.transform.forward = (hitInfo.point - _player.transform.position).normalized;

            // Add a rope to the grapple hook
            _ropeRenderer = _grapple.AddComponent<LineRenderer>();
            _ropeRenderer.material = _ropeMaterial;
            _ropeRenderer.useWorldSpace = true;
            _ropeRenderer.enabled = true;
            _ropeRenderer.SetWidth(0.1f, 0.1f);
            _ropeRenderer.SetColors(Color.white, Color.white);

            bool attachedToGoal = string.Equals(hitInfo.collider.tag, GOAL_TAG);
            _ropeForce = attachedToGoal ? _player._ropeForceGoal : _player._ropeForceNormal;
            _ropeRelaxedLength = attachedToGoal ? 0.0f : _player._ropeRelaxedLength;
        }

        public void Update()
        {
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
            
            var ropeVector = (ropeEnd - playerPosition);
            ropeVector -= ropeVector.normalized * _ropeRelaxedLength;
            _player._rigidbody.AddForce(ropeVector * _ropeForce, ForceMode.Acceleration);
        }

        public void Dispose()
        {
            _grapple.SetActive(false);
            _ropeRenderer.enabled = false;
            GameObject.Destroy(_grapple);
        }
    }

    private Color GetRopeColor(int rope)
    {
        switch (rope)
        {
            case 1:
                return Color.red;
            case 2:
                return Color.blue;
            default:
                return Color.black;
        }
    }

    private Material GetRopeMaterial(int rope)
    {
        switch (rope)
        {
            case 1:
                return _ropeMaterial1;
            case 2:
                return _ropeMaterial2;
            default:
                return _ropeMaterial1;
        }
    }

    public void OnDeath()
    {
        if (!_dead && !_goalReached)
        {
            _dead = true;
            _rigidbody.AddForce(_rigidbody.velocity * -0.5f, ForceMode.VelocityChange);
            _fxAudioSource.PlayOneShot(_collisionSound, _collisionSoundVolume);
            _hudFade.FadeTo(Color.black, _loadLevelDelay * 0.5f);
            _hudFade.fadeText.text = "OUCH!";
            _hudFade.reticle.gameObject.SetActive(false);

            foreach (var r in _ropes)
            {
                r.Dispose();
            }
            _ropes.Clear();
        }
    }

    public void OnGoalReached()
    {
        if (!_dead && !_goalReached)
        {
            _goalReached = true;
            _rigidbody.AddForce(-_rigidbody.velocity, ForceMode.VelocityChange);
            _hudFade.FadeTo(Color.white, _loadLevelDelay * 0.5f);
            _hudFade.fadeText.text = "NEXT LEVEL";
            _hudFade.reticle.gameObject.SetActive(false);

            foreach (var r in _ropes)
            {
                r.Dispose();
            }
            _ropes.Clear();
        }
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnCollisionEnter(Collision col)
    {
        switch (col.gameObject.tag)
        {
            case PLAYER_TAG:
                break;
            case PLATFORM_TAG:
                _rigidbody.AddForce(_rigidbody.velocity * -0.5f, ForceMode.VelocityChange);
                break;
            case GOAL_TAG:
                OnGoalReached();
                break;
            case LASER_TAG:
                OnDeath();
                break;
            default:
                break;
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        switch (col.gameObject.tag)
        {
            case PLAYER_TAG:
                break;
            case GOAL_TAG:
                OnGoalReached();
                break;
			case NEAR_BIRD_TAG:
            	_fxAudioSource.PlayOneShot(_nearBirdSound, _nearBirdSoundVolume);
                break;
            case LASER_TAG:
                OnDeath();
                break;
            default:
                break;
        }
    }
}

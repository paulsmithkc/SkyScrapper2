using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Layers
    public const int IGNORE_RAYCAST_LAYER = 2;
    public const int UI_LAYER = 5;
    public const int HOOKER_LAYER = 8;
    public const int PLAYER_LAYER = 9;
    public const int LASER_LAYER = 10;

    // Tags
    public const string PLAYER_TAG = "Player";
    public const string HOOKER_TAG = "Hooker";
    public const string PLATFORM_TAG = "Platform";
    public const string GOAL_TAG = "Goal";
    public const string BIRD_TAG = "Bird";
    public const string NEAR_BIRD_TAG = "NearBird";
    public const string LASER_TAG = "Laser";
    public const string BOUNDS_TAG = "Bounds";

    // Inputs
    public const string HORIZONTAL_INPUT = "Horizontal";
    public const string VERTICAL_INPUT = "Vertical";
    public const string MOUSE_X_INPUT = "Mouse X";
    public const string MOUSE_Y_INPUT = "Mouse Y";
    public const string FIRE1_INPUT = "Fire1";
    public const string FIRE2_INPUT = "Fire2";
    public const string FIRE3_INPUT = "Fire3";
    public const string QUIT_INPUT = "Quit";
    public const string RESTART_INPUT = "Restart";
    public const string PAUSE_INPUT = "Pause";
    public const string SLOW_TIME_INPUT = "Slow Time";
    public const string JUMP_INPUT = "Jump";

    // Physics Tuning
    private float _yawSensitivity = 360.0f;
    private float _pitch = 0.0f;
    private float _pitchSensitivity = 180.0f;
    private float _pitchMax = 90.0f;
    private float _instadeathHeight = -200.0f;
    private float _nearPlatformRadius = 3.0f;
    private float _slowTimeSpeed = 0.25f;
    private float _jetpackForce = 20.0f;
    private float _jumpForce = 3.0f;
    private float _runForce = 5.0f;
    private int _airJumpsMax = 2;

    private const float MIN_SPHERECAST_RADIUS = 0.5f;
    private const float NORMAL_DRAG = 0.2f;
    private const float ROPE_BOMB_DRAG = 0.9f;

    // Game State
    private bool _dead = false;
    private bool _goalReached = false;
    private float _loadLevelDelay = 2.0f;
    private List<PlayerRope> _ropes = new List<PlayerRope>();
    private float _sphereCastRadius = MIN_SPHERECAST_RADIUS;
    private bool _prevFrameWasHit = false;
    private RaycastHit _prevFrameHitInfo;
    private bool _grounded = false;
    private int _airJumpsAvailable = 0;

    // Editor Fields

    private AudioSource _fxAudioSource = null;
    //private AudioSource _targetAudioSource = null;

    public Camera _camera;
    public Rigidbody _rigidbody;
    public GameObject _target1;
    public GameObject _target2;
    public AudioClip _fireSound1;
    public float _fireSound1Volume = 1.0f;
    //public AudioClip _fireSound2;
    //public float _fireSound2Volume = 1.0f;
    public AudioClip _nearBirdSound = null;
    public float _nearBirdSoundVolume = 1.0f;
    public AudioClip _collisionSound = null;
    public float _collisionSoundVolume = 1.0f;
    public AudioClip _laserCollisionSound = null;
    public float _laserCollisionSoundVolume = 1.0f;
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
        _prevFrameWasHit = false;
        _grounded = false;
        _airJumpsAvailable = 0;

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
        if (_target1)
        {
            _target1.SetActive(false);
        }
        if (_target2)
        {
            _target2.SetActive(false);
        }

        _rigidbody.drag = NORMAL_DRAG;
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

        // Quit / Restart / Goal / Death

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
        if (Input.GetButtonDown(SLOW_TIME_INPUT))
        {
            Time.timeScale = (Time.timeScale == _slowTimeSpeed ? 1.0f : _slowTimeSpeed);
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
            OnDeath(BOUNDS_TAG);
            return;
        }

        // Inputs

        //float horizontal = Input.GetAxis(HORIZONTAL_INPUT);
        //float vertical = Input.GetAxis(VERTICAL_INPUT);
        float mouseX = Input.GetAxis(MOUSE_X_INPUT);
        float mouseY = Input.GetAxis(MOUSE_Y_INPUT);
        bool fire1 = Input.GetButtonDown(FIRE1_INPUT);
        bool fire2 = Input.GetButtonDown(FIRE2_INPUT);
        bool fire3 = Input.GetButtonDown(FIRE3_INPUT);
        bool jumpButtonDown = Input.GetButtonDown(JUMP_INPUT);

        // Camera

        float unscaledDeltaTime = Time.timeScale == 0.0f ? 0.0f : Time.unscaledDeltaTime;
        float deltaYaw = mouseX * _yawSensitivity * unscaledDeltaTime;
        float deltaPitch = -mouseY * _pitchSensitivity * unscaledDeltaTime;

        _pitch = Mathf.Clamp(
            _pitch + deltaPitch,
            -_pitchMax, _pitchMax
        );
        transform.Rotate(0.0f, deltaYaw, 0.0f);
        _camera.transform.forward = transform.forward;
        _camera.transform.Rotate(_pitch, 0.0f, 0.0f);

        // Grounded / Jump

        int grappleLayerMask = ~(
            1 << PLAYER_LAYER |
            1 << UI_LAYER |
            1 << LASER_LAYER |
            1 << IGNORE_RAYCAST_LAYER
        );
        RaycastHit hitInfo;
        _grounded = Physics.SphereCast(
            transform.position,
            0.5f,
            -transform.up,
            out hitInfo,
            1.5f,
            grappleLayerMask
        );
        if (_grounded) {
            _airJumpsAvailable = _airJumpsMax;
        }
        if (jumpButtonDown)
        {
            Jump();
        }

        // Grapple Hooks

        Vector3 cameraPosition = _camera.transform.position;
        Vector3 cameraForward = _camera.transform.forward;
        bool wasHit = Physics.SphereCast(
            cameraPosition,
            MIN_SPHERECAST_RADIUS,
            cameraForward, 
            out hitInfo, 
            PlayerRope.ROPE_MAX_LENGTH,
            grappleLayerMask
        );
        

        Vector3 velocity = _rigidbody.velocity;
        Vector3 velocityTangential = velocity - Vector3.Project(velocity, cameraForward);
        _sphereCastRadius = Mathf.Max(MIN_SPHERECAST_RADIUS, velocityTangential.magnitude);
        if (!wasHit)
        {
            // Try SphereCast if Raycast failed
            wasHit = Physics.SphereCast(
                cameraPosition,
                _sphereCastRadius,
                cameraForward,
                out hitInfo,
                PlayerRope.ROPE_MAX_LENGTH,
                grappleLayerMask
            );
        }

        if (wasHit)
        {
            Vector3 ropeVector = hitInfo.point - _rigidbody.position;
            if ((ropeVector.y >= -2.5f && ropeVector.y <= 1.5f) ||
                ropeVector.magnitude < 1.5f)
            {
                wasHit = false;
            }
        }

        // Position target1
        if (wasHit)
        {
            _target1.transform.parent = null;
            _target1.transform.position = hitInfo.point;
            _prevFrameWasHit = wasHit;
            _prevFrameHitInfo = hitInfo;
        }
        _target1.SetActive(wasHit);

        // Position target2
        _target2.transform.position = cameraPosition + cameraForward * 10.0f;
        _target2.transform.localScale = new Vector3(
            0.25f * _sphereCastRadius,
            0.25f * _sphereCastRadius,
            0.25f * _sphereCastRadius
        );
        _target2.SetActive(true);

        if (fire3)
        {
            FireRopeBomb();
        }
        else
        {
            if (fire1)
            {
                FireRope(1, _prevFrameWasHit, _prevFrameHitInfo);
            }
            if (fire2)
            {
                FireRope(2, _prevFrameWasHit, _prevFrameHitInfo);
            }
        }
        
        foreach (var r in _ropes)
        {
            r.Update(deltaTime);
        }

        _prevFrameWasHit = wasHit;
        _prevFrameHitInfo = hitInfo;
    }

    void FixedUpdate()
    {
        // Inputs

        float deltaTime = Time.fixedDeltaTime;
        float horizontal = Input.GetAxis(HORIZONTAL_INPUT);
        float vertical = Input.GetAxis(VERTICAL_INPUT);
        bool jumpPressed = Input.GetButton(JUMP_INPUT);
        
        // Running

        //if (_grounded)
        {
            _rigidbody.AddForce(transform.forward * vertical * _runForce);
            _rigidbody.AddForce(transform.right * horizontal * _runForce);
        }

        // Jetpack

        //if (jumpPressed) //|| _ropes.Count > 0)
        //{
        //    float force = _jetpackForce;
        //    if (!jumpPressed) { force *= 0.5f; }
        //    _rigidbody.AddForce(
        //        _camera.transform.forward * force,
        //        ForceMode.Acceleration
        //    );
        //}

        // Ropes

        foreach (var r in _ropes)
        {
            r.FixedUpdate(deltaTime);
        }
    }

    void OnDrawGizmos()
    {
        foreach (var r in _ropes)
        {
            r.OnDrawGizmos();
        }
    }

    private void Jump()
    {
        if (_grounded || _airJumpsAvailable > 0)
        {
            _rigidbody.AddForce(
            transform.up * (_jumpForce - _rigidbody.velocity.y),
                ForceMode.VelocityChange
            );
            if (!_grounded)
            {
                --_airJumpsAvailable;
            }
        }
    }

    private void FireRope(int id, bool wasHit, RaycastHit hitInfo)
    {
        _rigidbody.drag = NORMAL_DRAG;
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
            var r = new PlayerRope(id, this, hitInfo);
            _ropes.Add(r);

            // Play the firing sound
            _fxAudioSource.PlayOneShot(_fireSound1, _fireSound1Volume);

            // Slow the player down
            _rigidbody.AddForce(
                _rigidbody.velocity * -PlayerRope.ROPE_ATTACH_DECELERATION,
                ForceMode.VelocityChange
            );

            // Jump up if grounded
            if (_grounded) { Jump(); }
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
        _rigidbody.drag = NORMAL_DRAG;

        // Spawn ropes
        Vector3 playerPosition = transform.position;
        int birdMask = (1 << HOOKER_LAYER);
        int playerMask = ~(
            1 << PLAYER_LAYER |
            1 << UI_LAYER |
            1 << LASER_LAYER |
            1 << IGNORE_RAYCAST_LAYER
        );
        var nearbyColliders =
            from c in Physics.OverlapSphere(transform.position, PlayerRope.ROPE_MAX_LENGTH, birdMask)
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
                var r = new PlayerRope(3, this, hitInfo);
                _ropes.Add(r);
                if (_ropes.Count >= 6) { break; }
            }
        }
        //Debug.LogFormat("{0} Ropes Attached", _ropes.Count);

        // Play the firing sound
        if (_ropes.Count > 0)
        {
            _fxAudioSource.PlayOneShot(_fireSound1, _fireSound1Volume);
            _rigidbody.drag = ROPE_BOMB_DRAG;
        }
    }

    public Color GetRopeColor(int rope)
    {
        switch (rope)
        {
            case 1:
                return Color.blue;
            case 2:
                return Color.red;
            default:
                return Color.black;
        }
    }

    public Material GetRopeMaterial(int rope)
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

    public Vector3 GetPosition()
    {
        return _rigidbody.position;
    }

    public Vector3 GetVelocity()
    {
        return _rigidbody.velocity;
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        _rigidbody.AddForce(force, mode);
    }

    public void OnDeath(String tag)
    {
        if (!_dead && !_goalReached)
        {
            Debug.LogFormat("Death by {0}", tag);
            switch (tag)
            {
                case LASER_TAG:
                    _fxAudioSource.PlayOneShot(_laserCollisionSound, _laserCollisionSoundVolume);
                    break;
                case BIRD_TAG:
                case BOUNDS_TAG:
                default:
                    _fxAudioSource.PlayOneShot(_collisionSound, _collisionSoundVolume);
                    break;
            }
            _dead = true;
            _rigidbody.AddForce(_rigidbody.velocity * -0.5f, ForceMode.VelocityChange);
            _hudFade.FadeTo(Color.black, _loadLevelDelay * 0.5f);
            _hudFade.fadeText.text = "OUCH!";
            _hudFade.reticle.gameObject.SetActive(false);

            foreach (var r in _ropes)
            {
                r.Dispose();
            }
            _ropes.Clear();

            Time.timeScale = 1.0f;
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
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnCollisionEnter(Collision col)
    {
        String tag = col.gameObject.tag;
        switch (tag)
        {
            case PLATFORM_TAG:
                _rigidbody.AddForce(_rigidbody.velocity * -0.5f, ForceMode.VelocityChange);
                break;
            case GOAL_TAG:
                OnGoalReached();
                break;
            case LASER_TAG:
            case BIRD_TAG:
                OnDeath(tag);
                break;
            default:
                break;
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        String tag = col.gameObject.tag;
        switch (tag)
        {
			case NEAR_BIRD_TAG:
            	_fxAudioSource.PlayOneShot(_nearBirdSound, _nearBirdSoundVolume);
                break;
            case GOAL_TAG:
                OnGoalReached();
                break;
            case LASER_TAG:
            case BIRD_TAG:
                OnDeath(tag);
                break;
            case PLAYER_TAG:
            default:
                break;
        }
    }
}

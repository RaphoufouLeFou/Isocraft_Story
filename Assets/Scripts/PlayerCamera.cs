using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Player player;
    public Camera cam;
    private readonly float _moveDelay = 0.5f, _rotDelay = 0.1f, _zoom = 4, _debounceTime = 0.25f;
    private float _currentRotDelay; // different possible rotation speeds

    // position and rotation
    private Vector3 _currentPos;
    private Vector3 _currentRot;
    private float _lastPlayerY;

    // rotation target
    private Vector3 _goalPos;
    [NonSerialized] public Vector3 GoalRot;
    [NonSerialized] private bool _targetAbove;

    // mouse movement
    private Vector2 _prevMousePos;
    private float _startMouseShift = -1000f; // avoid playing the animation at the start
    private float _mouseOffset = -1000f;

    private void MouseMovement()
    {
        // change camera depending on mouse position
        Vector2 pos = Input.mousePosition;
        int w = Screen.width, h = Screen.height;
        float x = pos.x / w, y = pos.y / h;
        if (x < 0 || x > 1 || y < 0 || y > 1) return; // outside of screen

        // allow to rotate multiple times in a row, leave time to react and not rotate twice
        bool debounce = Time.time > _startMouseShift + _debounceTime;

        bool change = false;
        if (_mouseOffset > 0) _mouseOffset -= Time.deltaTime * 3; // add hysteresis
        else _mouseOffset = 0;
        if (x < 0.2f)
        {
            if (pos.x + 1.5f < _prevMousePos.x && debounce)
            {
                GoalRot.y -= 45;
                change = true;
            }

            _mouseOffset = 1;
        }
        else if (x > 0.8f)
        {
            if (pos.x - 1.5f > _prevMousePos.x && debounce)
            {
                GoalRot.y += 45;
                change = true;
            }

            _mouseOffset = 1;
        }

        if (y < 0.3f) _targetAbove = true;
        else if (y > 0.8f) _targetAbove = false;

        if (change) _startMouseShift = Time.time;
        
        // move the mouse towards the center if needed
        if (_mouseOffset > 0)
        {
            float goalX = x < 0.5f ? w * 0.3f : w * 0.7f;
            if ((x < 0.5f) ^ (pos.x < goalX)) _mouseOffset = 0; // stop animation early if moved back to the center
            float fps = Time.deltaTime == 0 ? 10e6f : _rotDelay / Time.deltaTime;
            pos.x = (pos.x * (fps - 1) + goalX) / fps;
            Mouse.current.WarpCursorPosition(pos);
        }

        _prevMousePos = pos;
    }
    
    private void Start()
    {
        GoToPlayer();
    }

    public void GoToPlayer()
    {
        // start by looking down at the player
        _currentPos = player.transform.position + new Vector3(0, 3, 0);
        _currentRot = new Vector3(90, GoalRot.y, 0);
    }

    private void Update()
    {
        
        // change camera target with mouse movement
        if (Settings.Playing && Application.isFocused) MouseMovement();

        if (player.Body == null) return; // if this is not the current player, skip 

        Transform tr = transform;
        Vector3 pPos = player.transform.position;
        if (_lastPlayerY < 0) pPos.y = 0;
        if (_lastPlayerY > Chunk.Size + 2) pPos.y = Chunk.Size + 2; 
        Vector3 m = player.Body.Movement;

        // only update target height if the player is falling or on the ground
        if (m.y == 0 || (m.y < 0 && pPos.y < player.GroundedHeight)) _lastPlayerY = player.transform.position.y;

        // edit target position: use last Y, move camera when walking up/down, rotate when walking left/right
        pPos.y = _lastPlayerY + player.Body.MoveRelative.z * (m.x * m.x + m.z * m.z) * 3;
        float goalRotY = GoalRot.y + player.Body.MoveRelative.x * 5; 

        float fps = Time.deltaTime == 0 ? 10e6f : 1 / Time.deltaTime;
        float posFps = _moveDelay * fps, rotFps = _rotDelay * (1 + _lastPlayerY / 8) * fps;
        
        // fix y rotation 360 wrapping
        float currentRotY = _currentRot.y;
        while (currentRotY - goalRotY > 180) goalRotY += 360;
        while (goalRotY - currentRotY > 180) currentRotY += 360;
        
        // set target X rotation
        float targetX = _targetAbove ? 90 : 80 - _lastPlayerY * 4;
        
        // smoothly interpolate according to fps:
        // (current * (fps-1) + goal) / fps
        float posFps1 = posFps - 1, rotFps1 = rotFps - 1;
        _currentPos = (_currentPos * posFps1 + pPos) / posFps;
        _currentRot.x = (_currentRot.x * rotFps1 + targetX) / rotFps;
        _currentRot.y = (currentRotY * rotFps1 + goalRotY) / rotFps;
        cam.orthographicSize = (cam.orthographicSize * posFps1 + _zoom * (1 + _lastPlayerY / 10)) / posFps;
        
        // update transform: go to currentPos, rotate, then move back
        tr.rotation = Quaternion.Euler(_currentRot);
        tr.position = _currentPos + tr.rotation * new Vector3(0, 0, -20);
    }
}

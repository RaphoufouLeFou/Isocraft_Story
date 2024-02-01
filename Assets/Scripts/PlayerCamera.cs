using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Player player;
    public Camera cam;
    private readonly float _moveDelay = 0.5f, _rotDelay = 0.3f, _zoom = 4;
    private float _currentRotDelay; // different possible rotation speeds

    private Vector3 _currentPos;
    private Vector3 _currentRot;
    private float _lastPlayerY;
    private float _startMouseShift = -1000f; // avoid playing the animation at the start
    private Vector2 _startMousePos, _goalMousePos;
    private Vector2 _prevMousePos;

    private Vector3 _goalPos;
    [NonSerialized] public Vector3 GoalRot;
    [NonSerialized] private bool _targetAbove;

    private void MouseMovement()
    {
        // change camera depending on mouse position
        Vector2 pos = Input.mousePosition;
        int w = Screen.width, h = Screen.height;
        float x = pos.x / w, y = pos.y / h;
        int change = 0;
        bool debounce = Time.time > _startMouseShift + 0.2f; // leave time to react and not rotate twice

        if (x < 0.2f && pos.x + 0.5f < _prevMousePos.x && debounce)
        {
            // allow to rotate multiple times in a row
            GoalRot.y -= 45;
            change = 1;
        }
        else if (x > 0.8f && pos.x - 0.5f > _prevMousePos.x && debounce)
        {
            GoalRot.y += 45;
            change = 1;
        }

        if (y < 0.3f && !_targetAbove)
        {
            _targetAbove = true;
            change = 2;
        }
        else if (y > 0.8f && _targetAbove)
        {
            _targetAbove = false;
            change = 2;
        }
        
        if (change != 0) // initiate mouse movement when changing rotation
        {
            _startMousePos = pos;
            Vector2 goal = pos;
            if (change == 1) // x shift
            {
                if (goal.x < w >> 1) goal.x += w * 0.1f;
                else goal.x -= w * 0.1f;
            }
            else // y shift
            {
                if (goal.y < h >> 1) goal.y += h * 0.1f;
                else goal.y -= h * 0.1f;
            }

            _goalMousePos = goal;
            _startMouseShift = Time.time;
        }
        
        // move mouse according to rotation changes
        float t = (Time.time - _startMouseShift) / _rotDelay / 2f;
        if (t < 1)
        {
            // edit _goalMousePos according to player mouse movement
            Vector2 delta = pos - _prevMousePos;
            if (delta != new Vector2())
            {
                _goalMousePos += delta;
                _startMousePos += delta;
            }
            
            // update pos for _prevMousePos setting
            pos = _startMousePos + (_goalMousePos - _startMousePos) * Game.SmoothStep(t); 
            Mouse.current.WarpCursorPosition(new Vector2((float)Math.Round(pos.x), (float)Math.Round(pos.y)));
        }

        _prevMousePos = pos;
    }
    
    private void Start()
    {
        // force starting position to avoid clipping in ground
        _currentPos = player.transform.position + new Vector3(0, 3, 0);
        _currentRot = SaveInfos.HasBeenLoaded ? SaveInfos.PlayerRotation : new Vector3(45, 0, 0);
    }

    private void Update()
    {
        // change camera target with mouse movement
        MouseMovement();

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

        float fps = Time.deltaTime == 0 ? 10e6f : _moveDelay / Time.deltaTime;
        float posFps = _moveDelay * fps, rotFps = _rotDelay * (1 + _lastPlayerY / 4) * fps;
        
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
        SaveInfos.PlayerRotation = _currentRot;
    }
}

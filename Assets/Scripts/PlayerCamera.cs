using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Player player;
    public Camera cam;
    private readonly float _moveDelay = 0.5f, _rotDelay = 0.3f, _zoom = 4;
    private float _currentRotDelay; // different possible rotation speeds

    private Vector3 _currentPos;
    private Vector3 _currentRot;
    private float _lastPlayerY;

    private Vector3 _goalPos;
    [NonSerialized] public Vector3 GoalRot;
    [NonSerialized] public bool TargetAbove = false;
    
    void Start()
    {
        // force starting position to avoid clipping in ground
        _currentPos = player.transform.position + new Vector3(0, 3, 0);
        _currentRot = new Vector3(45, 0, 0);
    }

    void Update()
    {
        if (player.Body == null) return;  // if this is not the current player, skip 

        Transform tr = transform;
        Vector3 pPos = player.transform.position;
        if (_lastPlayerY < 0) pPos.y = 0;
        if (_lastPlayerY > Chunk.Size + 2) pPos.y = Chunk.Size + 2; 
        Vector3 m = player.Body.Movement;

        // don't update the player height if it is moving up, or jumping into no above block
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
        float targetX = TargetAbove ? 90 : 80 - _lastPlayerY * 4;
        
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

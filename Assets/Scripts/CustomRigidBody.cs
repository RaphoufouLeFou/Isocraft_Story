using System;
using System.Collections.Generic;
using UnityEngine;

public enum BodyMovement
{
    Absolute,
    Relative
}

public interface IBody
{
    public Vector3 Movement { get; }
    public Vector3 MoveRelative { get; }
    public BodyMovement MovementType { get; set; }
    public bool OnFloor { get; }

    public void Stop();
    public void Update();
}

public abstract class CustomRigidBody : IBody
{
    private Vector3 _movement = Vector3.zero;
    private Vector3 _moveRelative;
    public Vector3 Movement => _movement;
    public Vector3 MoveRelative => _moveRelative;
    public bool OnFloor { get; private set; }
    public BodyMovement MovementType { get; set; } = BodyMovement.Relative;

    private readonly Transform _transform;
    private readonly float _speed, _drag, _jumpForce, _gravity;
    private bool _sprinting;

    private readonly float _width, _height;

    protected CustomRigidBody(Transform transform, float speed, float drag,
                           float jumpForce, float gravity, float width, float height)
    {
        _transform = transform;
        _speed = speed;
        _drag = drag;
        _jumpForce = jumpForce;
        _gravity = gravity;
        _width = width / 2;
        _height = height / 2;
    }

    private float Positive(float x)
    {
        return x > 0 ? x : 0;
    }

    private float Negative(float x)
    {
        return x < 0 ? x : 0;
    }

    private float Min3Abs(float a, float b, float c)
    {
        if (a < 0) a = -a;
        if (b < 0) b = -b;
        if (c < 0) c = -c;
        return a < b && b < c ? a : b < c ? b : c;
    }

    void CheckCollisions(Vector3 pos)
    {
        OnFloor = false;

        Vector3 movement = pos - _transform.position;

        int chunkX = (int)MathF.Floor(pos.x / Chunk.Size);
        int chunkZ = (int)MathF.Floor(pos.z / Chunk.Size);

        // check collisions with chunks around
        for (int i = chunkX - 1; i < chunkX + 2; i++)
        for (int j = chunkZ - 1; j < chunkZ + 2; j++)
        {
            // no collisions for unloaded chunks
            if (!MapManagement.Chunks.TryGetValue(Chunk.GetName(i, Game.Player.Level, j), out Chunk chunk))
            {
                if (i == chunkX && j == chunkZ) return; // sitting in unloaded chunk
                continue;
            }

            // check collisions with blocks around
            // only calculate collisions with the block with the most depth
            Vector3 p = pos - new Vector3(i * Chunk.Size, 0, j * Chunk.Size); // pos in the chunk
            List<Quaternion> corrections = new List<Quaternion>();
            for (int x = (int)p.x - 1; x < (int)p.x + 2; x++)
            for (int y = (int)p.y - 3; y < (int)p.y + 3; y++)
            for (int z = (int)p.z - 1; z < (int)p.z + 2; z++)
            {
                if (x < 0 || x >= Chunk.Size || y < 0 || y >= Chunk.Size || z < 0 ||
                    z >= Chunk.Size) continue;
                if (Game.Blocks.FromId[chunk.Blocks[x, y, z]].NoCollide) continue;
                if (x + 1 > p.x - _width && x < p.x + _width &&
                    y + 1 > p.y - _height && y < p.y + _height &&
                    z + 1 > p.z - _width && z < p.z + _width)
                {
                    // collision happens
                    float dx = x < p.x ? Positive(x + 1 - p.x + _width) : Negative(x - p.x - _width);
                    float dy = y <= p.y ? Positive(y + 1 - p.y + _height) : Negative(y - p.y - _height);
                    float dz = z < p.z ? Positive(z + 1 - p.z + _width) : Negative(z - p.z - _width);
                    float dist = Min3Abs(dx, dy, dz);
                    int k = 0;
                    for (; k < corrections.Count; k++)
                        if (corrections[k].w <= dist)
                            break;
                    corrections.Insert(k, new Quaternion(dx, dy, dz, dist));
                }
            }

            // get final collision
            Vector3 correction = Vector3.zero;
            foreach (Quaternion corr in corrections)
            {
                // correct position in the direction with the least depth
                int toChange = -1; // 0: x, 1: y, 2: z
                float min = 1000;
                for (int k = 0; k < 3; k++)
                {
                    float coords = MathF.Abs(corr[k]);
                    if (coords != 0 && coords < min)
                    {
                        min = coords;
                        toChange = k;
                    }
                }

                if (toChange == 0) // x wall collision
                {
                    if (MathF.Abs(corr.x) > MathF.Abs(correction.x)) correction.x = corr.x;
                    _sprinting = false; // reset sprint when bonking on wall
                }

                else if (toChange == 1) // floor / ceiling collision
                {
                    if (MathF.Abs(corr.y) > MathF.Abs(correction.y))
                    {
                        correction.y = corr.y;
                        if (movement.y <= 0) break; // stop checking for collisions if on the ground
                    }
                }

                else if (toChange == 2) // z wall collision
                {
                    if (MathF.Abs(corr.z) > MathF.Abs(correction.z)) correction.z = corr.z;
                    _sprinting = false;
                }

                else throw new PlayerException("Way too much correction, gotta be an error somewhere");
            }

            // move by final collision
            if (correction.x != 0)
            {
                _movement.x = 0;
                pos.x += correction.x;
            }

            if (correction.y != 0)
            {
                if (movement.y <= 0) OnFloor = true; // floor collision
                _movement.y = 0;
                pos.y += correction.y;
            }

            if (correction.z != 0)
            {
                _movement.z = 0;
                pos.z += correction.z;
            }
        }

        _transform.position = pos;
    }

    protected float GetDelta()
    {
        // capped movement speed
        float delta = Time.deltaTime;
        return delta > 0.1f ? 0.1f : delta;
    }

    protected void Update(float x, float z, float delta)
    {
        // not gonna check, but x and z must be <= 1 in absolute value
        _moveRelative = new Vector3(x * 0.8f, 0, z).normalized;
        Vector3 move = MovementType == BodyMovement.Absolute ? _moveRelative : _transform.rotation * _moveRelative;
        float speed = _sprinting ? 1.7f * _speed : _speed;
        _movement += move * (speed * delta);

        // move according to Movement
        float drag = MathF.Pow(_drag, 100 * delta);
        _movement.x *= drag;
        _movement.y += _gravity * delta;
        _movement.z *= drag;

        Vector3 newPos = _transform.position + _movement * (delta * _speed);
        CheckCollisions(newPos);
    }

    // overriden by children, this is the method called from the mob's Update
    public abstract void Update();

    protected void StartSprinting()
    {
        _sprinting = true;
    }

    protected void StopSprinting()
    {
        _sprinting = false;
    }

    protected void Jump()
    {
        _movement.y = _jumpForce;
    }

    public void Stop()
    {
        _movement = Vector3.zero;
    }
}

// body set up with the planet's constants
public abstract class PlanetBody : CustomRigidBody
{
    protected PlanetBody(Transform transform) : base(transform, 8, 0.9f, 1.3f, -5, 0.95f, 1.85f) { }
}

// body with AI controller
public class MobBody : PlanetBody, IBody
{
    private readonly Func<(float side, float forwards)> _moveFunc;

    public MobBody(Transform transform, Func<(float side, float forwards)> moveFunc) : base(transform)
    {
        _moveFunc = moveFunc;
    }

    public override void Update()
    {
        float delta = GetDelta();
        (float x, float z) = _moveFunc();
        base.Update(x, z, delta);
    }
}

public class PlayerBody : PlanetBody, IBody
{
    public PlayerBody(Transform transform) : base(transform) { }

    public override void Update()
    {
        float delta = GetDelta();

        if (Settings.IsPaused) return; // keys movement

        float x = 0;
        float z = 0;
        bool forwards = Input.GetKey(Settings.KeyMap["Forwards"]);
        bool backwards = Input.GetKey(Settings.KeyMap["Backwards"]);
        if (forwards) z++;
        if (backwards) z--;
        if (Input.GetKey(Settings.KeyMap["Left"])) x--;
        if (Input.GetKey(Settings.KeyMap["Right"])) x++;

        if (Input.GetKey(Settings.KeyMap["Sprint"])) StartSprinting();
        if (!forwards || backwards) StopSprinting();

        if (Input.GetKey(Settings.KeyMap["Jump"]) && OnFloor) Jump();
        base.Update(x, z, delta);
    }
}

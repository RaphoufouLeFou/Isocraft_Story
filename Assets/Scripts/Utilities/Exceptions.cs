using System;

public class ColliderException : Exception
{
    public ColliderException(string message = "Collider exception") : base(message) { }
}

public class BlockException : Exception
{
    public BlockException(string message = "Block exception") : base(message) { }
}

public class GenerationException : Exception
{
    public GenerationException(string message = "Generation exception") : base(message) { }
}
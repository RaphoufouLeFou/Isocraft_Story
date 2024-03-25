﻿using System;

public class CustomException : Exception
{
    protected CustomException(string message) : base(message)
    {
        Game.QuitGame();
    }
}

public class PlayerException : CustomException
{
    public PlayerException(string message = "Player exception") : base(message) { }
}

public class BlockException : CustomException
{
    public BlockException(string message = "Block exception") : base(message) { }
}

public class GenerationException : CustomException
{
    public GenerationException(string message = "Generation exception") : base(message) { }
}

public class CommandException : Exception
{
    public CommandException(string message = "Invalid command syntax") : base(message) { }
}
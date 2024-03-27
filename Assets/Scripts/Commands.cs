using System;
using System.Collections.Generic;
using UnityEngine;

public static class Commands
{
    private static List<string> Parse(string command)
    {
        // temporary (?)
        List<string> l = new();
        foreach (string s in command.Split(" "))
            if (s != "")
                l.Add(s.ToLower());

        return l;
    }

    public static void ExecuteCommand(string command)
    {
        List<string> args = Parse(command);
        if (args.Count == 0) throw new CommandException("Empty command");
        string cmd = args[0];
        args.RemoveAt(0);
        switch (cmd)
        {
            case "tick": Tick(args);
                break;
            case "quit": Game.QuitGame();
                break;
            case "give": Give(args);
                break;
            default: throw new CommandException($"Unknown command: {cmd}");
        }
    }

    private static void Tick(List<string> args)
    {
        if (args.Count != 2) throw new CommandException();
        switch (args[0])
        {
            case "rate":
                try
                {
                    Game.TickRate = int.Parse(args[1]);
                    Debug.Log($"Changed tick rate to {Game.TickRate}");
                }

                catch
                {
                    throw new CommandException("Invalid value");
                }

                break;
            default: throw new CommandException();
        }
    }

    private static void Give(List<string> args)
    {
        throw new NotImplementedException();
    }
}

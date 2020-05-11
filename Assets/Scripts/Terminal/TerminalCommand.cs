using System;

public class TerminalCommand
{
    public string desc;
    public Action action;
    public string[] examples;
    public bool hiddenFromList;

    public TerminalCommand(string Description, Action Action, bool HiddenFromList, params string[] Examples)
    {
        desc = Description;
        action = Action;
        examples = Examples;
        hiddenFromList = HiddenFromList;
    }

    public void ExecuteCommand()
    {
        action();
    }
}
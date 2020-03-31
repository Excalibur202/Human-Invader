using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerminalController : MonoBehaviour
{
    public TextMeshProUGUI log;
    public TextMeshProUGUI input;

    Dictionary<string, TerminalCommand> commands = new Dictionary<string, TerminalCommand>();

    public List<string> logStrings = new List<string>();
    public Queue<char> logBuffer = new Queue<char>();
    bool outputtingBuffer;

    string inputString;
    Mutex bufferMutex = new Mutex();

    //public List<RoomInfo> roomInfo = new List<RoomInfo>();

    private void Start()
    {
        //StartCoroutine(ConnectToMap());

        Action HELP = () =>
        {
            string[] inputArray = inputString.Split(' ');

            switch (inputArray.Length)
            {
                case 1:
                    AddStringToLog("Available commands:");
                    foreach (string cmd in commands.Keys)
                        if (!commands[cmd].hiddenFromList)
                            AddStringToLog(cmd);
                    break;
                case 2:
                    if (commands.ContainsKey(inputArray[1]))
                    {
                        AddStringToLog(inputArray[1] + " -> " + commands[inputArray[1]].desc + "\nExamples:");
                        foreach (string s in commands[inputArray[1]].examples)
                            AddStringToLog(s);
                    }
                    else
                        AddStringToLog("INPUT ERROR! That command does not exist.");
                    break;
                default:
                    AddStringToLog("INPUT ERROR! The command HELP can only be used by itself or with another command.");
                    break;
            }
        };
        commands.Add("HELP", new TerminalCommand("Lists most available commands, or returns the description of a command", HELP, false,
        "'HELP'",
        "'HELP [COMMAND]'"));

        Action LIST = () =>
        {
            string[] inputArray = inputString.Split(' ');

            switch (inputArray.Length)
            {
                case 1:
                    int lastSector = -1;
                    //foreach (RoomInfo room in roomInfo)
                    //{
                    //    if (room.sector != lastSector)
                    //    {
                    //        AddStringToLog(room.sector.ToString());
                    //        lastSector = room.sector;
                    //    }
                    //}
                    break;
                default:
                    AddStringToLog("INPUT ERROR!");
                    break;
            }
        };
        commands.Add("LIST", new TerminalCommand("Lists stuff", LIST, false,
        "'LIST'",
        "'LIST [NAME]'"));

        AddStringToLog("Enter 'HELP' to list the available commands.\nEnter 'HELP [COMMAND]' to get more details about a command.\n");
    }

    public void ParseInput()
    {
        inputString = input.text.ToUpper();

        MoveInputToLog();

        // If valid input
        if (InputDiscrepancyCheck())
        {
            // Try to match input with a valid command
            string inputtedCommand = inputString.Split(' ')[0]; // get first word

            if (commands.ContainsKey(inputtedCommand))
                commands[inputtedCommand].action.Invoke();
            else
                AddStringToLog("INPUT ERROR! " + inputtedCommand + " is not a valid command!");
        }

        AddStringToLog("");
    }

    // Check inputted string for general input imcompatibilities (true good, false bad)
    public bool InputDiscrepancyCheck()
    {
        bool errorFound = false;

        // Any characters that are not in the ranges A-Z, a-z, 0-9 or _
        if (!inputString.All(c => char.IsLetterOrDigit(c) || c == '_' || c == ' '))
        {
            AddStringToLog("INPUT ERROR! Your input had invalid characters!");
            errorFound = true;
        }

        // Too long input?
        if (inputString.Length > 30)
        {
            AddStringToLog("INPUT ERROR! Your input had over 30 characters!");
            errorFound = true;
        }

        // Too many words?
        if (inputString.Split(' ').Length > 3)
        {
            AddStringToLog("INPUT ERROR! Your input had more than 3 words!");
            errorFound = true;
        }

        // Two spaces in a row anywhere?
        if (inputString.IndexOf(" " + " ") != -1)
        {
            AddStringToLog("INPUT ERROR! Your input had multiple spaces in a row!");
            errorFound = true;
        }

        // Return true if no errors were found
        return !errorFound;
    }

    public void MoveInputToLog()
    {
        logStrings.Add("> " + input.text + "\n");
        input.text = "";

        UpdateLogText();
    }

    public void AddStringToLog(string input)
    {
        logStrings.Add(input + "\n");

        UpdateLogText();
    }

    // Take the latest string in the log strings and add it to the terminal's log text buffer
    public void UpdateLogText()
    {
        string lastString = logStrings[logStrings.Count - 1];
        for (int i = 0; i < lastString.Length; i++)
            logBuffer.Enqueue(lastString[i]);

        bufferMutex.WaitOne();
        if (!outputtingBuffer)
            StartCoroutine(LogBufferInputter());
        bufferMutex.ReleaseMutex();
    }

    private IEnumerator LogBufferInputter()
    {
        outputtingBuffer = true;

        while (logBuffer.Count > 0)
        {
            log.text += logBuffer.Dequeue();

            print(logBuffer);

            yield return new WaitForSeconds(Random.Range(0.01f, 0.04f));
        }

        bufferMutex.WaitOne();
        outputtingBuffer = false;
        bufferMutex.ReleaseMutex();
    }

    //private IEnumerator ConnectToMap()
    //{
    //    while (roomInfo.Count < 1)
    //    {
    //        roomInfo = GameObject.FindGameObjectWithTag("MapGen").GetComponent<MapGenerator>().spawnedRooms;

    //        yield return new WaitForSeconds(0.25f);
    //    }
    //}

}

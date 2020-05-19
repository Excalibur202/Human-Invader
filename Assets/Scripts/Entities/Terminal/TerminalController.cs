using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerminalController : MonoBehaviour {
    public TextMeshProUGUI log;
    public TextMeshProUGUI input;

    Dictionary<string, TerminalCommand> commands = new Dictionary<string, TerminalCommand> ();

    public List<string> logStrings = new List<string> ();
    public Queue<char> logBuffer = new Queue<char> ();
    bool outputtingBuffer;

    string inputString;
    Mutex bufferMutex = new Mutex ();

    public List<RoomInfo> roomInfo = new List<RoomInfo> ();

    public void Initialize () {
        if (GameObject.FindGameObjectWithTag ("MapGen"))
            StartCoroutine (ConnectToMap ());

        LoadCommands ();

        AddStringToLog (
            "Welcome, user [ERROR_NaN_error].",
            "You may exit this interface at anytime by pressing ESC.\n",
            "Enter 'HELP' to list the available commands.",
            "Enter 'HELP [COMMAND]' to learn more about an existing command.\n");
    }

    private void LoadCommands () {
        //Help
        Action HELP = () => {
            string[] inputArray = inputString.Split (' ');

            switch (inputArray.Length) {
                case 1:
                    AddStringToLog ("Available commands:");
                    foreach (string cmd in commands.Keys)
                        if (!commands[cmd].hiddenFromList)
                            AddStringToLog (cmd);
                    break;
                case 2:
                    if (commands.ContainsKey (inputArray[1])) {
                        AddStringToLog (inputArray[1] + " -> " + commands[inputArray[1]].desc + "\nExamples:");
                        foreach (string s in commands[inputArray[1]].examples)
                            AddStringToLog (s);
                    } else
                        AddStringToLog ("INPUT ERROR! That command does not exist.");
                    break;
                default:
                    AddStringToLog ("INPUT ERROR! The command HELP can only be used by itself or with another command.");
                    break;
            }
        };
        commands.Add ("HELP", new TerminalCommand ("Lists most available commands, or returns the description of a command", HELP, false,
            "'HELP'",
            "'HELP [COMMAND]'"));

        //List
        Action LIST = () => {
            string[] inputArray = inputString.Split (' ');

            switch (inputArray.Length) {
                case 1:
                    int lastSector = -1;
                    foreach (RoomInfo room in roomInfo) {
                        if (room.sector != lastSector) {
                            AddStringToLog (room.sector.ToString ());
                            lastSector = room.sector;
                        }
                    }
                    break;
                default:
                    AddStringToLog ("INPUT ERROR!");
                    break;
            }
        };
        commands.Add ("LIST", new TerminalCommand ("Lists information from the map database", LIST, false,
            "'LIST'",
            "'LIST [FILTER STRING]'"));

        //MapDownload
        Action MapDownload = () => {
            string[] inputArray = inputString.Split (' ');

            switch (inputArray.Length) {
                case 1:
                    int sector = transform.root.GetComponent<RoomEntrance> ().sector;
                    if (MapToTexture.instance.DrawSector (sector))
                        AddStringToLog ("Sector info downloaded to interface." + sector);
                    else
                        AddStringToLog ("ERROR! Couldn't find the door's sector.");
                    break;

                default:
                    AddStringToLog ("INPUT ERROR! The command MAP_DOWNLOAD can only be used by itself.");
                    break;
            }
        };
        commands.Add ("MAP_DOWNLOAD", new TerminalCommand ("Downloads the information of sector you are in to your map.", MapDownload, false,
            "'MAP_DOWNLOAD'"));

        //OpenDoor
        Action DoorUnlock = () => {
            string[] inputArray = inputString.Split (' ');

            switch (inputArray.Length) {
                case 1:
                    if (Util.GetPlayer ().GetComponent<PlayerAbilityController> ().keycards > 0) {
                        Util.GetPlayer ().GetComponent<PlayerAbilityController> ().keycards--;
                        transform.GetComponentInParent<DoorCrl> ().OpenDoor ();
                        AddStringToLog ("SUCCESS! The door is now open.");
                    } else
                        AddStringToLog ("ERROR! You need a keycard to unlock the door.");
                    break;

                default:
                    AddStringToLog ("INPUT ERROR! The command DOOR_UNLOCK can only be used by itself.");
                    break;
            }
        };
        commands.Add ("DOOR_UNLOCK", new TerminalCommand ("Unlocks the door next to the terminal, if you have a keycard.", DoorUnlock, false,
            "'DOOR_UNLOCK'"));
    }

    public void AddStringToInput (string inputtedString) {
        // Parse backspaces
        for (int i = inputtedString.Length - 1; i >= 0; i--) {
            if (i < inputtedString.Length) {
                if (inputtedString[i] == '\b') {
                    inputtedString = inputtedString.Remove (i);
                    if (inputtedString.Length > 0)
                        inputtedString = inputtedString.Remove (inputtedString.Length - 1);
                    else if (input.text.Length > 0)
                        input.text = input.text.Remove (input.text.Length - 1);
                } else
                    // Remove unwanted characters
                    if (inputtedString[i] < 32 || inputtedString[i] > 126)
                        inputtedString = inputtedString.Remove (i);
            }
        }

        input.text += inputtedString;
    }

    public void ParseInput () {
        if (input.text.Length > 0) {
            inputString = input.text.ToUpper ();

            MoveInputToLog ();

            // If valid input
            if (InputDiscrepancyCheck ()) {
                // Try to match input with a valid command
                string inputtedCommand = inputString.Split (' ') [0]; // get first word

                if (commands.ContainsKey (inputtedCommand))
                    commands[inputtedCommand].action.Invoke ();
                else
                    AddStringToLog ("INPUT ERROR! " + inputtedCommand + " is not a valid command!");
            }

            AddStringToLog ("");
        }
    }

    // Check inputted string for general input imcompatibilities (true good, false bad)
    private bool InputDiscrepancyCheck () {
        bool errorFound = false;

        // Any characters that are not in the ranges A-Z, a-z, 0-9 or _
        if (!inputString.All (c => char.IsLetterOrDigit (c) || c == '_' || c == ' ')) {
            AddStringToLog ("INPUT ERROR! Your input had invalid characters!");
            errorFound = true;
        }

        // Too long input?
        if (inputString.Length > 30) {
            AddStringToLog ("INPUT ERROR! Your input had over 30 characters!");
            errorFound = true;
        }

        // Too many words?
        if (inputString.Split (' ').Length > 3) {
            AddStringToLog ("INPUT ERROR! Your input had more than 3 words!");
            errorFound = true;
        }

        // Two spaces in a row anywhere?
        if (inputString.IndexOf (" " + " ") != -1) {
            AddStringToLog ("INPUT ERROR! Your input had multiple spaces in a row!");
            errorFound = true;
        }

        // Return true if no errors were found
        return !errorFound;
    }

    private void MoveInputToLog () {
        logStrings.Add ("> " + input.text + "\n");
        input.text = "";

        UpdateLogText ();
    }

    private void AddStringToLog (params string[] input) {
        for (int i = 0; i < input.Length; i++) {
            logStrings.Add (input[i] + "\n");
            UpdateLogText ();
        }
    }

    // Take the latest string in the log strings and add it to the terminal's log text buffer
    private void UpdateLogText () {
        string lastString = logStrings[logStrings.Count - 1];
        for (int i = 0; i < lastString.Length; i++)
            logBuffer.Enqueue (lastString[i]);

        bufferMutex.WaitOne ();
        if (!outputtingBuffer)
            StartCoroutine (LogBufferInputter ());
        bufferMutex.ReleaseMutex ();
    }

    private IEnumerator LogBufferInputter () {
        outputtingBuffer = true;

        while (logBuffer.Count > 0) {
            log.text += logBuffer.Dequeue ();

            if (log.text.Last () == '\n')
                yield return new WaitForSeconds (Random.Range (0.4f, 0.9f));
            else
                yield return new WaitForSeconds (Random.Range (0.01f, 0.05f));
        }

        bufferMutex.WaitOne ();
        outputtingBuffer = false;
        bufferMutex.ReleaseMutex ();
    }

    private IEnumerator ConnectToMap () {
        while (roomInfo.Count < 1) {
            roomInfo = MapGenerator.instance.spawnedRooms;

            yield return new WaitForSeconds (0.25f);
        }
    }
}
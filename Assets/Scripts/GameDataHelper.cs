﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GameDataHelper
{
    private const string DATA_FOLDER_PATH = "/Data";
    private const string QUESTIONNAIRE_TITLE = "Questionnaire_";
    private const string CORRECTNESS_DATA_TITLE = "/CorrectnessData_";
    private const string REACTION_DATA_TITLE = "/ReactionData_";

    private int sessionsCompleted = 0;

    public bool isGroupAB { get; private set; }
    public bool isStudyComplete => sessionsCompleted == 2;
    public bool isFirstSession => sessionsCompleted == 0;
    public bool isPitchNext
    {
        get
        {
            if ((isGroupAB && sessionsCompleted == 0) || (!isGroupAB && sessionsCompleted == 1))
            {
                return true;
            }
            return false;
        }
    }

    private string dataPath;
    private int playSessionID;
    private List<int> correctnessData;
    private List<double> reactionData;

    public int PlaySessionID => playSessionID;
    public string StudyGroup
    {
        get
        {
            if (playSessionID % 2 == 0)
            {
                return "AB";
            }
            return "BA";
        }
    }

    private static GameDataHelper instance = null;
    public static GameDataHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameDataHelper();
            }
            return instance;
        }
    }

    private GameDataHelper()
    {
        correctnessData = new List<int>();
        reactionData = new List<double>();
        dataPath = Application.streamingAssetsPath + DATA_FOLDER_PATH;
        CheckAndCreateDataPath();
        playSessionID = CreateDirectoriesAndGetNextPlaySessionID();
        isGroupAB = (playSessionID % 2 == 0);
    }

    public void RestartInstance()
    {
        instance = new GameDataHelper();
    }

    public void SubmitQuestionnaire(int[] selected)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int i in selected)
        {
            sb.Append(i.ToString() + '\n');
        }
        File.WriteAllText(dataPath + $"/{playSessionID}/" + QUESTIONNAIRE_TITLE + playSessionID + ".txt", sb.ToString());
    }

    public void SubmitPlayData(int[] correctnessData, double[] reactionData)
    {
        this.correctnessData.AddRange(correctnessData);
        this.reactionData.AddRange(reactionData);

        if (isStudyComplete)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in this.correctnessData)
            {
                sb.Append(i.ToString() + '\n');
            }
            File.WriteAllText(dataPath + $"/{playSessionID}/" + CORRECTNESS_DATA_TITLE + $"{StudyGroup}_" + playSessionID + ".txt", sb.ToString());

            sb = new StringBuilder();
            foreach (double d in this.reactionData)
            {
                sb.Append(d.ToString() + '\n');
            }
            File.WriteAllText(dataPath + $"/{playSessionID}/" + REACTION_DATA_TITLE + $"{StudyGroup}_" + playSessionID + ".txt", sb.ToString());
        }
    }
    
    public void SessionFinished()
    {
        sessionsCompleted++;
    }

    private int CreateDirectoriesAndGetNextPlaySessionID()
    {
        string[] directories = Directory.GetDirectories(dataPath);
        int index = directories.Length;
        Directory.CreateDirectory(dataPath + $"/{index}");
        return index;
    }

    private void CheckAndCreateDataPath()
    {
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
    }
}

using System.IO;
using System.Text;
using UnityEngine;

public class DataHelper
{
    private const string DATA_FOLDER_PATH = "/Data";
    private const string QUESTIONNAIRE_TITLE = "Questionnaire_";
    private const string PLAY_DATA_TITLE = "/PlayData_";
    
    private string dataPath;
    private int playSessionID;

    public int PlaySessionID => playSessionID;

    private static DataHelper instance = null;
    public static DataHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataHelper();
            }
            return instance;
        }
    }

    private DataHelper()
    {
        dataPath = Application.streamingAssetsPath + DATA_FOLDER_PATH;
        CheckAndCreateDataPath();
        playSessionID = CreateDirectoriesAndGetNextPlaySessionID();
    }

    public void RestartInstance()
    {
        instance = new DataHelper();
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

    public void SubmitPlayData(string studyGroup, int[] playData)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int i in playData)
        {
            sb.Append(i.ToString() + '\n');
        }
        File.WriteAllText(dataPath + $"/{playSessionID}/" + PLAY_DATA_TITLE + $"{studyGroup}_" + playSessionID + ".txt", sb.ToString());
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

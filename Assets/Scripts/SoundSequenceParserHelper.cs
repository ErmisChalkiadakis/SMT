using UnityEngine;

public static class SoundSequenceParserHelper
{
    public static int[] ParseTextFile(TextAsset textAsset)
    {
        string text = textAsset.text;
        string[] lines = text.Split('\n');
        int[] result = new int[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            int.TryParse(lines[i], out result[i]);
        }

        return result;
    }
}

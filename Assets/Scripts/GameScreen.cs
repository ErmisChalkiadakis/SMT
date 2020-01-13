using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private GameSequenceHelper gameSequenceHelper;
    [SerializeField] private RawImage fader;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TextAsset[] gameSoundSequences;
    
    private int playSessionID;

    public void Start()
    {
        playSessionID = GameDataHelper.Instance.PlaySessionID;
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        fader.gameObject.SetActive(true);

        Color faderColor = new Color(0f, 0f, 0f, 1f);
        fader.color = faderColor;
        while (fader.color.a > 0)
        {
            faderColor.a -= 1f * Time.deltaTime;
            fader.color = faderColor;
            yield return null;
        }

        int sequenceIndex;
        if (GameDataHelper.Instance.isPitchNext)
        {
            sequenceIndex = 0;
        }
        else
        {
            sequenceIndex = 1;
        }

        gameSequenceHelper.gameObject.SetActive(true);
        audioMixer.gameObject.SetActive(true);
        audioMixer.Initialize(SoundSequenceParserHelper.ParseTextFile(gameSoundSequences[sequenceIndex]));
    }
}

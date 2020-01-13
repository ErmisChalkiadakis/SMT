using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private TutorialSequenceHelper tutorialSequenceHelper;
    [SerializeField] private RawImage fader;
    
    public void Start()
    {
        tutorialSequenceHelper.SetTutorial(GameDataHelper.Instance.isPitchNext);
        tutorialSequenceHelper.OnTutorialCompletedEvent += OnTutorialCompleted;
    }
    
    private void OnTutorialCompleted()
    {
        StartCoroutine(LoadGameScene());
    }
    
    private IEnumerator LoadGameScene()
    {
        fader.gameObject.SetActive(true);
        Color faderColor = new Color(0f, 0f, 0f, 0f);
        fader.color = faderColor;
        while (fader.color.a < 1)
        {
            faderColor.a += 2f * Time.deltaTime;
            fader.color = faderColor;
            yield return null;
        }

        SceneManager.LoadScene(SceneConstants.GAME_SCENE_HASH);
    }
}

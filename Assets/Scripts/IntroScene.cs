using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    [SerializeField] private RawImage fader;

    void Start()
    {
        StartCoroutine(FlashIntroScreen());
    }

    private IEnumerator FlashIntroScreen()
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
        
        yield return new WaitForSecondsRealtime(2f);

        faderColor = new Color(0f, 0f, 0f, 0f);
        fader.color = faderColor;
        while (fader.color.a < 1)
        {
            faderColor.a += 2f * Time.deltaTime;
            fader.color = faderColor;
            yield return null;
        }

        SceneManager.LoadScene(SceneConstants.QUESTIONNAIRE_SCENE_HASH);
    }
}

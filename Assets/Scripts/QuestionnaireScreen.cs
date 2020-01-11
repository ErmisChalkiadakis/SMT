using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionnaireScreen : MonoBehaviour
{
    [SerializeField] private Button submitButton;
    [SerializeField] private InputField ageField;
    [SerializeField] private ToggleGroup[] toggleGroups;
    [SerializeField] private TextMeshProUGUI ageQuestion;
    [SerializeField] private TextMeshProUGUI[] questions;
    [SerializeField] private RawImage fader;

    private DataHelper dataHelper;
    private bool activeCoroutine;

    public void Start()
    {
        StartCoroutine(FadeInQuestionnaire());
        dataHelper = DataHelper.Instance;
    }

    public void OnDestroy()
    {
        dataHelper = null;
    }

    public void TrySubmit()
    {
        if (int.TryParse(ageField.text, out int result))
        {
            if (result < 10 || result > 100)
            {
                if (!activeCoroutine)
                {
                    StartCoroutine(HighlightQuestion(ageQuestion));
                }
                return;
            }
        }
        else
        {
            {
                StartCoroutine(HighlightQuestion(ageQuestion));
            }
            return;
        }

        for (int i = 0; i < toggleGroups.Length; i++)
        {
            if (!toggleGroups[i].AnyTogglesOn())
            {
                if (!activeCoroutine)
                {
                    StartCoroutine(HighlightQuestion(questions[i]));
                }
                return;
            }
        }

        SubmitQuestionnaireData();
        StartCoroutine(LoadTutorialScene());
    }

    private void SubmitQuestionnaireData()
    {
        int[] questionnaireSelected = new int[7];
        int index = 1;
        questionnaireSelected[0] = int.Parse(ageField.text);
        foreach (ToggleGroup tg in toggleGroups)
        {
            for (int i = 0; i < tg.transform.childCount; i++)
            {
                if (tg.transform.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    questionnaireSelected[index] = i + 1;
                    index++;
                    break;
                }
            }
        }

        dataHelper.SubmitQuestionnaire(questionnaireSelected);
    }

    private IEnumerator HighlightQuestion(TextMeshProUGUI text)
    {
        activeCoroutine = true;
        Color color = new Color(text.color.r, text.color.g, text.color.b);

        while (text.color.g > 0f)
        {
            color.g -= 15f * Time.deltaTime;
            color.b -= 15f * Time.deltaTime;
            text.color = color;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        while (text.color.g < 1f)
        {
            color.g += 5f * Time.deltaTime;
            color.b += 5f * Time.deltaTime;
            text.color = color;
            yield return null;
        }
        activeCoroutine = false;
    }

    private IEnumerator FadeInQuestionnaire()
    {
        Color faderColor = new Color(0f, 0f, 0f, 1f);
        fader.color = faderColor;
        while (fader.color.a > 0)
        {
            faderColor.a -= 1f * Time.deltaTime;
            fader.color = faderColor;
            yield return null;
        }

        fader.gameObject.SetActive(false);
    }

    private IEnumerator LoadTutorialScene()
    {
        submitButton.interactable = false;
        ageField.interactable = false;
        foreach (ToggleGroup tg in toggleGroups)
        {
            for (int i = 0; i < tg.transform.childCount; i++)
            {
                tg.transform.GetChild(i).GetComponent<Toggle>().interactable = false;
            }
        }

        fader.gameObject.SetActive(true);
        Color faderColor = new Color(0f, 0f, 0f, 0f);
        fader.color = faderColor;
        while (fader.color.a < 1)
        {
            faderColor.a += 2f * Time.deltaTime;
            fader.color = faderColor;
            yield return null;
        }

        SceneManager.LoadScene(SceneConstants.TUTORIAL_SCENE_HASH);
    }
}

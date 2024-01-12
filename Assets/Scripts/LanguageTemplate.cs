using UnityEngine;
using TMPro;

public class LanguageTemplate : MonoBehaviour
{
    public int index;

    [SerializeField] private TextMeshProUGUI languageText;

    public void LanguageButtonPressed()
    {
        NavigationManager.Instance.LanguageButtonPressed(index);
    }
    public void SetupLanguageTemplate(string topicName)
    {
        languageText.text = topicName;
    }
}

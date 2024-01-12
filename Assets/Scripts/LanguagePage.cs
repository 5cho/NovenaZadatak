using UnityEngine;

public class LanguagePage : MonoBehaviour
{
    [SerializeField] private GameObject languagePageTemplate;
    [SerializeField] private GameObject languagePageTemplateParent;
    private void OnEnable()
    {
        bool isFirst = true;
        foreach(Transform child in languagePageTemplateParent.transform)
        {
            if (isFirst)
            {
                isFirst = false;
                continue;
            }
            Destroy(child.gameObject);
        }
        TranslatedContentContainer translatedContentContainer = ContentManager.Instance.GetParsedContent();
        int index = 0;
        foreach (TranslatedContent content in translatedContentContainer.TranslatedContents)
        {
            CreateLanguageEntry(content, index);
            index++;
        }
    }
    private void CreateLanguageEntry(TranslatedContent content, int index)
    {
        GameObject newEntry = Instantiate(languagePageTemplate, languagePageTemplateParent.transform);
        newEntry.GetComponentInChildren<LanguageTemplate>().SetupLanguageTemplate(content.LanguageName);
        newEntry.GetComponentInChildren<LanguageTemplate>().index = index;
        newEntry.SetActive(true);
    }  
}

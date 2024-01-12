using System.Collections.Generic;
using UnityEngine;

public class TopicListPage : MonoBehaviour
{
    [SerializeField] private GameObject topicListTemplate;
    [SerializeField] private GameObject topicListTemplateParent;
    private void OnEnable()
    {
        bool isFirst = true;
        foreach(Transform child in topicListTemplateParent.transform)
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
        List<Topic> topicList = translatedContentContainer.TranslatedContents[NavigationManager.Instance.SelectedLanguageIndex].Topics;

        foreach(Topic topic in topicList)
        {
            string topicName = topic.Name;
            CreateNewTopicListEntry(index, topicName);
            index++;
        }
    }
    private void CreateNewTopicListEntry(int index, string topicName)
    {
        GameObject newEntry = Instantiate(topicListTemplate, topicListTemplateParent.transform);

        newEntry.GetComponent<TopicListTemplate>().SetupTopicListTemplate(index + 1, topicName);
        newEntry.GetComponent<TopicListTemplate>().index = index;
        newEntry.SetActive(true);
    }
}

using UnityEngine;
using TMPro;
public class TopicListTemplate : MonoBehaviour
{
    public int index;

    [SerializeField] private TextMeshProUGUI screenIndexText;
    [SerializeField] private TextMeshProUGUI topicNameText;

    public void TopicListButtonPressed()
    {
        NavigationManager.Instance.TopicListButtonPressed(index);
    }
    public void SetupTopicListTemplate(int index, string topicName)
    {
        screenIndexText.text = index.ToString();
        topicNameText.text = topicName;
    }
}

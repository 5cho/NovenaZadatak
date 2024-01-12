using UnityEngine;
using System;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [SerializeField] private GameObject languagePage;
    [SerializeField] private GameObject topicListPage;
    [SerializeField] private GameObject detailsPage;


    [SerializeField] private Button topicListPageBackButton;
    [SerializeField] private Button detailsPageBackButton;


    public int SelectedLanguageIndex;
    public int SelectedTopicIndex;

    private void Awake()
    {
        Instance = this;

        topicListPageBackButton.onClick.AddListener(() => {
            topicListPage.SetActive(false);
            languagePage.SetActive(true);
        });
        detailsPageBackButton.onClick.AddListener(() => {
            detailsPage.SetActive(false);
            topicListPage.SetActive(true);
        });
    }
    private void Start()
    {
        ContentManager.Instance.OnContentParsed += ContentManager_OnContentParsed;
    }

    private void ContentManager_OnContentParsed(object sender, EventArgs e)
    {
        OpenLanguagePage();
    }
    private void OpenLanguagePage()
    {
        languagePage.SetActive(true);
    }
    private void OpenTopicListPage()
    {
        topicListPage.SetActive(true);
    }
    private void OpenDetailsPage()
    {
        detailsPage.SetActive(true);
    }
    public void LanguageButtonPressed(int index)
    {
        SelectedLanguageIndex = index;
        languagePage.SetActive(false);
        OpenTopicListPage();
    }
    public void TopicListButtonPressed(int index)
    {
        SelectedTopicIndex = index;
        topicListPage.SetActive(false);
        OpenDetailsPage();
    }
}

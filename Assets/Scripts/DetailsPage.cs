using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DetailsPage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indexText;
    [SerializeField] private TextMeshProUGUI headlineText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Image image;
    [SerializeField] private GameObject playButtonIcon;
    [SerializeField] private GameObject pauseButtonIcon;
    [SerializeField] private Button playPauseButton;

    [SerializeField] private Slider audioSlider;
    [SerializeField] private TextMeshProUGUI audioSliderText;

    private bool isMusicPlaying = false;

    private bool isPageSetup = false;
    private int currentImageIndex = 0;
    private float imageSwapTimer = 0f;
    private float imageSwapTimerMax = 3f;

    private List<string> imageLocationList = new List<string>();
    private List<Sprite> spriteList = new List<Sprite>();
    private string audioClipLocation;

    private bool isDraggingSlider = false;

    private void OnEnable()
    {
        SetupDetailsPage();
        audioSource.Pause();
    }
    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            isMusicPlaying = false;
            audioSource.Pause();
            playButtonIcon.SetActive(true);
            pauseButtonIcon.SetActive(false);
            audioSource.clip = null;
            isPageSetup = false;
        }
    }
    private void Start()
    {
        playPauseButton.onClick.AddListener(() => {
            if (isMusicPlaying)
            {
                isMusicPlaying = false;
                audioSource.Pause();
                playButtonIcon.SetActive(true);
                pauseButtonIcon.SetActive(false);
            }
            else
            {
                isMusicPlaying = true;
                audioSource.Play();
                playButtonIcon.SetActive(false);
                pauseButtonIcon.SetActive(true);
            }
        });

        audioSlider.onValueChanged.AddListener((float newValue) =>
        {
            if (!isDraggingSlider)
            {
                audioSliderText.text = FormatTime(newValue * audioSource.clip.length) + " / " + FormatTime(audioSource.clip.length);
            }
        });
        EventTrigger eventTrigger = audioSlider.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { isDraggingSlider = true; });
        eventTrigger.triggers.Add(pointerDownEntry);
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnAudioSliderReleased(); });
        eventTrigger.triggers.Add(pointerUpEntry);

        audioSliderText.text = "0:00 / " + FormatTime(audioSource.clip.length);
        audioSlider.value = 0f;
    }
    private void Update()
    {
        if (isPageSetup)
        {
            HandleGallery();
        }
        if (!isDraggingSlider && audioSource != null && audioSource.clip != null)
        {
            float currentTime = audioSource.time;
            float normalizedTime = currentTime / audioSource.clip.length;

            audioSliderText.text = FormatTime(currentTime) + " / " + FormatTime(audioSource.clip.length);
            audioSlider.value = normalizedTime;
        }
    }
    private void OnAudioSliderReleased()
    {
        if (isDraggingSlider)
        {
            float newTime = audioSlider.value * audioSource.clip.length;

            audioSource.time = newTime;

            audioSliderText.text = FormatTime(newTime) + " / " + FormatTime(audioSource.clip.length);

            isDraggingSlider = false;
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private void HandleGallery()
    {
        imageSwapTimer += Time.deltaTime;

        if (imageSwapTimer >= imageSwapTimerMax && imageLocationList.Count > 0)
        {
            imageSwapTimer = 0f;

            currentImageIndex = (currentImageIndex + 1) % imageLocationList.Count;

            image.sprite = spriteList[currentImageIndex];
        }
    }
    private void SetupDetailsPage()
    {
        imageLocationList.Clear();
        spriteList.Clear();
        indexText.text = (NavigationManager.Instance.SelectedTopicIndex + 1).ToString();
        headlineText.text = ContentManager.Instance.GetParsedContent().TranslatedContents[NavigationManager.Instance.SelectedLanguageIndex].Topics[NavigationManager.Instance.SelectedTopicIndex].Name;

        TranslatedContentContainer parsedContent = ContentManager.Instance.GetParsedContent();

        if (parsedContent != null &&
    NavigationManager.Instance.SelectedLanguageIndex < parsedContent.TranslatedContents.Count)
        {
            TranslatedContent selectedLanguage = parsedContent.TranslatedContents[NavigationManager.Instance.SelectedLanguageIndex];

            if (NavigationManager.Instance.SelectedTopicIndex < selectedLanguage.Topics.Count)
            {
                Topic selectedTopic = selectedLanguage.Topics[NavigationManager.Instance.SelectedTopicIndex];

                foreach (Media media in selectedTopic.Media)
                {
                    if (media.Name == "Audio")
                    {
                        audioClipLocation = media.FilePath;
                    }
                    if (media.Name == "Gallery")
                    {
                        foreach (Photo photo in media.Photos)
                        {
                            imageLocationList.Add(photo.Path);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Invalid SelectedTopicIndex");
            }
        }
        else
        {
            Debug.LogError("Invalid SelectedLanguageIndex or parsedContent is null");
        }

        foreach (string imagePath in imageLocationList)
        {
            string newimagePath = imagePath.TrimStart('/');
            Sprite sprite = ContentManager.Instance.GetImageFileByPath(newimagePath);

            if (sprite != null)
            {
                spriteList.Add(sprite);
            }
            else
            {
                Debug.LogError($"Sprite not found for imagePath: {imagePath}");
            }

            audioSource.clip = ContentManager.Instance.GetAudioFileByPath(audioClipLocation);
        }
        currentImageIndex = 0;
        image.sprite = spriteList[currentImageIndex];
        isPageSetup = true;
    }
}

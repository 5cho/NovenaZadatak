using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ContentManager : MonoBehaviour
{
    public static ContentManager Instance { get; private set; }

    private const string GITHUB_URL_FILES = "https://raw.githubusercontent.com/5cho/NovenaJSONFile/main/FileList.json";

    private List<DownloadFileInfo> filesToDownload;
    private List<ImageFile> imageFileList = new List<ImageFile>();
    private List<AudioFile> audioFileList = new List<AudioFile>();

    private TranslatedContentContainer parsedContent;

    public event EventHandler OnContentParsed;

    private List<string> downloadedImageLocations = new List<string>();

    [System.Serializable]
    private class DownloadFileInfo
    {
        public string url;
        public string localPath;
    }
    [Serializable]
    public class ImageFile
    {
        public string path;
        public Sprite sprite;
    }
    [Serializable]
    public class AudioFile
    {
        public string path;
        public AudioClip clip;
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(DownloadFileList(GITHUB_URL_FILES));
    }
    private IEnumerator DownloadFileList(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading file list: " + webRequest.error);
            }
            else
            {
                string jsonFileList = webRequest.downloadHandler.text;
                filesToDownload = JsonUtility.FromJson<DownloadFileInfoList>(jsonFileList).files;

                StartCoroutine(DownloadFiles());
            }
        }
    }
    private IEnumerator DownloadFiles()
    {
        downloadedImageLocations.Clear();

        DownloadFileInfo jsonFile = filesToDownload[0];
        yield return StartCoroutine(DownloadFile(jsonFile.url, jsonFile.localPath));

        string filesFolderPath = Path.Combine(Application.persistentDataPath, "files");
        Directory.CreateDirectory(filesFolderPath);

        for (int i = 1; i < filesToDownload.Count; i++)
        {
            DownloadFileInfo fileInfo = filesToDownload[i];
            string localPathInFilesFolder = Path.Combine("files", fileInfo.localPath);
            yield return StartCoroutine(DownloadFile(fileInfo.url, localPathInFilesFolder));

            if (IsAudioFile(fileInfo.localPath))
            {
                LoadAudioClip(localPathInFilesFolder);
            }
            else if (IsImageFile(fileInfo.localPath))
            {
                downloadedImageLocations.Add(localPathInFilesFolder);
            }
        }
        foreach (string imageLocation in downloadedImageLocations)
        {
            LoadImages(imageLocation);
        }
        CreateContentData(Path.Combine(Application.persistentDataPath, filesToDownload[0].localPath));
    }
    private bool IsImageFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
    }
    private bool IsAudioFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return extension == ".mp3" || extension == ".wav" || extension == ".ogg";
    }
    private void LoadAudioClip(string audioLocation)
    {
        AudioClip audioClip = LoadAudioClipFromFile(Path.Combine(Application.persistentDataPath, audioLocation));

        AudioFile newAudioFile = new AudioFile();
        newAudioFile.path = audioLocation;
        newAudioFile.clip = audioClip;

        

        foreach (AudioFile audioFile in audioFileList)
        {
            if (audioFile.path == newAudioFile.path)
            {
                return;
            }
        }
        audioFileList.Add(newAudioFile);
    }

    private AudioClip LoadAudioClipFromFile(string path)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);

            int sampleCount = fileData.Length / 2;
            float[] floatData = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(fileData, i * 2);
                floatData[i] = sample / 32768.0f;
            }
            GameObject audioSourceObject = new GameObject("TempAudioSource");
            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();

            audioSource.clip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), sampleCount, 1, 44100, false);
            audioSource.clip.SetData(floatData, 0);

            AudioClip newAudioClip = audioSource.clip;

            Destroy(audioSourceObject);

            return newAudioClip;
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception while loading audio clip from path {path}: {e.Message}");
            return null;
        }
    }
    public AudioClip GetAudioFileByPath(string audioPath)
    {
        foreach (AudioFile audioFile in audioFileList)
        {
            string normalizedPath = audioFile.path
                .Replace('/', '\\')
                .TrimStart('\\');

            string normalizedAudioPath = audioPath
                .Replace('/', '\\')
                .TrimStart('\\');

            if (normalizedPath.Equals(normalizedAudioPath, StringComparison.OrdinalIgnoreCase))
            {
                return audioFile.clip;
            }
        }

        Debug.LogWarning($"Audio clip not found for path: {audioPath}");
        return null;
    }


    private IEnumerator DownloadFile(string url, string localPath)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading file {url}: {webRequest.error}");
            }
            else
            {
                byte[] fileData = webRequest.downloadHandler.data;
                string localFilePath = Path.Combine(Application.persistentDataPath, localPath);
                File.WriteAllBytes(localFilePath, fileData);
            }
        }
    }
    private void CreateContentData(string localFilePath)
    {
        try
        {
            string jsonData = File.ReadAllText(localFilePath);
            parsedContent = JsonUtility.FromJson<TranslatedContentContainer>(jsonData);
            OnContentParsed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception during deserialization: {e.Message}");
            Debug.LogError($"StackTrace: {e.StackTrace}");
        }
    }

    public TranslatedContentContainer GetParsedContent()
    {
        return parsedContent;
    }

    [System.Serializable]
    private class DownloadFileInfoList
    {
        public List<DownloadFileInfo> files;
    }

    private void LoadImages(string imageLocation)
    {
        Texture2D texture = LoadTextureFromFile(Path.Combine(Application.persistentDataPath, imageLocation));

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        ImageFile newImageFile = new ImageFile();
        newImageFile.path = imageLocation;
        newImageFile.sprite = sprite;

        foreach(ImageFile imageFile in imageFileList)
        {
            if(imageFile.path == newImageFile.path)
            {
                return;
            }
        }
        imageFileList.Add(newImageFile);
    }
    private Texture2D LoadTextureFromFile(string path)
    {
        try
        {
            byte[] fileData = System.IO.File.ReadAllBytes(Path.Combine(path));

            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(fileData))
            {

                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                texture.Apply();

                return texture;
            }
            else
            {
                Debug.LogError($"Failed to load image data into texture from path: {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception while loading texture from path {path}: {e.Message}");
            return null;
        }
    }
    public Sprite GetImageFileByPath(string imagePath)
    {
        foreach (ImageFile imageFile in imageFileList)
        {
            if (Path.Combine("files", imageFile.path).Replace('\\', '/').Equals(Path.Combine("files", imagePath).Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
            {
                return imageFile.sprite;
            }
        }
        return null;
    }


}

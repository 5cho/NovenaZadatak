using System.Collections.Generic;

[System.Serializable]
public class TranslatedContentContainer
{
    public List<TranslatedContent> TranslatedContents;
}

[System.Serializable]
public class TranslatedContent
{
    public int LanguageId;
    public string LanguageName;
    public List<Topic> Topics;
}

[System.Serializable]
public class Topic
{
    public string Name;
    public List<Media> Media;
}

[System.Serializable]
public class Media
{
    public string Name;
    public string FilePath;
    public List<Photo> Photos;
}

[System.Serializable]
public class Photo
{
    public string Path;
    public string Name;
}
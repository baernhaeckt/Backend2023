namespace Backend2023.Cognitive;

/// <summary>
///     Base config for Speech2Text and Text2Speech.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=stt">List of Languages and Voices</see>
public abstract record SpeechRequest(string Language, string Voice);

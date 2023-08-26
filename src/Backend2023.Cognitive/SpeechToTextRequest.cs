namespace Backend2023.Cognitive;

public record SpeechToTextRequest(string FileName, string Language = "de-CH", string Voice = "de-CH-LeniNeural")
    : SpeechRequest(Language, Voice);
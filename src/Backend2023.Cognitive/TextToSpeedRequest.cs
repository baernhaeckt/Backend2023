namespace Backend2023.Cognitive;

public record TextToSpeedRequest(string Text, string Language = "de-CH", string Voice = "de-CH-LeniNeural", bool IsSpeechSynthesisMarkupLanguage = false)
    : SpeechRequest(Language, Voice);
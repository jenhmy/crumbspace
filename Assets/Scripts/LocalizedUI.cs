using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string localizationKey;

    [Header("Image")]
    [SerializeField] private Sprite spanishSprite;
    [SerializeField] private Sprite englishSprite;

    private TMP_Text textComponent;
    private Image imageComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        imageComponent = GetComponent<Image>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
        }

        if (textComponent != null)
        {
            textComponent.text = LocalizationManager.Get(localizationKey);
        }

        if (imageComponent != null)
        {
            imageComponent.sprite = GameSettings.CurrentLanguage ==
                GameLanguage.Spanish
                ? spanishSprite
                : englishSprite;
        }
    }
}

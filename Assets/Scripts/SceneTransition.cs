using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField, Min(0f)] private float fadeDuration = 0.4f;

    private bool transitionPending;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transitionPanel == null || transitionCanvasGroup == null)
        {
            Debug.LogError(
                "SceneTransition requires TransitionPanel and its CanvasGroup.",
                this);
            return;
        }

        transitionPanel.SetActive(false);
        SetPanelState(0f, false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (transitionCanvasGroup == null)
        {
            return;
        }

        StopAllCoroutines();

        if (!transitionPending)
        {
            transitionPanel.SetActive(false);
            SetPanelState(0f, false);
            return;
        }

        transitionPending = false;
        transitionPanel.SetActive(true);
        SetPanelState(1f, true);
        StartCoroutine(Fade(1f, 0f, null));
    }

    public void FadeOutAndRun(Action onComplete)
    {
        if (transitionCanvasGroup == null)
        {
            Debug.LogError("SceneTransition has no CanvasGroup assigned.", this);
            return;
        }

        StopAllCoroutines();
        transitionPending = true;
        transitionPanel.SetActive(true);
        SetPanelState(transitionCanvasGroup.alpha, true);
        StartCoroutine(Fade(transitionCanvasGroup.alpha, 1f, onComplete));
    }

    public void FadeOutAndBack(Action onComplete)
    {
        if (transitionCanvasGroup == null)
        {
            Debug.LogError("SceneTransition has no CanvasGroup assigned.", this);
            return;
        }

        StopAllCoroutines();
        transitionPending = false;
        transitionPanel.SetActive(true);
        SetPanelState(transitionCanvasGroup.alpha, true);
        StartCoroutine(FadeOutAndBackCoroutine(onComplete));
    }

    private IEnumerator FadeOutAndBackCoroutine(Action onComplete)
    {
        yield return Fade(transitionCanvasGroup.alpha, 1f, null);
        onComplete?.Invoke();
        yield return Fade(1f, 0f, null);
    }

    private IEnumerator Fade(float from, float to, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            transitionCanvasGroup.alpha = Mathf.Lerp(from, to, progress);
            yield return null;
        }

        SetPanelState(to, to > 0f);
        onComplete?.Invoke();
    }

    private void SetPanelState(float alpha, bool blocksRaycasts)
    {
        transitionCanvasGroup.alpha = alpha;
        transitionCanvasGroup.blocksRaycasts = blocksRaycasts;
        transitionCanvasGroup.interactable = blocksRaycasts;

        if (alpha <= 0f)
        {
            transitionPanel.SetActive(false);
        }
    }
}

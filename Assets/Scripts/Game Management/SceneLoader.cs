using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    [SerializeField] private int mainMenuSceneBuildIndex = 0;
    [SerializeField] private int gameSceneBuildIndex = 1;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start() => StartCoroutine(Fade(1f, 0f));

    public IEnumerator LoadScene(int sceneId)
    {
        var op = SceneManager.LoadSceneAsync(sceneId);
        yield return op;
    }
    
    public void LoadMainMenuWithFade()
    {
        StartCoroutine(DoSceneTransition(mainMenuSceneBuildIndex));
    }
    
    public void LoadGameWithFade()
    {
        StartCoroutine(DoSceneTransition(gameSceneBuildIndex));
    }
    
    public void LoadSceneWithFade(int sceneId)
    {
        StartCoroutine(DoSceneTransition(sceneId));
    }
    
    private IEnumerator DoSceneTransition(int sceneId)
    {
        yield return Fade(0f, 1f);
        var op = SceneManager.LoadSceneAsync(sceneId);
        yield return op;
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (!fadeCanvasGroup.gameObject.activeSelf)
            fadeCanvasGroup.gameObject.SetActive(true);
        
        float time = 0f;
        while (time < fadeDuration)
        {
            Debug.Log("fading time: " + time);
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }
}

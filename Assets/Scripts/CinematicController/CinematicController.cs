using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CinematicSlide
{
    public Sprite image;
    [TextArea(2, 5)]
    public string subtitle;
    public float duration = 3f;
    public TransitionType transition = TransitionType.Fade;
    public float transitionDuration = 0.5f;
}

public enum TransitionType
{
    None,
    Fade,
    Slide,
    Zoom
}

public class CinematicController : MonoBehaviour
{
    [Header("UI References")]
    public Image displayImage;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup canvasGroup;
    
    [Header("Cinematic Settings")]
    public CinematicSlide[] slides;
    public bool autoStart = true;
    public bool loopCinematic = false;
    
    [Header("Game Flow")]
    public string defaultNextSceneName = "Level1"; // Nivel a cargar si no hay uno guardado
    [SerializeField] private string nextSceneName;

    [Header("Transition Settings")]
    public float defaultTransitionDuration = 0.5f;
    
    private int currentSlide = 0;
    private bool isPlaying = false;
    private RectTransform imageRectTransform;

    void Start()
    {
        nextSceneName = "Lv.1";
        
        if (displayImage == null)
        {
            Debug.LogError("Display Image not assigned to Cinematic Controller!");
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = displayImage.GetComponentInParent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup not found! Adding one to the display image's parent.");
                canvasGroup = displayImage.transform.parent.gameObject.AddComponent<CanvasGroup>();
            }
        }

        imageRectTransform = displayImage.GetComponent<RectTransform>();
        
        // Initialize subtitle text if assigned
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }

        if (autoStart && slides.Length > 0)
        {
            StartCinematic();
        }
    }
    
    void Update()
    {
        // Permitir saltar toda la cinemática con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadNextScene();
        }
        
        // Permitir avanzar a la siguiente diapositiva con Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipToNextSlide();
        }
    }

    public void StartCinematic()
    {
        if (!isPlaying && slides.Length > 0)
        {
            isPlaying = true;
            currentSlide = 0;
            StartCoroutine(PlayCinematic());
        }
    }

    public void StopCinematic()
    {
        isPlaying = false;
        StopAllCoroutines();
    }

    IEnumerator PlayCinematic()
    {
        while (currentSlide < slides.Length)
        {
            CinematicSlide slide = slides[currentSlide];
            
            // Apply the transition effect
            yield return StartCoroutine(TransitionToSlide(slide));
            
            // Show subtitle if there is text and a text component
            if (subtitleText != null)
            {
                subtitleText.text = slide.subtitle;
            }
            
            // Wait for the duration of this slide
            yield return new WaitForSeconds(slide.duration);
            
            currentSlide++;
        }

        if (loopCinematic)
        {
            currentSlide = 0;
            StartCoroutine(PlayCinematic());
        }
        else
        {
            isPlaying = false;
            Debug.Log("Cinematic finished");
            
            // Invoke the event and load the next scene
            OnCinematicComplete();
            LoadNextScene();
        }
    }

    private IEnumerator TransitionToSlide(CinematicSlide slide)
    {
        float duration = slide.transitionDuration > 0 ? slide.transitionDuration : defaultTransitionDuration;
        
        switch (slide.transition)
        {
            case TransitionType.None:
                // Simply change the sprite without any transition
                displayImage.sprite = slide.image;
                break;
                
            case TransitionType.Fade:
                yield return StartCoroutine(FadeTransition(slide.image, duration));
                break;
                
            case TransitionType.Slide:
                yield return StartCoroutine(SlideTransition(slide.image, duration));
                break;
                
            case TransitionType.Zoom:
                yield return StartCoroutine(ZoomTransition(slide.image, duration));
                break;
        }
    }

    private IEnumerator FadeTransition(Sprite newSprite, float duration)
    {
        // Fade out
        float time = 0;
        while (time < duration / 2)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, time / (duration / 2));
            time += Time.deltaTime;
            yield return null;
        }
        
        // Change sprite
        displayImage.sprite = newSprite;
        
        // Fade in
        time = 0;
        while (time < duration / 2)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / (duration / 2));
            time += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1; // Ensure we end at full alpha
    }

    private IEnumerator SlideTransition(Sprite newSprite, float duration)
    {
        // Create a temporary image to slide in
        GameObject tempObj = new GameObject("TempSlide");
        tempObj.transform.SetParent(displayImage.transform.parent);
        
        RectTransform rectTransform = tempObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = imageRectTransform.anchorMin;
        rectTransform.anchorMax = imageRectTransform.anchorMax;
        rectTransform.pivot = imageRectTransform.pivot;
        rectTransform.sizeDelta = imageRectTransform.sizeDelta;
        
        // Position it to the right of the screen
        rectTransform.anchoredPosition = new Vector2(Screen.width, imageRectTransform.anchoredPosition.y);
        
        // Add image component
        Image tempImage = tempObj.AddComponent<Image>();
        tempImage.sprite = newSprite;
        
        // Slide both images
        float time = 0;
        Vector2 startPosOld = imageRectTransform.anchoredPosition;
        Vector2 startPosNew = rectTransform.anchoredPosition;
        Vector2 targetPosOld = new Vector2(-Screen.width, startPosOld.y);
        Vector2 targetPosNew = startPosOld;
        
        while (time < duration)
        {
            float t = time / duration;
            imageRectTransform.anchoredPosition = Vector2.Lerp(startPosOld, targetPosOld, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosNew, targetPosNew, t);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Set final position
        imageRectTransform.anchoredPosition = startPosOld;
        displayImage.sprite = newSprite;
        
        // Destroy the temporary image
        Destroy(tempObj);
    }

    private IEnumerator ZoomTransition(Sprite newSprite, float duration)
    {
        // Zoom out current image
        float time = 0;
        Vector3 startScale = imageRectTransform.localScale;
        Vector3 zoomOutScale = startScale * 1.5f;
        
        while (time < duration / 2)
        {
            float t = time / (duration / 2);
            imageRectTransform.localScale = Vector3.Lerp(startScale, zoomOutScale, t);
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Change sprite
        displayImage.sprite = newSprite;
        imageRectTransform.localScale = Vector3.one * 0.5f;
        
        // Zoom in new image
        time = 0;
        Vector3 zoomInScale = Vector3.one * 0.5f;
        
        while (time < duration / 2)
        {
            float t = time / (duration / 2);
            imageRectTransform.localScale = Vector3.Lerp(zoomInScale, startScale, t);
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Reset scale
        imageRectTransform.localScale = startScale;
        canvasGroup.alpha = 1;
    }
    
    public void SkipToNextSlide()
    {
        if (isPlaying && currentSlide < slides.Length - 1)
        {
            StopAllCoroutines();
            currentSlide++;
            StartCoroutine(PlayCinematic());
        }
        else if (isPlaying && currentSlide >= slides.Length - 1)
        {
            // Si estamos en la última diapositiva, avanzar al juego
            LoadNextScene();
        }
    }
    
    public void SkipToSlide(int slideIndex)
    {
        if (isPlaying && slideIndex >= 0 && slideIndex < slides.Length)
        {
            StopAllCoroutines();
            currentSlide = slideIndex;
            StartCoroutine(PlayCinematic());
        }
    }
    
    // Cargar la siguiente escena (nivel del juego)
    public void LoadNextScene()
    {
        StopAllCoroutines();
        isPlaying = false;
        SceneManager.LoadScene(nextSceneName);
    }
    
    // Event that you can subscribe to when the cinematic completes
    public event Action OnCinematicCompleted;
    
    private void OnCinematicComplete()
    {
        if (OnCinematicCompleted != null)
        {
            OnCinematicCompleted.Invoke();
        }
    }
}
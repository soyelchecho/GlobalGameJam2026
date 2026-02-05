using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple loading screen that displays an image during scene transitions.
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private Image loadingImage;

    public void SetLoadingImage(Sprite sprite)
    {
        if (loadingImage != null)
        {
            loadingImage.sprite = sprite;
        }
    }

    public void SetProgress(float progress)
    {
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide(Action onComplete = null)
    {
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}

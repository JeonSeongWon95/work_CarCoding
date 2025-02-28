using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private float startAlpha;
    [SerializeField] private float endAlpha;
    [SerializeField] private float fadeSpeed = 1.0f;
    [SerializeField] private bool isLoop = false;
    [SerializeField] private bool isFade = false;

    private Image image;
    private Color currentColor;

    private void Start()
    {
        image = gameObject.GetComponent<Image>();
        currentColor = image.color;
    }

    private void Update()
    {
        if (!isFade) return;
        
        image = image ? image : gameObject.GetComponent<Image>();

        float newAlpha;
        if (endAlpha > startAlpha)
        {
            newAlpha = Mathf.Clamp(image.color.a + Time.deltaTime * fadeSpeed, startAlpha, endAlpha);
        }
        else
        {
            newAlpha = Mathf.Clamp(image.color.a - Time.deltaTime * fadeSpeed, endAlpha, startAlpha);
        }

        currentColor.a = newAlpha;
        image.color = currentColor;
    
        if (newAlpha.Equals(endAlpha))
        {
            if (!isLoop)
            {
                isFade = false;
            }
            else
            {
                SetFade(endAlpha, startAlpha);
                BeginFade();
            }
            
        }
    }

    public void BeginFade()
    {
        image = image ? image : gameObject.GetComponent<Image>();

        currentColor = image.color;
        currentColor.a = startAlpha;
        image.color = currentColor;
        isFade = true;
    }

    public void SetFade(float originAlpha, float departureAlpha)
    {
        startAlpha = originAlpha;
        endAlpha = departureAlpha;
    }

}

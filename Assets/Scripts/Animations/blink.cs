using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class blink : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Image targetImage;
    private Color ImageColor;
    private Coroutine PlayingCoroutine;
    private bool IsIncrease = true;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        ImageColor = targetImage.color;
    }

    private void OnEnable()
    {
        PlayingCoroutine =  StartCoroutine(BlinkImage());
    }

    private void OnDisable()
    {
        if (PlayingCoroutine != null)
            StopCoroutine(PlayingCoroutine);

        ResetData();

    }

    IEnumerator BlinkImage() 
    {
        while (true) 
        {
            if (IsIncrease)
            {
                ImageColor.a = Mathf.Clamp(ImageColor.a + (Time.deltaTime * speed), 0.0f, 1.0f);
                targetImage.color = ImageColor;

                if (ImageColor.a >= 1.0f)
                {
                    ImageColor.a = 1.0f;
                    targetImage.color = ImageColor;
                    IsIncrease = false;
                }
            }
            else 
            {
                ImageColor.a = Mathf.Clamp(ImageColor.a - (Time.deltaTime * speed), 0.0f, 1.0f);
                targetImage.color = ImageColor;

                if (ImageColor.a <= 0.0f)
                {
                    ImageColor.a = 0.0f;
                    targetImage.color = ImageColor;
                    IsIncrease = true;
                }
            }

            yield return null;
        }
    }

    void ResetData() 
    {
        ImageColor.a = 1.0f;
        targetImage.color = ImageColor;
    }
}

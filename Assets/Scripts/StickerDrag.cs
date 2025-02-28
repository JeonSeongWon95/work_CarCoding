using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StickerDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private string stickerValue;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private StickerDrag childSticker;
    private CarCodingFlowController flowController;

    private bool isOrigin = true;

    private float initialDistance;
    private Vector3 initialScale;
    
    private void Awake()
    {
        // RectTransform과 Canvas 설정
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>(); // 부모 캔버스를 참조
        flowController = FindFirstObjectByType<CarCodingFlowController>();
        
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isOrigin)
        {
            var instanceChild = Instantiate(gameObject, canvas.transform, true);
            instanceChild.transform.SetParent(canvas.transform);
            childSticker = instanceChild.GetComponent<StickerDrag>();
            childSticker.SetInstance();
        }
        else
        {
            flowController.SetStickerUpper(gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // 마우스 드래그로 스티커 UI 위치 이동 (캔버스의 스케일에 따라 조정)
        if (isOrigin)
        {
            childSticker.GetRectTransform().anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isOrigin)
        {
            flowController.StickerAdjustPos(childSticker.gameObject);
        }
        else
        {
            flowController.StickerAdjustPos(gameObject);
        }
        
    }

    private RectTransform GetRectTransform()
    {
        rectTransform = rectTransform == null ? GetComponent<RectTransform>() : rectTransform;
        return rectTransform;
    }

    private void SetInstance()
    {
        isOrigin = false;
        flowController = flowController == null ? FindFirstObjectByType<CarCodingFlowController>() : flowController;
    }

    public string GetDirection()
    {
        return stickerValue;
    }
    
}

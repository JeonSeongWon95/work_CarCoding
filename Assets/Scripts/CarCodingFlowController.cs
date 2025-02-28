using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CarCodingFlowController : MonoBehaviour
{
    public static CarCodingFlowController instance;

    [Header("HUD")]
    [SerializeField] private Canvas MainMenu;
    [SerializeField] private Canvas InfoMenu;
    [SerializeField] private Canvas CodingMenu;
    [SerializeField] private Canvas Coding2Menu;
    [SerializeField] private Canvas MapMenu;

    [Space]

    [Header("Coding")]
    [SerializeField] private GameObject CodingStacksGameObject;
    [SerializeField] private List<GameObject> CodingStacksList;
    [SerializeField] private GameObject CodingBlockTouchImage;
    [SerializeField] private GameObject CodingRestartObject;

    [Header("Coding2")]
    [SerializeField] private List<GameObject> Coding2OrderObjectList;
    [SerializeField] private List<GameObject> Coding2StacksObjectList;
    [SerializeField] private List<GameObject> Coding2StacksList;
    [SerializeField] private RawImage Coding2BGImage;
    [SerializeField] private List<Texture2D> Coding2BGTextures;
    [SerializeField] private GameObject Coding2BlockTouchImage;
    [SerializeField] private GameObject Coding2RestartObject;

    [Header("Map")]
    [SerializeField] private RawImage MapBGImage;
    [SerializeField] private GameObject MapLightObject;
    [SerializeField] private Texture2D MapBGDefaultTexture;
    [SerializeField] private Texture2D MapBGCodingTexture;
    [SerializeField] private Texture2D MapBGEndTexture;

    [SerializeField] private GameObject MapBGSuccessObject;
    [SerializeField] private RawImage MapBGSuccessImage;
    [SerializeField] private List<Texture2D> MapBGSuccessTexutres;
    [SerializeField] private GameObject MapBGFailObject;
    [SerializeField] private RawImage MapBGFailImage;
    [SerializeField] private Texture2D MapBGFailDefaultTexuture;
    [SerializeField] private List<Texture2D> MapBGFailTexutres;
    [SerializeField] private GameObject MapTimerBGImage;
    [SerializeField] private Image MapTimerImage;

    [SerializeField] private GameObject MapCarObject;
    [SerializeField] private GameObject MapCarImpactObject;

    [SerializeField] private GameObject mapAmbulanceGameObject;
    [SerializeField] private GameObject mapPersonGameObject;

    [Header("Values")]
    [SerializeField] private float CodingDelayTime = 2.0f;
    [SerializeField] private float CarMovingDistance = 159.0f;
    [SerializeField] private float RestartTimeLimit = 5.0f;

    private Canvas currentCanvas;

    private List<Vector2> CodingStacksPos;
    private Dictionary<int, List<Vector2>> Coding2StacksPos;

    private const int Coding2StackLen = 2;
    private int currentCoding2Index = 0;

    private Vector2 MapCarDefaultPos;
    private RectTransform MapCarRectTransform;

    private int[,] MapTile;
    private Vector2Int StartIndex;
    private Vector2Int currentIndex;

    private List<string> orderList;
    private int currentOrderIndex = 0;

    // Middle Point, End Point, Blinker Point, Ambulance Point, Person Point
    private const int MD = 9;
    private const int ED = 10;
    private const int BK = 3;
    private const int AB = 4;
    private const int PS = 5;

    private float restartTimer = 0.0f;
    private bool bRestartTimerEnable = false;
    private bool isPassMiddle = false;
    private bool isClear = false;

    private Image carImage;
    private Image carImpactImage;
    private Color darkerColor;
    private Color carOriginalColor;
    private Color impactOriginalColor;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private enum HUDState
    {
        Main,
        Info,
        Coding,
        Coding2
    }

    void Start()
    {
#if UNITY_EDITOR
        Cursor.visible = true;
#else
        Cursor.visible = false;
#endif

        darkerColor = new Color(0.3f, 0.3f, 0.3f);
        carImage = MapCarObject.GetComponent<Image>();
        carImpactImage = MapCarImpactObject.GetComponent<Image>();
        carOriginalColor = carImage.color;
        impactOriginalColor = carImpactImage.color;

        currentCanvas = MainMenu;

        ChangeStatus(HUDState.Main);
        MapMenu.gameObject.SetActive(true);

        CodingStacksPos = new List<Vector2>();

        foreach (var codingStack in CodingStacksList)
        {
            CodingStacksPos.Add(codingStack.GetComponent<RectTransform>().anchoredPosition);
        }

        /*
         * Coding2StacksList is store 3 groups of stacks.
         * each group contains 2 stacks.
         * 0, 1 is the first stack. 2, 3 is the second stack. 4, 5 is the third stack
         */
        Coding2StacksPos = new Dictionary<int, List<Vector2>>();

        for (int i = 0; i < Coding2StacksList.Count; i++)
        {
            if (!Coding2StacksPos.ContainsKey(i / 2))
            {
                Coding2StacksPos.Add(i / 2, new List<Vector2>());
            }

            Coding2StacksPos[i / 2].Add(Coding2StacksList[i].GetComponent<RectTransform>().anchoredPosition);

        }

        orderList = new List<string>();

        MapCarDefaultPos = MapCarObject.GetComponent<RectTransform>().anchoredPosition;
        MapCarRectTransform = MapCarObject.GetComponent<RectTransform>();

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        SetMapTile();
    }

    private void Update()
    {
        if (!bRestartTimerEnable) return;

        restartTimer += Time.deltaTime;
        MapTimerImage.fillAmount = restartTimer / RestartTimeLimit;

        if (restartTimer >= RestartTimeLimit)
        {
            if (currentCoding2Index >= 2 && isClear && currentCanvas == Coding2Menu)
            {
                bRestartTimerEnable = false;
                restartTimer = 0.0f;
                carImage.color = carOriginalColor;
                carImpactImage.color = impactOriginalColor;
                ChangeStatus(HUDState.Main);
                return;
            }
            else 
            {

            }

            restartTimer = 0.0f;
            bRestartTimerEnable = false;
            carImage.color = carOriginalColor;
            carImpactImage.color = impactOriginalColor;
            CodingEnd();
        }
    }

    private void SetMapTile()
    {
        MapTile = new int[8, 13]
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
            {0, 1, MD, AB, 1, 1, 1, BK, 1, 1, 1, 1, 0},
            {0, 0, PS, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
            {0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0},
            {0, ED, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        };

        StartIndex = new Vector2Int(2, 8);
    }

    private void MainStateStart()
    {
        MapBGImage.texture = MapBGDefaultTexture;
        MapCarObject.SetActive(false);
        MapLightObject.SetActive(false);
        MapBGSuccessObject.SetActive(false);
        MapBGFailObject.SetActive(false);
        MapTimerBGImage.SetActive(false);
        mapAmbulanceGameObject.SetActive(false);
        mapPersonGameObject.SetActive(false);
    }

    private void InfoStateStart()
    {
        MapBGImage.texture = MapBGDefaultTexture;
        MapBGSuccessObject.SetActive(false);
        MapBGFailObject.SetActive(false);
        MapTimerBGImage.SetActive(false);
        mapAmbulanceGameObject.SetActive(false);
        mapPersonGameObject.SetActive(false);
    }

    private void CodingStateStart()
    {
        ResetMapSettings();
        MapCarObject.SetActive(true);
        currentOrderIndex = 0;

        foreach (var codingStack in CodingStacksList)
        {
            StickerDrag childComp = codingStack.GetComponentInChildren<StickerDrag>();

            if (childComp)
            {
                Destroy(childComp.gameObject);
            }
        }
    }

    private void ResetMapSettings()
    {

        MapBGImage.texture = MapBGCodingTexture;
        carImage.color = carOriginalColor;
        MapBGSuccessObject.SetActive(false);
        MapBGFailObject.SetActive(false);
        MapBGFailImage.texture = MapBGFailDefaultTexuture;
        MapTimerBGImage.SetActive(false);
        MapLightObject.SetActive(true);

        MapCarImpactObject.SetActive(false);
        mapAmbulanceGameObject.SetActive(false);
        mapPersonGameObject.SetActive(false);
        MapCarObject.GetComponent<RectTransform>().anchoredPosition = MapCarDefaultPos;
        MapCarObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(Vector3.zero);
        currentIndex = StartIndex;
        isPassMiddle = false;
        isClear = false;

        CodingBlockTouchImage.SetActive(false);
        CodingRestartObject.SetActive(false);
    }

    private void Coding2StateStart()
    {
        ResetMap2Settings();
        MapCarObject.SetActive(true);

        currentCoding2Index = 0;
        Coding2BGImage.texture = Coding2BGTextures[currentCoding2Index];

        foreach (var codingStack in Coding2StacksList)
        {
            StickerDrag childComp = codingStack.GetComponentInChildren<StickerDrag>();

            if (childComp)
            {
                Destroy(childComp.gameObject);
            }
        }

        currentOrderIndex = 0;

        for (int i = 0; i < Coding2OrderObjectList.Count; i++)
        {
            if (i == 0)
            {
                Coding2OrderObjectList[i].SetActive(true);
                Coding2StacksObjectList[i].SetActive(true);
            }
            else
            {
                Coding2OrderObjectList[i].SetActive(false);
                Coding2StacksObjectList[i].SetActive(false);
            }
        }

        Coding2BlockTouchImage.SetActive(true);
        StartCoroutine(CoOrderStart());
    }

    private void ResetMap2Settings()
    {
        MapBGImage.texture = MapBGCodingTexture;
        MapBGSuccessObject.SetActive(false);
        MapBGFailObject.SetActive(false);
        MapTimerBGImage.SetActive(false);
        MapLightObject.SetActive(true);

        MapCarImpactObject.SetActive(false);
        MapCarObject.GetComponent<RectTransform>().anchoredPosition = MapCarDefaultPos;
        MapCarObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(Vector3.zero);
        currentIndex = StartIndex;
        isPassMiddle = false;
        isClear = false;

        Coding2BlockTouchImage.SetActive(false);
        Coding2RestartObject.SetActive(false);
    }

    private void ChangeStatus(HUDState newState)
    {
        currentCanvas.gameObject.SetActive(false);
        MapCarObject.SetActive(false);

        switch (newState)
        {
            case HUDState.Main:
                currentCanvas = MainMenu;
                MainStateStart();
                break;
            case HUDState.Info:
                currentCanvas = InfoMenu;
                InfoStateStart();
                break;
            case HUDState.Coding:
                currentCanvas = CodingMenu;
                CodingStateStart();
                break;
            case HUDState.Coding2:
                currentCanvas = Coding2Menu;
                Coding2StateStart();
                break;
            default:
                break;
        }

        currentCanvas.gameObject.SetActive(true);
    }

    public void ChangeStatus(string newState)
    {
        foreach (var state in Enum.GetValues(typeof(HUDState)))
        {
            if (newState == state.ToString())
            {
                ChangeStatus((HUDState)state);
                return;
            }
        }
    }

    public void StickerAdjustPos(GameObject sticker)
    {
        if (currentCanvas == CodingMenu)
        {
            CodingStickerAdjustPos(sticker);
        }
        else if (currentCanvas == Coding2Menu)
        {
            Coding2StickerAdjustPos(sticker);
        }
    }

    private void CodingStickerAdjustPos(GameObject sticker)
    {
        Vector2 maxSize = CodingStacksGameObject.GetComponent<RectTransform>().sizeDelta;
        sticker.transform.SetParent(CodingStacksGameObject.transform);
        Vector2 stickerPos = sticker.GetComponent<RectTransform>().anchoredPosition;

        if (Mathf.Abs(stickerPos.x) > maxSize.x / 2 || Mathf.Abs(stickerPos.y) > maxSize.y / 2)
        {
            Destroy(sticker);
            return;
        }

        float minDistance = 123456789.0f;
        int minIndex = 0;

        for (int i = 0; i < CodingStacksPos.Count; i++)
        {
            if (Vector2.Distance(stickerPos, CodingStacksPos[i]) < minDistance)
            {
                minDistance = Vector2.Distance(stickerPos, CodingStacksPos[i]);
                minIndex = i;
            }
        }

        if (CodingStacksList[minIndex].GetComponentInChildren<StickerDrag>())
        {
            Destroy(CodingStacksList[minIndex].GetComponentInChildren<StickerDrag>().gameObject);
        }

        sticker.transform.SetParent(CodingStacksList[minIndex].transform);
        sticker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private void Coding2StickerAdjustPos(GameObject sticker)
    {
        Vector2 maxSize = Coding2StacksObjectList[currentCoding2Index].GetComponent<RectTransform>().sizeDelta;
        sticker.transform.SetParent(Coding2StacksObjectList[currentCoding2Index].transform);
        Vector2 stickerPos = sticker.GetComponent<RectTransform>().anchoredPosition;

        if (Mathf.Abs(stickerPos.x) > maxSize.x / 2 || Mathf.Abs(stickerPos.y) > maxSize.y / 2)
        {
            Destroy(sticker);
            return;
        }

        float minDistance = 123456789.0f;
        int minIndex = 0;

        for (int i = 0; i < Coding2StacksPos[currentCoding2Index].Count; i++)
        {
            if (Vector2.Distance(stickerPos, Coding2StacksPos[currentCoding2Index][i]) < minDistance)
            {
                minDistance = Vector2.Distance(stickerPos, Coding2StacksPos[currentCoding2Index][i]);
                minIndex = i;
            }
        }

        if (Coding2StacksList[minIndex + Coding2StackLen * currentCoding2Index].GetComponentInChildren<StickerDrag>())
        {
            Destroy(Coding2StacksList[minIndex + Coding2StackLen * currentCoding2Index].GetComponentInChildren<StickerDrag>().gameObject);
        }

        sticker.transform.SetParent(Coding2StacksList[minIndex + Coding2StackLen * currentCoding2Index].transform);
        sticker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void SetStickerUpper(GameObject sticker)
    {
        if (currentCanvas == CodingMenu)
        {
            sticker.transform.SetParent(CodingStacksGameObject.transform);
        }
        else if (currentCanvas == Coding2Menu)
        {
            sticker.transform.SetParent(Coding2StacksObjectList[currentCoding2Index].transform);
        }
    }

    public void CodingStart()
    {
        CodingBlockTouchImage.SetActive(true);

        orderList.Clear();

        foreach (var codingStack in CodingStacksList)
        {
            StickerDrag childComp = codingStack.GetComponentInChildren<StickerDrag>();

            if (childComp)
            {
                orderList.Add(childComp.GetDirection());
            }
        }

        StartCoroutine(CoOrderStart());
    }

    private IEnumerator CoOrderStart(float waitTime = 0.0f)
    {
        yield return new WaitForSeconds(waitTime);

        isPassMiddle = false;

        if (currentCanvas == CodingMenu)
        {
            currentOrderIndex = 0;
        }

        for (; currentOrderIndex < orderList.Count; currentOrderIndex++)
        {
            yield return new WaitForSeconds(CodingDelayTime);

            Vector2Int MovingIndex;
            Vector2 MovingAmount;
            Vector3 RotateDir;

            switch (orderList[currentOrderIndex])
            {
                case "Up":
                    MovingIndex = new Vector2Int(-1, 0);
                    MovingAmount = new Vector2(0, CarMovingDistance);
                    RotateDir = Vector3.forward * 180;
                    break;
                case "Down":
                    MovingIndex = new Vector2Int(1, 0);
                    MovingAmount = new Vector2(0, -CarMovingDistance);
                    RotateDir = Vector3.forward * 0;
                    break;
                case "Left":
                    MovingIndex = new Vector2Int(0, -1);
                    MovingAmount = new Vector2(-CarMovingDistance, 0);
                    RotateDir = Vector3.forward * 270;
                    break;
                case "Right":
                    MovingIndex = new Vector2Int(0, 1);
                    MovingAmount = new Vector2(CarMovingDistance, 0);
                    RotateDir = Vector3.forward * 90;
                    break;
                default:
                    MovingIndex = new Vector2Int(0, 0);
                    MovingAmount = new Vector2();
                    RotateDir = new Vector3();
                    break;
            }

            if (!Mathf.Approximately(MapCarRectTransform.rotation.z, Quaternion.Euler(RotateDir).z) ||
                !Mathf.Approximately(MapCarRectTransform.rotation.w, Quaternion.Euler(RotateDir).w))
            {
                StartCoroutine(MapCarRotate(Quaternion.Euler(RotateDir)));
                yield return new WaitForSeconds(CodingDelayTime);
            }

            currentIndex += MovingIndex;

            if (MapTile[currentIndex.x, currentIndex.y] > 0)
            {
                StartCoroutine(MapCarMove(MovingAmount));

                if (MapTile[currentIndex.x, currentIndex.y] == MD)
                {
                    isPassMiddle = true;
                }

                if (currentCanvas == Coding2Menu)
                {
                    if (MapTile[currentIndex.x, currentIndex.y] == BK ||
                        MapTile[currentIndex.x, currentIndex.y] == AB ||
                        MapTile[currentIndex.x, currentIndex.y] == PS)
                    {
                        currentOrderIndex++;
                        Coding2Waiting(true);
                        yield break;
                    }
                }
            }
            else
            {
                StartCoroutine(MapCarMove(MovingAmount / 2));

                MapCarImpactObject.SetActive(true);
                MapCarImpactObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(RotateDir + new Vector3(0, 0, 180));

                MapBGImage.texture = MapBGEndTexture;
                MapLightObject.SetActive(false);
                MapBGFailObject.SetActive(true);
                MapTimerBGImage.SetActive(true);
                CodingRestartObject.SetActive(true);
                isClear = false;
                carImage.color = darkerColor;
                carImpactImage.color = darkerColor;
                bRestartTimerEnable = true;

                AudioManager.instance.PlayAudio(AudioClipType.ResultFail);

                yield break;
            }
        }

        //Last Move
        yield return new WaitForSeconds(CodingDelayTime);

        ClearCheck();

        bRestartTimerEnable = true;
    }

    private void ClearCheck()
    {
        carImage.color = darkerColor;
        MapLightObject.SetActive(false);

        if (currentCanvas == CodingMenu)
        {
            if (MapTile[currentIndex.x, currentIndex.y] == ED && isPassMiddle)
                ResultSucceed(HUDState.Coding);

            else
                ResultFail();
        }
        else if (currentCanvas == Coding2Menu)
        {
            if (MapTile[currentIndex.x, currentIndex.y] == ED)
            {
                ResultSucceed(HUDState.Coding2);
            }
        }
    }

    private void Coding2Waiting(bool isFirst)
    {
        Coding2BlockTouchImage.SetActive(true);

        Coding2BGImage.texture = Coding2BGTextures[currentCoding2Index];

        if (isFirst)
        {
            MapBGFailImage.texture = MapBGFailTexutres[currentCoding2Index];
        }
        else
        {
            MapBGFailImage.texture = MapBGFailDefaultTexuture;
        }

        Coding2OrderObjectList[Mathf.Clamp(currentCoding2Index - 1, 0, 3)].SetActive(false);
        Coding2OrderObjectList[Mathf.Clamp(currentCoding2Index, 0, 3)].SetActive(true);
        Coding2StacksObjectList[Mathf.Clamp(currentCoding2Index - 1, 0, 3)].SetActive(false);
        Coding2StacksObjectList[Mathf.Clamp(currentCoding2Index, 0, 3)].SetActive(true);

        if (currentCoding2Index == 0)
        {
            StartCoroutine(MapCarRotate(Quaternion.Euler(Vector3.forward * 270)));
        }
        else if (currentCoding2Index == 1)
        {
            mapAmbulanceGameObject.SetActive(true);
            mapAmbulanceGameObject.GetComponent<FadeInOut>().SetFade(0.0f, 1.0f);
            mapAmbulanceGameObject.GetComponent<FadeInOut>().BeginFade();
        }
        else if (currentCoding2Index == 2)
        {
            mapPersonGameObject.SetActive(true);
            mapPersonGameObject.GetComponent<FadeInOut>().SetFade(0.0f, 1.0f);
            mapPersonGameObject.GetComponent<FadeInOut>().BeginFade();
        }
        
        StartCoroutine(CoCoding2WaitingEnd());
    }

    private IEnumerator CoCoding2WaitingEnd()
    {
        if (currentCoding2Index == 0)
            yield return new WaitForSeconds(2.0f);
        else if (currentCoding2Index == 1 || currentCoding2Index == 2)
            yield return new WaitForSeconds(4.0f);

        MapBGFailObject.SetActive(true);
        yield return new WaitForSeconds(5.0f);

        Coding2BlockTouchImage.SetActive(false);
        MapBGFailObject.SetActive(false);

    }

    public void Coding2Start()
    {
        Coding2BlockTouchImage.SetActive(true);

        List<string> tempOrders = new List<string>();

        foreach (var codingStack in Coding2StacksList)
        {
            StickerDrag childComp = codingStack.GetComponentInChildren<StickerDrag>();

            if (childComp)
            {
                tempOrders.Add(childComp.GetDirection());
            }
        }

        bool isCorrect = false;
        float waitTime = 0.0f;

        if (currentCoding2Index == 0 && tempOrders.Count >= 2)
        {
            if ((tempOrders[0] == "Red" && tempOrders[1] == "Pause") ||
                (tempOrders[0] == "Green" && tempOrders[1] == "Forward"))
            {
                isCorrect = true;
            }
        }
        else if (currentCoding2Index == 1 && tempOrders.Count >= 4)
        {
            if (tempOrders[2] == "Ambulance" && tempOrders[3] == "Out")
            {
                waitTime = 3.0f;
                mapAmbulanceGameObject.GetComponent<FadeInOut>().SetFade(1.0f, 0.0f);
                mapAmbulanceGameObject.GetComponent<FadeInOut>().BeginFade();
                isCorrect = true;
            }
        }
        else if (currentCoding2Index == 2 && tempOrders.Count >= 6)
        {
            if (tempOrders[4] == "Person" && tempOrders[5] == "Pause")
            {
                waitTime = 3.0f;
                mapPersonGameObject.GetComponent<FadeInOut>().SetFade(1.0f, 0.0f);
                mapPersonGameObject.GetComponent<FadeInOut>().BeginFade();
                isCorrect = true;
            }
        }

        if (isCorrect)
        {
            currentCoding2Index++;
            AudioManager.instance.PlayAudio(AudioClipType.ResultSucceed);
            StartCoroutine(CoOrderStart(waitTime));
        }
        else
        {
            AudioManager.instance.PlayAudio(AudioClipType.ResultFail);
            Coding2Waiting(false);
        }

    }

    private IEnumerator MapCarRotate(Quaternion RotateDir)
    {
        Quaternion startRotation = MapCarRectTransform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < CodingDelayTime)
        {
            MapCarRectTransform.rotation = Quaternion.Lerp(startRotation, RotateDir, elapsedTime / CodingDelayTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        MapCarRectTransform.rotation = RotateDir;
    }

    private IEnumerator MapCarMove(Vector2 MoveAmount)
    {
        Vector2 startPosition = MapCarRectTransform.anchoredPosition;
        Vector2 endPosition = MapCarRectTransform.anchoredPosition + MoveAmount;
        float elapsedTime = 0f;

        while (elapsedTime < CodingDelayTime)
        {
            MapCarRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime / CodingDelayTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        MapCarRectTransform.anchoredPosition = endPosition;
    }

    public void CodingEnd()
    {
        carImage.color = carOriginalColor;
        restartTimer = 0.0f;
        bRestartTimerEnable = false;

        if (isClear)
        {
            isClear = false;
            ChangeStatus(HUDState.Coding2);
        }
        else
        {
            StopAllCoroutines();
            ResetMapSettings();
        }
    }

    public void Restart()
    {
        restartTimer = 0.0f;
        bRestartTimerEnable = false;

        if (isClear)
        {
            ChangeStatus(HUDState.Main);
        }
        else
        {
            ResetMap2Settings();
        }
    }

    public bool NowWaitStatus()
    {
        bool isWaitingStatus = currentCanvas == MainMenu ||
                               (currentCanvas == CodingMenu && CodingBlockTouchImage.activeSelf);

        return isWaitingStatus;
    }

    void ResultFail()
    {
        AudioManager.instance.PlayAudio(AudioClipType.ResultFail);

        MapBGImage.texture = MapBGEndTexture;
        MapBGFailObject.SetActive(true);
        MapTimerBGImage.SetActive(true);
        CodingRestartObject.SetActive(true);
        MapLightObject.SetActive(false);
        isClear = false;
    }

    void ResultSucceed(HUDState type) 
    {
        AudioManager.instance.PlayAudio(AudioClipType.ResultSucceed);

        if (type == HUDState.Coding)
        {
            MapBGSuccessImage.texture = MapBGSuccessTexutres[0];
            CodingRestartObject.SetActive(true);
        }
        else if (type == HUDState.Coding2)
        {
            MapBGSuccessImage.texture = MapBGSuccessTexutres[1];
            Coding2RestartObject.SetActive(true);
        }

        MapBGImage.texture = MapBGEndTexture;
        MapBGSuccessObject.SetActive(true);
        MapTimerBGImage.SetActive(true);
        MapLightObject.SetActive(false);
        isClear = true;
    }

}

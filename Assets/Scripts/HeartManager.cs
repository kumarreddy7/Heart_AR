using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HeartManager : MonoBehaviour
{
    [Header("AR Components")]
    private ARRaycastManager arRaycastManager;
    private Camera arCamera;

    [Header("Prefabs")]
    public GameObject heartPrefab;
    public GameObject lightPivotPrefab;
    public GameObject uiPanelPrefab;
    public GameObject labelPrefab;
    public GameObject reticlePrefab;

    [Header("UI")]
    public Button deleteButton;
    public Button toggleModeButton;
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public Sprite heartBeatSprite;
    public Sprite heartCSSprite;

    [Header("Settings")]
    public float placementHeight = 0.25f;

    private GameObject spawnedHeart;
    private GameObject lightPivot;
    private GameObject spawnedUIPanel;
    private GameObject spawnedReticle;

    private Animator heartAnimator;
    private Transform fullHeart;
    private Transform heartCS;

    private float previousPinchDistance = 0f;
    private Vector3 lightOffset;

    private ScrollRect scrollRect;
    private RectTransform contentTransform;
    private Image imageCurrent;
    private Image imageNext;
    private Image panelImage;

    private List<GameObject> spawnedLabels = new List<GameObject>();
    private Dictionary<string, Transform> heartParts = new Dictionary<string, Transform>();
    private Dictionary<string, Vector3> labelOffsets = new Dictionary<string, Vector3>();

    void Start()
    {
        arCamera = Camera.main;
        arRaycastManager = GetComponent<ARRaycastManager>();

        deleteButton.onClick.AddListener(DeleteHeart);
        toggleModeButton.onClick.AddListener(OnToggleModePressed);

        deleteButton.gameObject.SetActive(false);
        toggleModeButton.gameObject.SetActive(false);
        toggleModeButton.image.sprite = toggleOffSprite;

        SetupLabelOffsets();

        // Spawn Reticle
        if (reticlePrefab != null)
        {
            spawnedReticle = Instantiate(reticlePrefab);
            spawnedReticle.SetActive(false);
        }
    }

    void Update()
    {
        UpdateReticle();

        if (Input.touchCount == 1 && spawnedHeart == null && spawnedReticle.activeSelf)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                PlaceHeartModel(spawnedReticle.transform.position);
            }
        }

        HandlePinchZoom();

        if (spawnedHeart && lightPivot)
        {
            lightPivot.transform.position = spawnedHeart.transform.position + lightOffset;
            lightPivot.transform.rotation = Quaternion.Euler(0, arCamera.transform.eulerAngles.y, 0);
        }

        if (spawnedHeart && spawnedUIPanel)
        {
            Vector3 toCamera = arCamera.transform.position - spawnedHeart.transform.position;
            toCamera.y = 0;
            toCamera.Normalize();

            Vector3 offset = -toCamera * 1.4f + Vector3.up * 0.65f + Vector3.left * 1.2f;
            Vector3 targetPosition = spawnedHeart.transform.position + offset;

            spawnedUIPanel.transform.position = Vector3.Lerp(spawnedUIPanel.transform.position, targetPosition, Time.deltaTime * 5f);

            Vector3 lookDirection = arCamera.transform.position - spawnedUIPanel.transform.position;
            lookDirection.y = 0;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                spawnedUIPanel.transform.rotation = Quaternion.Lerp(spawnedUIPanel.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    void UpdateReticle()
    {
        if (!spawnedReticle || spawnedHeart != null)
        {
            if (spawnedReticle) spawnedReticle.SetActive(false);
            return;
        }

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            spawnedReticle.SetActive(true);
            spawnedReticle.transform.SetPositionAndRotation(hitPose.position, Quaternion.Euler(90, 0, 0));
        }
        else
        {
            spawnedReticle.SetActive(false);
        }
    }

    void PlaceHeartModel(Vector3 position)
    {
        spawnedHeart = Instantiate(heartPrefab, position + Vector3.up * placementHeight, Quaternion.identity);
        spawnedHeart.transform.rotation *= Quaternion.Euler(0, -150, 0);

        heartAnimator = spawnedHeart.GetComponent<Animator>();
        fullHeart = spawnedHeart.transform.Find("heart1");
        heartCS = spawnedHeart.transform.Find("heart_cs1");

        if (fullHeart && heartCS)
        {
            fullHeart.gameObject.SetActive(true);
            heartCS.gameObject.SetActive(false);
        }

        lightPivot = Instantiate(lightPivotPrefab, spawnedHeart.transform.position + new Vector3(0, 0.5f, -2f), Quaternion.identity);
        lightOffset = lightPivot.transform.position - spawnedHeart.transform.position;

        ToggleHeartCS(false);
        SetupUIPanel();

        deleteButton.gameObject.SetActive(true);
        toggleModeButton.gameObject.SetActive(true);
        toggleModeButton.image.sprite = toggleOffSprite;

        if (spawnedReticle)
            spawnedReticle.SetActive(false);
    }

    void SetupUIPanel()
    {
        if (uiPanelPrefab && spawnedUIPanel == null)
        {
            Vector3 panelPos = spawnedHeart.transform.position + new Vector3(-1.75f, 0.65f, 1.5f);
            spawnedUIPanel = Instantiate(uiPanelPrefab, panelPos, Quaternion.Euler(0, -60, 0));

            scrollRect = spawnedUIPanel.GetComponentInChildren<ScrollRect>();
            contentTransform = scrollRect.content;

            imageCurrent = contentTransform.Find("Image_Current")?.GetComponent<Image>();
            imageNext = contentTransform.Find("Image_Next")?.GetComponent<Image>();
            panelImage = spawnedUIPanel.GetComponentInChildren<Image>();

            if (imageCurrent && heartBeatSprite)
                imageCurrent.sprite = heartBeatSprite;
        }
    }

    void DeleteHeart()
    {
        foreach (GameObject label in spawnedLabels)
            Destroy(label);

        spawnedLabels.Clear();

        Destroy(spawnedHeart);
        Destroy(lightPivot);
        Destroy(spawnedUIPanel);

        spawnedHeart = null;
        lightPivot = null;
        spawnedUIPanel = null;

        ToggleHeartCS(false);
        deleteButton.gameObject.SetActive(false);
        toggleModeButton.gameObject.SetActive(false);

        if (spawnedReticle)
            spawnedReticle.SetActive(true);
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2 && spawnedHeart)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(t0.position, t1.position);

            if (previousPinchDistance > 0f)
            {
                float delta = currentDistance - previousPinchDistance;
                float scaleFactor = delta * 0.001f;
                spawnedHeart.transform.localScale += Vector3.one * scaleFactor;
            }

            previousPinchDistance = currentDistance;
        }
        else
        {
            previousPinchDistance = 0f;
        }
    }

    void OnToggleModePressed()
    {
        if (!spawnedHeart || !heartAnimator) return;

        string currentAnim = heartAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        bool goingToCS = currentAnim != "HeartCS";

        ToggleHeartCS(goingToCS);

        if (toggleModeButton && toggleModeButton.image)
        {
            toggleModeButton.image.sprite = goingToCS ? toggleOnSprite : toggleOffSprite;
        }
    }

    void ToggleHeartCS(bool enable)
    {
        if (!fullHeart || !heartCS || !heartAnimator) return;

        string currentAnim = heartAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (enable && currentAnim != "HeartCS")
        {
            PlayHeartCSAnimation();
        }
        else if (!enable && currentAnim != "HeartBeat")
        {
            PlayHeartBeatAnimation();
        }
    }

    void PlayHeartCSAnimation()
    {
        heartCS.gameObject.SetActive(true);
        fullHeart.gameObject.SetActive(false);
        heartAnimator.speed = 1f;
        heartAnimator.Play("HeartCS", 0, 0);

        if (panelImage) StartCoroutine(SmoothScrollToSprite(heartCSSprite));
        CacheHeartParts();
        SpawnHeartLabels();
    }

    void PlayHeartBeatAnimation()
    {
        foreach (GameObject label in spawnedLabels)
            Destroy(label);
        spawnedLabels.Clear();

        heartCS.gameObject.SetActive(false);
        fullHeart.gameObject.SetActive(true);
        heartAnimator.speed = 1.8f;
        heartAnimator.Play("HeartBeat", 0, 0);

        if (panelImage) StartCoroutine(SmoothScrollToSprite(heartBeatSprite));
    }

    IEnumerator SmoothScrollToSprite(Sprite newSprite)
    {
        float duration = 0.65f;
        float elapsedTime = 0f;

        if (!imageCurrent || !imageNext || !contentTransform) yield break;

        imageNext.sprite = newSprite;
        contentTransform.anchoredPosition = Vector2.zero;
        Vector2 targetPos = new Vector2(-contentTransform.rect.width / 2f, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            contentTransform.anchoredPosition = Vector2.Lerp(contentTransform.anchoredPosition, targetPos, elapsedTime / duration);
            yield return null;
        }

        imageCurrent.sprite = newSprite;
        contentTransform.anchoredPosition = Vector2.zero;
    }

    void CacheHeartParts()
    {
        heartParts.Clear();
        Transform[] allChildren = heartCS.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            string name = child.name.ToLower();
            if (name.Contains("aorta")) heartParts["Aorta"] = child;
            else if (name.Contains("pulmonary_artery")) heartParts["Pulmonary Artery"] = child;
            else if (name.Contains("left_ventrical")) heartParts["Left Ventricle"] = child;
            else if (name.Contains("right_ventrical")) heartParts["Right Ventricle"] = child;
            else if (name.Contains("left_atrium")) heartParts["Left Atrium"] = child;
            else if (name.Contains("right_atrium")) heartParts["Right Atrium"] = child;
            else if (name.Contains("aortic_valve1")) heartParts["Aortic Valve"] = child;
            else if (name.Contains("pulmonary_valve1")) heartParts["Pulmonary Valve"] = child;
            else if (name.Contains("tricuspid_valve")) heartParts["Tricuspid Valve"] = child;
            else if (name.Contains("mitral_valve")) heartParts["Mitral Valve"] = child;
            else if (name.Contains("cardiac_nerves_01")) heartParts["Cardiac Nerves"] = child;
            else if (name.Contains("vein2")) heartParts["Veins"] = child;
        }
    }

    void SetupLabelOffsets()
    {
        labelOffsets.Clear();

        // Left side
        labelOffsets["Cardiac Nerves"] = new Vector3(-0.5f, 0.25f, -0.75f);
        labelOffsets["Right Atrium"] = new Vector3(-0.65f, 0.3f, -0.5f);
        labelOffsets["Aortic Valve"] = new Vector3(-1.25f, 0, -0.5f);
        labelOffsets["Right Ventricle"] = new Vector3(-0.5f, 0.2f, -0.75f);
        labelOffsets["Tricuspid Valve"] = new Vector3(-0.7f, 0, -0.75f);
        labelOffsets["Veins"] = new Vector3(-0.75f, 0, -0.4f);

        // Right side
        labelOffsets["Aorta"] = new Vector3(0.5f, 0.65f, -0.4f);
        labelOffsets["Pulmonary Artery"] = new Vector3(0.6f, 0.7f, -0.75f);
        labelOffsets["Pulmonary Valve"] = new Vector3(0.75f, 0.6f, -0.75f);
        labelOffsets["Left Atrium"] = new Vector3(0.7f, 0.2f, -0.5f);
        labelOffsets["Left Ventricle"] = new Vector3(0.75f, 0.25f, -0.75f);
        labelOffsets["Mitral Valve"] = new Vector3(0.9f, 0.1f, -0.75f);
    }

    void SpawnHeartLabels()
    {
        foreach (var part in heartParts)
        {
            if (part.Value == null) continue;

            Vector3 offset = labelOffsets.ContainsKey(part.Key) ? labelOffsets[part.Key] : new Vector3(0.2f, 0.1f, 0.5f);
            Vector3 labelPos = part.Value.position + offset;

            GameObject label = Instantiate(labelPrefab, labelPos, Quaternion.identity);
            label.GetComponentInChildren<TextMeshPro>().text = part.Key;

            LabelFollow follow = label.GetComponent<LabelFollow>();
            if (follow != null)
            {
                follow.Initialize(part.Value, arCamera);
                follow.labelOffset = offset;
            }

            spawnedLabels.Add(label);
        }
    }
}
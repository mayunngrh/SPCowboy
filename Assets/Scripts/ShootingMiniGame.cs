using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShootingMinigame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shootingPanel;
    public RectTransform shootingBar;
    public RectTransform marker;
    public Button shootButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI instructionText;

    [Header("Settings")]
    public float barWidth = 600f;
    public float baseMarkerSpeed = 220f;

    private List<GameObject> zoneObjects = new List<GameObject>();
    private ShotSetup currentShot;
    private float markerPos = 0f;
    private float markerDir = 1f;
    private bool isRunning = false;
    private bool hasFired = false;

    void Start()
    {
        if (shootingPanel != null)
            shootingPanel.SetActive(false);
    }

    public void OpenShootingPhase(ShotSetup shot)
    {
        currentShot = shot;
        hasFired = false;
        markerPos = 0f;
        markerDir = 1f;

        shootingPanel.SetActive(true);
        RefreshZones(shot);

        if (resultText != null)
            resultText.text = "";

        if (instructionText != null)
            instructionText.text = shot.hitZones.Count > 0
                ? "Stop the marker on a hit zone!"
                : "No hit zones — miss guaranteed!";

        isRunning = true;

        if (shootButton != null)
            shootButton.interactable = true;
    }

    void Update()
    {
        if (!isRunning || hasFired) return;

        float speed = baseMarkerSpeed * currentShot.markerSpeed;
        markerPos += speed * markerDir * Time.deltaTime;

        if (markerPos >= barWidth - 8f)
        {
            markerPos = barWidth - 8f;
            markerDir = -1f;
        }
        if (markerPos <= 0f)
        {
            markerPos = 0f;
            markerDir = 1f;
        }

        if (marker != null)
            marker.anchoredPosition = new Vector2(markerPos, 0);
    }

    public void RefreshZones(ShotSetup shot)
    {
        foreach (GameObject z in zoneObjects)
            Destroy(z);
        zoneObjects.Clear();

        if (shot == null) return;

        foreach (HitZone zone in shot.hitZones)
            CreateZoneVisual(zone);
    }

    void CreateZoneVisual(HitZone zone)
    {
        GameObject zoneObj = new GameObject("Zone_" + zone.element);
        zoneObj.transform.SetParent(shootingBar, false);
        zoneObj.transform.SetAsFirstSibling();

        Image img = zoneObj.AddComponent<Image>();
        img.color = GetElementColor(zone.element);

        RectTransform rt = zoneObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(zone.position, 0);
        rt.sizeDelta = new Vector2(zone.width, 0);

        // Label inside zone
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(zoneObj.transform, false);

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = zone.element.ToString()
            + "\n" + zone.damage
            + (zone.bonusDamage > 0 ? "+" + zone.bonusDamage : "");
        label.fontSize = 11;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        RectTransform lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        zoneObjects.Add(zoneObj);
    }

    Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color(0.91f, 0.36f, 0.14f, 0.85f);
            case ElementType.Water: return new Color(0.22f, 0.54f, 0.87f, 0.85f);
            case ElementType.Wood: return new Color(0.39f, 0.60f, 0.13f, 0.85f);
            case ElementType.Metal: return new Color(0.53f, 0.53f, 0.50f, 0.85f);
            case ElementType.Earth: return new Color(0.73f, 0.46f, 0.09f, 0.85f);
            default: return new Color(0.50f, 0.47f, 0.87f, 0.85f);
        }
    }

    public void OnShootPressed()
    {
        if (hasFired) return;
        hasFired = true;
        isRunning = false;

        if (shootButton != null)
            shootButton.interactable = false;

        StartCoroutine(ResolveShot());
    }

    IEnumerator ResolveShot()
    {
        // Flash marker red
        Image markerImg = marker.GetComponent<Image>();
        if (markerImg != null) markerImg.color = Color.red;

        yield return new WaitForSeconds(0.4f);

        // Send result to CombatManager
        CombatManager cm = FindFirstObjectByType<CombatManager>();
        cm.OnPlayerShoot(markerPos);

        // Show result for 1.5 seconds then close
        yield return new WaitForSeconds(1.5f);

        shootingPanel.SetActive(false);

        if (markerImg != null)
            markerImg.color = Color.white;
    }
}
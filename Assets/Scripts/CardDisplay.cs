using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDisplay : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Card UI")]
    public CardData cardData;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardCostText;
    public TextMeshProUGUI cardDescText;
    public Image cardArtwork;
    public Image cardBackground;

    [Header("Colors")]
    public Color basicColor = new Color(0.8f, 0.7f, 0.2f);       // Yellow
    public Color elementalColor = new Color(0.8f, 0.2f, 0.2f);   // Red
    public Color buffColor = new Color(0.2f, 0.8f, 0.2f);        // Green
    public Color utilityColor = new Color(0.2f, 0.6f, 0.8f);     // Light Blue
    public Color tradeoffColor = new Color(0.6f, 0.2f, 0.8f);    // Purple
    public Color supportColor = new Color(0.8f, 0.5f, 0.2f);     // Orange

    [Header("Hover/Select Tuning")]
    public float hoverLift = 35f;
    public float selectLift = 25f;
    public float tweenSpeed = 14f;
    public float floatAmplitude = 5f;
    public float floatSpeed = 1.2f;

    // State
    public bool isSelected = false;
    private bool isHovered = false;
    private bool isReady = false;
    public bool isDiscarding = false;

    // References
    private HandManager handManager;
    private RectTransform rectTransform;
    private float floatPhase;

    // Position model
    private Vector3 stackPosition;
    private Vector3 currentDisplayOffset;
    private Vector3 currentSpreadOffset;
    private Vector3 targetSpreadOffset;
    private Vector3 targetStackPosition;

    // Glow
    private Image glowImage;

    void Awake()
    {
        handManager = FindFirstObjectByType<HandManager>();
        rectTransform = GetComponent<RectTransform>();
        floatPhase = Random.Range(0f, Mathf.PI * 2f);

        // Only the root catches pointer events. Child graphics (esp. text wider
        // than the card) would otherwise fire spurious enter/exit on neighbors.
        Graphic[] childGraphics = GetComponentsInChildren<Graphic>(true);
        Graphic rootGraphic = GetComponent<Graphic>();
        foreach (Graphic g in childGraphics)
        {
            if (g == rootGraphic) continue;
            g.raycastTarget = false;
        }
        if (rootGraphic != null) rootGraphic.raycastTarget = true;
    }

    void LateUpdate()
    {
        if (!isReady) return;
        if (isDiscarding) return;

        Vector3 targetOffset = Vector3.zero;
        if (isSelected) targetOffset = new Vector3(0f, selectLift, 0f);
        if (isHovered) targetOffset = new Vector3(0f, hoverLift, 0f);

        float t = 1f - Mathf.Exp(-tweenSpeed * Time.deltaTime);
        stackPosition = Vector3.Lerp(stackPosition, targetStackPosition, t);
        currentDisplayOffset = Vector3.Lerp(currentDisplayOffset, targetOffset, t);
        currentSpreadOffset = Vector3.Lerp(currentSpreadOffset, targetSpreadOffset, t);

        Vector3 floatVec = Vector3.zero;
        if (!isHovered && !isSelected)
        {
            float floatY = Mathf.Sin(Time.time * floatSpeed + floatPhase) * floatAmplitude;
            floatVec = new Vector3(0f, floatY, 0f);
        }

        rectTransform.localPosition = stackPosition + currentDisplayOffset
            + currentSpreadOffset + floatVec;

        if (isSelected && glowImage != null)
        {
            float alpha = Mathf.Lerp(0.3f, 0.9f,
                (Mathf.Sin(Time.time * 4f) + 1f) / 2f);
            glowImage.color = new Color(1f, 0.9f, 0.1f, alpha);
        }
    }
    public void Setup(CardData data)
    {
        cardData = data;
        cardNameText.text = data.cardName;
        cardCostText.text = data.energyCost.ToString();
        cardDescText.text = data.description;

        if (data.artwork != null)
            cardArtwork.sprite = data.artwork;

        switch (data.cardType)
        {
            case CardType.Basic:
                cardBackground.color = new Color(0.8f, 0.2f, 0.2f); // red
                break;
            case CardType.Elemental:
                cardBackground.color = GetElementColor(data.element);
                break;
            case CardType.Buff:
                cardBackground.color = new Color(0.2f, 0.4f, 0.8f); // blue
                break;
            case CardType.Utility:
                cardBackground.color = new Color(0.7f, 0.5f, 0.1f); // amber
                break;
            case CardType.Tradeoff:
                cardBackground.color = new Color(0.8f, 0.2f, 0.4f); // pink-red
                break;
            case CardType.Support:
                cardBackground.color = new Color(0.1f, 0.6f, 0.3f); // green
                break;
            default:
                cardBackground.color = new Color(0.5f, 0.5f, 0.5f); // gray
                break;
        }

        if (glowImage == null) CreateGlowOverlay();
    }

    Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color(0.91f, 0.36f, 0.14f);
            case ElementType.Water: return new Color(0.22f, 0.54f, 0.87f);
            case ElementType.Wood: return new Color(0.39f, 0.60f, 0.13f);
            case ElementType.Metal: return new Color(0.53f, 0.53f, 0.50f);
            case ElementType.Earth: return new Color(0.73f, 0.46f, 0.09f);
            default: return new Color(0.50f, 0.47f, 0.87f);
        }
    }
    
    void CreateGlowOverlay()
    {
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.SetAsFirstSibling();

        glowImage = glowObj.AddComponent<Image>();
        glowImage.color = new Color(1f, 0.9f, 0.1f, 0f);
        glowImage.raycastTarget = false;

        RectTransform rt = glowObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-6, -6);
        rt.offsetMax = new Vector2(6, 6);
    }

    public void SetReady(Vector3 basePos)
    {
        stackPosition = basePos;
        targetStackPosition = basePos;
        currentDisplayOffset = Vector3.zero;
        currentSpreadOffset = Vector3.zero;
        targetSpreadOffset = Vector3.zero;
        isReady = true;
    }

    public void SetTargetPosition(Vector3 newPos)
    {
        targetStackPosition = newPos;
    }

    public void SetSpreadOffset(Vector3 offset)
    {
        targetSpreadOffset = offset;
    }

    public void SetDiscarding()
    {
        isDiscarding = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;
        isHovered = true;
        transform.SetAsLastSibling();
        if (handManager != null) handManager.OnCardHoverEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;
        isHovered = false;
        if (handManager != null) handManager.OnCardHoverExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CombatManager cm = FindFirstObjectByType<CombatManager>();

        if (!isSelected)
        {
            int totalCost = handManager.GetSelectedEnergyCost()
                + cardData.energyCost;
            if (totalCost > cm.currentEnergy)
            {
                StartCoroutine(FlashRed());
                return;
            }

            isSelected = true;
            isHovered = false;
            transform.SetAsLastSibling();
            handManager.SelectCard(this);
        }
        else
        {
            isSelected = false;

            if (glowImage != null)
                glowImage.color = new Color(1f, 0.9f, 0.1f, 0f);

            handManager.DeselectCard(this);
        }
    }

    IEnumerator FlashRed()
    {
        Color original = cardBackground.color;
        cardBackground.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        cardBackground.color = original;
    }
}

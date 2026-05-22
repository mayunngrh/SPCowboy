using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handArea;
    public DeckManager deckManager;
    public TextMeshProUGUI selectedOrderText;

    [Header("Draw Animation")]
    public RectTransform drawPileIcon;
    public TextMeshProUGUI drawPileCountText;
    public float drawAnimDuration = 0.3f;
    public float drawDelayBetweenCards = 0.15f;

    [Header("Discard Animation")]
    public RectTransform discardPileIcon;
    public TextMeshProUGUI discardPileCountText;
    public float discardAnimDuration = 0.4f;

    [Header("Hover Spread")]
    public float hoverSpread = 35f; // how far neighbors slide aside

    private List<GameObject> cardObjects = new List<GameObject>();
    private List<CardDisplay> selectedCards = new List<CardDisplay>();

    private CardDisplay currentHovered;

    void Start()
    {
        Invoke("DrawHandWithAnimation", 0.3f);
    }

    public void DisplayHand()
    {
        foreach (GameObject card in cardObjects)
            Destroy(card);
        cardObjects.Clear();
        selectedCards.Clear();

        for (int i = 0; i < deckManager.hand.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            display.Setup(deckManager.hand[i]);
            cardObjects.Add(cardObj);
        }

        UpdateDrawPileCount();
    }

    public void DrawHandWithAnimation()
    {
        foreach (GameObject card in cardObjects)
            Destroy(card);
        cardObjects.Clear();
        selectedCards.Clear();

        StartCoroutine(DrawCardsOneByOne());
    }

    IEnumerator DrawCardsOneByOne()
    {
        for (int i = 0; i < deckManager.hand.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            display.Setup(deckManager.hand[i]);
            cardObjects.Add(cardObj);
        }

        yield return null;
        yield return null;

        // Stack cards with offset
        List<RectTransform> cardRTs = new List<RectTransform>();
        List<Vector3> finalPositions = new List<Vector3>();

        int count = cardObjects.Count;
        float stackOffsetX = 140f;
        float stackOffsetY = 0f;
        float totalWidth = stackOffsetX * (count - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardObjects.Count; i++)
        {
            RectTransform rt = cardObjects[i].GetComponent<RectTransform>();
            cardRTs.Add(rt);

            // Stack position
            Vector3 stackPos = new Vector3(
                startX + i * stackOffsetX,
                i * stackOffsetY,
                0);

            finalPositions.Add(stackPos);
            rt.localPosition = stackPos;
            rt.localScale = Vector3.zero;
        }

        // Animate each card flying from pile
        Vector3 pilePos = GetDrawPileScreenPos();

        for (int i = 0; i < cardRTs.Count; i++)
        {
            cardRTs[i].localPosition = pilePos;
            int index = i; // capture for lambda

            StartCoroutine(AnimateCardFromPileAndReady(
                cardRTs[i], pilePos, finalPositions[i],
                cardObjects[i].GetComponent<CardDisplay>()));

            UpdateDrawPileCount();
            yield return new WaitForSeconds(drawDelayBetweenCards);
        }
    }

    Vector3 GetDrawPileScreenPos()
    {
        if (drawPileIcon == null) return Vector3.zero;

        // Convert draw pile world pos to HandArea local pos
        Vector3 worldPos = drawPileIcon.position;
        Vector3 localPos = handArea.InverseTransformPoint(worldPos);
        return localPos;
    }

    Vector3 GetDiscardPileScreenPos()
    {
        if (discardPileIcon == null)
        {
            Debug.LogError("Discard Pile Icon not assigned!");
            return Vector3.zero;
        }

        // Get position relative to canvas
        Vector3 discardWorldPos = discardPileIcon.position;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        Vector3 localPos = canvas.transform.InverseTransformPoint(discardWorldPos);

        // Adjust to handArea coordinates
        Vector3 handAreaPos = handArea.InverseTransformPoint(
            canvas.transform.TransformPoint(localPos));

        Debug.Log("Discard target pos: " + handAreaPos);
        return handAreaPos;
    }
    void UpdateDiscardPileCount()
    {
        if (discardPileCountText != null)
            discardPileCountText.text = deckManager.discardPile.Count.ToString();
    }

    IEnumerator AnimateCardFromPileAndReady(RectTransform card,
        Vector3 startPos, Vector3 endPos, CardDisplay display)
    {
        float elapsed = 0f;
        card.localPosition = startPos;
        card.localScale = Vector3.one * 0.3f;

        while (elapsed < drawAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / drawAnimDuration, 3f);
            card.localPosition = Vector3.Lerp(startPos, endPos, t);
            card.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one, t);
            yield return null;
        }

        card.localPosition = endPos;
        card.localScale = Vector3.one;

        // Tell card it's ready — start floating
        display.SetReady(endPos);
    }

    public IEnumerator AnimateCardToDiscard(RectTransform card)
    {
        Vector3 startPos = card.localPosition;
        Vector3 endPos = GetDiscardPileScreenPos();

        Debug.Log("Animating to discard: start=" + startPos
            + " end=" + endPos);

        float elapsed = 0f;

        while (elapsed < discardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - elapsed / discardAnimDuration, 3f);

            card.localPosition = Vector3.Lerp(startPos, endPos, t);
            card.localScale = Vector3.Lerp(Vector3.one,
                Vector3.one * 0.4f, t);

            yield return null;
        }

        Debug.Log("Discard animation complete. Final pos: "
            + card.localPosition);

        card.localPosition = endPos;
        card.localScale = Vector3.one * 0.4f;
    }

    public void OnCardHoverEnter(CardDisplay card)
    {
        currentHovered = card;
        ApplySpread(card);
    }

    public void OnCardHoverExit(CardDisplay card)
    {
        // Only clear if this card still owns the hover.
        if (currentHovered != card) return;
        currentHovered = null;
        ApplySpread(null);
    }

    void ApplySpread(CardDisplay hovered)
    {
        int hoveredIndex = hovered != null
            ? cardObjects.IndexOf(hovered.gameObject)
            : -1;

        for (int i = 0; i < cardObjects.Count; i++)
        {
            CardDisplay cd = cardObjects[i].GetComponent<CardDisplay>();
            if (cd == null) continue;

            if (hoveredIndex < 0 || i == hoveredIndex)
            {
                cd.SetSpreadOffset(Vector3.zero);
                continue;
            }

            float dir = i < hoveredIndex ? -1f : 1f;
            cd.SetSpreadOffset(new Vector3(dir * hoverSpread, 0f, 0f));
        }
    }

    void UpdateDrawPileCount()
    {
        if (drawPileCountText != null)
            drawPileCountText.text = deckManager.drawPile.Count.ToString();
    }

    public void SelectCard(CardDisplay card)
    {
        if (!selectedCards.Contains(card))
        {
            selectedCards.Add(card);
            UpdateOrderDisplay();
        }
    }

    public void DeselectCard(CardDisplay card)
    {
        selectedCards.Remove(card);
        UpdateOrderDisplay();
    }

    void UpdateUnselectedCardPositions()
    {
        List<CardDisplay> unselected = new List<CardDisplay>();
        for (int i = 0; i < cardObjects.Count; i++)
        {
            CardDisplay cd = cardObjects[i].GetComponent<CardDisplay>();
            if (cd != null)
                unselected.Add(cd);
        }

        if (unselected.Count == 0) return;

        float stackOffsetX = 140f;
        float totalWidth = stackOffsetX * (unselected.Count - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < unselected.Count; i++)
        {
            Vector3 newPos = new Vector3(startX + i * stackOffsetX, 0f, 0f);
            unselected[i].SetTargetPosition(newPos);
        }
    }

    public int GetSelectedEnergyCost()
    {
        int total = 0;
        foreach (CardDisplay card in selectedCards)
            total += card.cardData.energyCost;
        return total;
    }

    void UpdateOrderDisplay()
    {
        if (selectedOrderText == null) return;

        if (selectedCards.Count == 0)
        {
            selectedOrderText.text = "No cards selected";
            return;
        }

        string order = "Queue: ";
        for (int i = 0; i < selectedCards.Count; i++)
        {
            order += (i + 1) + ". " + selectedCards[i].cardData.cardName;
            if (i < selectedCards.Count - 1) order += " → ";
        }
        selectedOrderText.text = order;
    }

    public void RollTheCards()
    {
        if (selectedCards.Count == 0)
        {
            Debug.Log("No cards selected!");
            return;
        }

        List<CardDisplay> toPlay = new List<CardDisplay>(selectedCards);
        selectedCards.Clear();
        UpdateOrderDisplay();

        StartCoroutine(PlayCardsSequentially(toPlay));
    }

    IEnumerator PlayCardsSequentially(List<CardDisplay> cards)
    {
        CombatManager combatManager = FindFirstObjectByType<CombatManager>();
        AnimationManager animManager = FindFirstObjectByType<AnimationManager>();

        foreach (CardDisplay card in cards)
        {
            if (!combatManager.SpendEnergy(card.cardData.energyCost))
            {
                Debug.Log("Not enough energy! Stopping.");
                break;
            }

            // Play card effect animation
            switch (card.cardData.cardType)
            {
                case CardType.Attack:
                    yield return StartCoroutine(
                        animManager.PlayAttackAnimation(
                            card.cardData, combatManager.enemy,
                            card.cardData.damageAmount));
                    combatManager.enemy.TakeDamage(card.cardData.damageAmount);
                    break;

                case CardType.Defense:
                    yield return StartCoroutine(
                        animManager.PlayDefenseAnimation(
                            card.cardData.blockAmount));
                    combatManager.player.GainBlock(card.cardData.blockAmount);
                    break;

                case CardType.Magic:
                    yield return StartCoroutine(
                        animManager.PlayAttackAnimation(
                            card.cardData, combatManager.enemy,
                            card.cardData.damageAmount));
                    combatManager.enemy.TakeDamage(card.cardData.damageAmount);
                    break;
            }

            yield return new WaitForSeconds(0.3f);

            // Mark card as discarding so it stops updating position
            card.SetDiscarding();

            // Animate card to discard pile — WAIT FOR THIS TO COMPLETE
            RectTransform cardRT = card.GetComponent<RectTransform>();
            yield return StartCoroutine(AnimateCardToDiscard(cardRT));

            // THEN remove from hand
            deckManager.hand.Remove(card.cardData);
            deckManager.discardPile.Add(card.cardData);
            cardObjects.Remove(card.gameObject);
            Destroy(card.gameObject);

            UpdateDiscardPileCount();
            UpdateUnselectedCardPositions();
        }
    }

    public IEnumerator DiscardRemainingCards()
    {
        List<GameObject> cardsToDiscard = new List<GameObject>(cardObjects);

        foreach (GameObject cardObj in cardsToDiscard)
        {
            CardDisplay card = cardObj.GetComponent<CardDisplay>();
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();

            // Mark card as discarding so it stops updating position
            card.SetDiscarding();

            // Animate to discard
            yield return StartCoroutine(AnimateCardToDiscard(cardRT));

            // Remove
            deckManager.hand.Remove(card.cardData);
            deckManager.discardPile.Add(card.cardData);
            cardObjects.Remove(cardObj);
            Destroy(cardObj);

            UpdateDiscardPileCount();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
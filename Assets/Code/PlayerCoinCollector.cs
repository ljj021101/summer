using System.Collections.Generic;
using UnityEngine;

public class PlayerCoinCollector : MonoBehaviour
{
    [SerializeField] private Transform carryRoot;

    private readonly List<CollectableCoin> carriedCoins = new List<CollectableCoin>();
    private PlatformerPlayerController playerController;

    private Transform CarryRoot => carryRoot != null ? carryRoot : transform;
    private int FacingDirection => playerController != null ? playerController.FacingDirection : GetFallbackFacingDirection();

    private void Awake()
    {
        playerController = GetComponent<PlatformerPlayerController>();
    }

    private void Update()
    {
        RefreshCarrySlots();
    }

    public void TryPickUp(CollectableCoin coin)
    {
        if (coin == null || carriedCoins.Contains(coin))
        {
            return;
        }

        carriedCoins.Add(coin);
        coin.PickUp(CarryRoot, carriedCoins.Count - 1);
        RefreshCarrySlots();
    }

    public bool BankCarriedCoins()
    {
        if (carriedCoins.Count == 0)
        {
            return false;
        }

        bool hadCarriedCoins = true;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RefreshCoinTotalFromScene();

            for (int i = 0; i < carriedCoins.Count; i++)
            {
                CollectableCoin coin = carriedCoins[i];
                if (coin != null)
                {
                    LevelManager.Instance.MarkCoinCollected(coin.CoinId);
                }
            }
        }

        for (int i = carriedCoins.Count - 1; i >= 0; i--)
        {
            CollectableCoin coin = carriedCoins[i];
            if (coin != null)
            {
                coin.CompleteCollection();
            }
        }

        carriedCoins.Clear();
        return hadCarriedCoins;
    }

    public void ReturnCarriedCoins()
    {
        for (int i = carriedCoins.Count - 1; i >= 0; i--)
        {
            CollectableCoin coin = carriedCoins[i];
            if (coin != null)
            {
                coin.ReturnToOriginalPosition();
            }
        }

        carriedCoins.Clear();
    }

    private void RefreshCarrySlots()
    {
        int facingDirection = FacingDirection;

        for (int i = 0; i < carriedCoins.Count; i++)
        {
            CollectableCoin coin = carriedCoins[i];
            if (coin != null)
            {
                coin.UpdateCarrySlot(facingDirection, i);
            }
        }
    }

    private int GetFallbackFacingDirection()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        return spriteRenderer != null && spriteRenderer.flipX ? -1 : 1;
    }
}

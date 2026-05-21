using UnityEngine;

public class CollectableCoin : MonoBehaviour
{
    [SerializeField] private string coinId;

    public string CoinId => coinId;

    private void Reset()
    {
        if (string.IsNullOrWhiteSpace(coinId))
        {
            coinId = gameObject.name;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(coinId))
        {
            coinId = gameObject.name;
        }
    }
}

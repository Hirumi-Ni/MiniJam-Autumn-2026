using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int TotalCurrency { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        GameEventHandler.OnLeafCollected += AddCurrency;
    }

    private void OnDisable()
    {
        GameEventHandler.OnLeafCollected -= AddCurrency;
    }

    public void AddCurrency(int amount)
    {
        TotalCurrency += amount;
        GameEventHandler.OnCurrencyChange?.Invoke(TotalCurrency);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (TotalCurrency >= amount)
        {
            TotalCurrency -= amount;
            GameEventHandler.OnCurrencyChange?.Invoke(TotalCurrency);
            return true;
        }
        return false;
    }
}
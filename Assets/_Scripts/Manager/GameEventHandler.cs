using System;

public static class GameEventHandler
{
    public static Action<int> OnCurrencyChange;
    public static Action<int> OnLeafCollected;
    public static Action<UpgradeSO, int> OnUpgradePurchased;
}
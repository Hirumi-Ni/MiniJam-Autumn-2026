using System;
using UnityEngine;

public static class GameEventHandler
{
    public static Action<int, Vector3> OnLeafCollected;
    public static Action<int, Vector3> OnCoinsEarned;
    public static Action<int> OnCurrencyChange;
    public static Action<UpgradeSO, int> OnUpgradePurchased;
}

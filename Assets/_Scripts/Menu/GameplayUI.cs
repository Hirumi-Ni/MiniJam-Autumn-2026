using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    #region [References]
    [SerializeField] private TMP_Text currencyCounter;
    #endregion

    #region [Attribute]
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        GameEventHandler.OnCurrencyChange += ChangeCurrencyText;
    }

    private void OnDisable()
    {
        GameEventHandler.OnCurrencyChange -= ChangeCurrencyText;
    }

    private void Start()
    {
        currencyCounter.text = 0.ToString();
    }
    #endregion

    #region [Methods]
    private void ChangeCurrencyText(int amount)
    {
        currencyCounter.text = amount.ToString();
    }
    #endregion
}

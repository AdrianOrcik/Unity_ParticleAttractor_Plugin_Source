using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    public Transform Coins;
    public Text TextAmount;
    public int BaseAmount;
    public int FinalAmount;
    
    void Start()
    {
        SetCoinsText(BaseAmount);
    }

    void SetCoinsText(int amount)
    {
        TextAmount.text = amount.ToString();
    }

    public void OnFirstCoinCounted()
    {
        StartCoroutine(CoinsAmountUpdate());
    }

    IEnumerator CoinsAmountUpdate()
    {
        while (BaseAmount < FinalAmount)
        {
            BaseAmount += 10;
            SetCoinsText(BaseAmount);
            yield return new WaitForSeconds(0.05f);
        }
    }
}

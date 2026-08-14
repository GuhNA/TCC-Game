using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField]
    private int startingMoney = 1000;

    public int Money { get; private set; }

    public event Action<int> MoneyChanged;

    private void Awake()
    {
        CitySaveData saveData = CitySaveRepository.Load();
        Money = saveData.hasMoney ? saveData.money : startingMoney;
    }

    private void Start()
    {
        MoneyChanged?.Invoke(Money);
    }

    public bool CanAfford(int amount)
    {
        return Money >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!CanAfford(amount))
        {
            return false;
        }

        Money -= amount;
        CitySaveRepository.SaveMoney(Money);
        MoneyChanged?.Invoke(Money);

        return true;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Money += amount;
        CitySaveRepository.SaveMoney(Money);
        MoneyChanged?.Invoke(Money);
    }
}

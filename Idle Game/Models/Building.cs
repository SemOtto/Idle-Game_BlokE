using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Idle_Game.Models;

public class Building : INotifyPropertyChanged
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public double BasePrice { get; set; }
    public double IncomePerSecond { get; set; }

    private int _amount;
    private double _currentPrice;
    private double _currentIncome;
    private bool _canBuy;

    public int Amount
    {
        get => _amount;
        set
        {
            if (_amount == value)
                return;

            _amount = value;
            OnPropertyChanged();
        }
    }

    public double CurrentPrice
    {
        get => _currentPrice;
        set
        {
            if (_currentPrice == value)
                return;

            _currentPrice = value;
            OnPropertyChanged();
        }
    }

    public double CurrentIncome
    {
        get => _currentIncome;
        set
        {
            if (_currentIncome == value)
                return;

            _currentIncome = value;
            OnPropertyChanged();
        }
    }

    public bool CanBuy
    {
        get => _canBuy;
        set
        {
            if (_canBuy == value)
                return;

            _canBuy = value;
            OnPropertyChanged();
        }
    }

    public Building()
    {
    }

    public Building(
        string id,
        string name,
        string description,
        double basePrice,
        double incomePerSecond)
    {
        Id = id;
        Name = name;
        Description = description;
        BasePrice = basePrice;
        IncomePerSecond = incomePerSecond;
    }

    public double GetPrice(int amount)
    {
        return BasePrice * Math.Pow(1.15, amount);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
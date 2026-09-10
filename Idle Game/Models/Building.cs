using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idle_Game.Models;

public class Building
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public double BasePrice { get; set; }

    public double IncomePerSecond { get; set; }

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
}

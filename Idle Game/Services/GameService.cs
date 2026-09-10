using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Idle_Game.Models;

namespace Idle_Game.Services;

public class GameService
{
    public PlayerData Player { get; private set; }

    public List<Building> Buildings { get; }

    public List<Upgrade> Upgrades { get; }

    public GameService()
    {
        SaveService saveService = new SaveService();

        Player = saveService.Load();

        Buildings = new List<Building>
        {
            new Building(
                "robot",
                "Mijnrobot",
                "Een robot die automatisch grondstoffen verzamelt.",
                50,
                1),

            new Building(
                "station",
                "Mining Station",
                "Een klein station voor grootschalige mijnbouw.",
                500,
                10),

            new Building(
                "spaceship",
                "Ruimteschip",
                "Een schip dat grondstoffen uit de ruimte haalt.",
                5_000,
                75),

            new Building(
                "spacestation",
                "Ruimtestation",
                "Een groot station voor ruimtehandel.",
                50_000,
                500),

            new Building(
                "colony",
                "Planeetkolonie",
                "Een volledig geautomatiseerde kolonie.",
                500_000,
                5_000),

            new Building(
                "starsystem",
                "Sterrensysteem",
                "Een volledig systeem dat enorme hoeveelheden credits produceert.",
                10_000_000,
                100_000)
        };

        Upgrades = new List<Upgrade>
        {
            new Upgrade(
                "better_drill",
                "Betere mijnboor",
                "Je handmatige mijnactie geeft +10 credits.",
                250),

            new Upgrade(
                "stronger_robots",
                "Sterkere robots",
                "Mijnrobots produceren 2x zoveel.",
                2_500),

            new Upgrade(
                "better_engines",
                "Betere motoren",
                "Ruimteschepen produceren 2x zoveel.",
                10_000),

            new Upgrade(
                "advanced_technology",
                "Geavanceerde technologie",
                "Alle automatische inkomsten worden 25% hoger.",
                100_000)
        };
    }

    public void AddManualCredits()
    {
        Player.Credits += Player.ManualPower;
    }

    public double GetIncomePerSecond()
    {
        double income = 0;

        foreach (Building building in Buildings)
        {
            int amount = GetBuildingAmount(building.Id);

            double buildingIncome =
                amount * building.IncomePerSecond;

            if (
                building.Id == "robot" &&
                HasUpgrade("stronger_robots"))
            {
                buildingIncome *= 2;
            }

            if (
                building.Id == "spaceship" &&
                HasUpgrade("better_engines"))
            {
                buildingIncome *= 2;
            }

            income += buildingIncome;
        }

        if (HasUpgrade("advanced_technology"))
        {
            income *= 1.25;
        }

        income *= 1 + Player.GalaxyPoints * 0.10;

        return income;
    }

    public int GetBuildingAmount(string buildingId)
    {
        if (Player.Buildings.TryGetValue(
            buildingId,
            out int amount))
        {
            return amount;
        }

        return 0;
    }

    public double GetBuildingPrice(Building building)
    {
        int amount = GetBuildingAmount(building.Id);

        return building.GetPrice(amount);
    }

    public bool CanBuyBuilding(Building building)
    {
        return Player.Credits >= GetBuildingPrice(building);
    }

    public bool BuyBuilding(Building building)
    {
        double price = GetBuildingPrice(building);

        if (Player.Credits < price)
        {
            return false;
        }

        Player.Credits -= price;

        int currentAmount =
            GetBuildingAmount(building.Id);

        Player.Buildings[building.Id] =
            currentAmount + 1;

        return true;
    }

    public bool HasUpgrade(string upgradeId)
    {
        return Player.PurchasedUpgrades.Contains(upgradeId);
    }

    public bool CanBuyUpgrade(Upgrade upgrade)
    {
        return !HasUpgrade(upgrade.Id)
            && Player.Credits >= upgrade.Price;
    }

    public bool BuyUpgrade(Upgrade upgrade)
    {
        if (!CanBuyUpgrade(upgrade))
        {
            return false;
        }

        Player.Credits -= upgrade.Price;

        Player.PurchasedUpgrades.Add(upgrade.Id);

        if (upgrade.Id == "better_drill")
        {
            Player.ManualPower += 10;
        }

        return true;
    }

    public double CalculateOfflineIncome()
    {
        DateTime now = DateTime.Now;

        TimeSpan offlineTime =
            now - Player.LastSaved;

        // Maximaal 8 uur offline inkomsten.
        if (offlineTime.TotalHours > 8)
        {
            offlineTime = TimeSpan.FromHours(8);
        }

        if (offlineTime.TotalSeconds < 0)
        {
            return 0;
        }

        double incomePerSecond =
            GetIncomePerSecond();

        return incomePerSecond *
               offlineTime.TotalSeconds;
    }

    public TimeSpan GetOfflineTime()
    {
        TimeSpan time =
            DateTime.Now - Player.LastSaved;

        if (time.TotalHours > 8)
        {
            return TimeSpan.FromHours(8);
        }

        if (time.TotalSeconds < 0)
        {
            return TimeSpan.Zero;
        }

        return time;
    }

    public void ApplyOfflineIncome()
    {
        double offlineIncome =
            CalculateOfflineIncome();

        Player.Credits += offlineIncome;
    }

    public bool CanPrestige()
    {
        return Player.Credits >= 10_000_000;
    }

    public bool Prestige()
    {
        if (!CanPrestige())
        {
            return false;
        }

        Player.Credits = 0;

        Player.ManualPower = 10;

        Player.Buildings.Clear();

        Player.PurchasedUpgrades.Clear();

        Player.GalaxyPoints++;

        return true;
    }
}

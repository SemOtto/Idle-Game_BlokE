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

        Player.Buildings ??= new Dictionary<string, int>();
        Player.UpgradeCounts ??= new Dictionary<string, int>();
        Player.PurchasedUpgrades ??= new List<string>();

        foreach (string id in Player.PurchasedUpgrades)
        {
            if (!Player.UpgradeCounts.ContainsKey(id))
            {
                Player.UpgradeCounts[id] = 1;
            }
        }

        Buildings = new List<Building>
        {
            new Building("robot", "Mijnrobot",
                "Een robot die automatisch grondstoffen verzamelt.",
                50, 1),

            new Building("station", "Mining Station",
                "Een klein station voor grootschalige mijnbouw.",
                250, 10),

            new Building("spaceship", "Ruimteschip",
                "Een schip dat grondstoffen uit de ruimte haalt.",
                2500, 75),

            new Building("spacestation", "Ruimtestation",
                "Een groot station voor ruimtehandel.",
                25000, 500),

            new Building("colony", "Planeetkolonie",
                "Een volledig geautomatiseerde kolonie.",
                250000, 5000),

            new Building("starsystem", "Sterrensysteem",
                "Een volledig systeem dat enorme hoeveelheden credits produceert.",
                5000000, 100000)
        };

        Upgrades = new List<Upgrade>
        {
            new Upgrade("better_drill",
                "Betere mijnboor",
                "Je handmatige mijnactie geeft +10 credits per aankoop.",
                250),

            new Upgrade("stronger_robots",
                "Sterkere robots",
                "Mijnrobots produceren 2x zoveel per aankoop.",
                1000),

            new Upgrade("better_engines",
                "Betere motoren",
                "Ruimteschepen produceren 2x zoveel per aankoop.",
                5000),

            new Upgrade("advanced_technology",
                "Geavanceerde technologie",
                "Alle automatische inkomsten worden 25% hoger per aankoop.",
                25000)
        };

        UpdateDisplays();
    }

    public void AddManualCredits()
    {
        Player.Credits += Player.ManualPower;
    }

    public int GetBuildingAmount(string buildingId)
    {
        return Player.Buildings.TryGetValue(buildingId, out int amount)
            ? amount : 0;
    }

    public int GetUpgradeAmount(string upgradeId)
    {
        return Player.UpgradeCounts.TryGetValue(upgradeId, out int amount)
            ? amount : 0;
    }

    public bool HasUpgrade(string upgradeId)
    {
        return GetUpgradeAmount(upgradeId) > 0;
    }

    public double GetBuildingPrice(Building building)
    {
        return building.GetPrice(GetBuildingAmount(building.Id));
    }

    public double GetUpgradePrice(Upgrade upgrade)
    {
        return upgrade.GetPrice(GetUpgradeAmount(upgrade.Id));
    }

    public bool CanBuyBuilding(Building building)
    {
        return Player.Credits >= GetBuildingPrice(building);
    }

    public bool CanBuyUpgrade(Upgrade upgrade)
    {
        return Player.Credits >= GetUpgradePrice(upgrade);
    }

    public bool BuyBuilding(Building building)
    {
        double price = GetBuildingPrice(building);

        if (Player.Credits < price)
            return false;

        Player.Credits -= price;

        Player.Buildings[building.Id] =
            GetBuildingAmount(building.Id) + 1;

        UpdateDisplays();
        return true;
    }

    public bool BuyUpgrade(Upgrade upgrade)
    {
        double price = GetUpgradePrice(upgrade);

        if (Player.Credits < price)
            return false;

        Player.Credits -= price;

        Player.UpgradeCounts[upgrade.Id] =
            GetUpgradeAmount(upgrade.Id) + 1;

        if (!Player.PurchasedUpgrades.Contains(upgrade.Id))
            Player.PurchasedUpgrades.Add(upgrade.Id);

        if (upgrade.Id == "better_drill")
            Player.ManualPower += 10;

        UpdateDisplays();
        return true;
    }

    public double GetIncomePerSecond()
    {
        double income = 0;

        foreach (Building building in Buildings)
        {
            int amount = GetBuildingAmount(building.Id);
            double buildingIncome = amount * building.IncomePerSecond;

            if (building.Id == "robot")
                buildingIncome *= Math.Pow(2, GetUpgradeAmount("stronger_robots"));

            if (building.Id == "spaceship")
                buildingIncome *= Math.Pow(2, GetUpgradeAmount("better_engines"));

            income += buildingIncome;
        }

        income *= Math.Pow(1.25, GetUpgradeAmount("advanced_technology"));
        income *= 1 + Player.GalaxyPoints * 0.10;

        return income;
    }

    public void UpdateDisplays()
    {
        foreach (Building building in Buildings)
        {
            building.Amount = GetBuildingAmount(building.Id);
            building.CurrentPrice = GetBuildingPrice(building);
            building.CurrentIncome =
                building.IncomePerSecond * building.Amount;
            building.CanBuy = CanBuyBuilding(building);
        }

        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.Amount = GetUpgradeAmount(upgrade.Id);
            upgrade.CurrentPrice = GetUpgradePrice(upgrade);
            upgrade.CanBuy = CanBuyUpgrade(upgrade);
        }
    }

    public double CalculateOfflineIncome()
    {
        TimeSpan offlineTime = GetOfflineTime();
        return GetIncomePerSecond() * offlineTime.TotalSeconds;
    }

    public TimeSpan GetOfflineTime()
    {
        TimeSpan time = DateTime.Now - Player.LastSaved;

        if (time.TotalHours > 8)
            return TimeSpan.FromHours(8);

        if (time.TotalSeconds < 0)
            return TimeSpan.Zero;

        return time;
    }

    public void ApplyOfflineIncome()
    {
        Player.Credits += CalculateOfflineIncome();
    }

    public bool CanPrestige()
    {
        return Player.Credits >= 10_000_000;
    }

    public bool Prestige()
    {
        if (!CanPrestige())
            return false;

        Player.Credits = 0;
        Player.ManualPower = 10;
        Player.Buildings.Clear();
        Player.PurchasedUpgrades.Clear();
        Player.UpgradeCounts.Clear();
        Player.GalaxyPoints++;

        UpdateDisplays();
        return true;
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using Idle_Game.Models;
using Idle_Game.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Idle_Game.Models;
using Idle_Game.Services;

namespace Idle_Game;

public sealed partial class MainWindow : Window
{
    private readonly GameService _game;
    private readonly SaveService _saveService;

    private readonly DispatcherTimer _gameTimer;
    private readonly DispatcherTimer _saveTimer;

    private DateTime _lastUpdate;

    public MainWindow()
    {
        this.InitializeComponent();

        _game = new GameService();

        _saveService = new SaveService();

        _lastUpdate = DateTime.Now;

        _gameTimer = new DispatcherTimer();

        _gameTimer.Interval =
            TimeSpan.FromMilliseconds(100);

        _gameTimer.Tick += GameTimer_Tick;

        _saveTimer = new DispatcherTimer();

        _saveTimer.Interval =
            TimeSpan.FromSeconds(30);

        _saveTimer.Tick += SaveTimer_Tick;

        Loaded += MainWindow_Loaded;

        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (_saveService.SaveExists())
        {
            double offlineIncome =
                _game.CalculateOfflineIncome();

            TimeSpan offlineTime =
                _game.GetOfflineTime();

            if (offlineIncome > 0)
            {
                _game.ApplyOfflineIncome();

                AddLog(
                    $"🌙 Welkom terug! Je hebt {offlineIncome:N0} credits verdiend tijdens je afwezigheid ({offlineTime.Hours}u {offlineTime.Minutes}m).");
            }
        }

        BuildingsList.ItemsSource =
            _game.Buildings;

        UpgradesList.ItemsSource =
            _game.Upgrades;

        UpdateUI();

        _gameTimer.Start();
        _saveTimer.Start();
    }

    private void GameTimer_Tick(
        object? sender,
        object e)
    {
        DateTime now = DateTime.Now;

        double seconds =
            (now - _lastUpdate).TotalSeconds;

        _lastUpdate = now;

        double income =
            _game.GetIncomePerSecond();

        _game.Player.Credits +=
            income * seconds;

        UpdateUI();
    }

    private void SaveTimer_Tick(
        object? sender,
        object e)
    {
        SaveGame();

        AddLog("💾 Automatisch opgeslagen.");
    }

    private void UpdateUI()
    {
        CreditsText.Text =
            $"{_game.Player.Credits:N0}";

        IncomeText.Text =
            $"+{_game.GetIncomePerSecond():N1} / sec";

        ManualPowerText.Text =
            $"+{_game.Player.ManualPower:N0} per klik";

        GalaxyPointsText.Text =
            $"⭐ Galaxy Points: {_game.Player.GalaxyPoints}";

        UpdateBuildingButtons();

        UpdateUpgradeButtons();

        PrestigeButton.IsEnabled =
            _game.CanPrestige();
    }

    private void UpdateBuildingButtons()
    {
        BuildingsList.ItemsSource = null;

        BuildingsList.ItemsSource =
            _game.Buildings;
    }

    private void UpdateUpgradeButtons()
    {
        UpgradesList.ItemsSource = null;

        UpgradesList.ItemsSource =
            _game.Upgrades;
    }

    private void MineButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _game.AddManualCredits();

        AddLog(
            $"⛏️ Je hebt {_game.Player.ManualPower:N0} credits gemined.");

        UpdateUI();
    }

    private void BuyBuilding_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not Building building)
        {
            return;
        }

        double price =
            _game.GetBuildingPrice(building);

        if (!_game.BuyBuilding(building))
        {
            AddLog(
                $"❌ Niet genoeg credits voor {building.Name}. Nodig: {price:N0}");

            return;
        }

        AddLog(
            $"🏗️ {building.Name} gekocht voor {price:N0} credits.");

        UpdateUI();
    }

    private void BuyUpgrade_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not Upgrade upgrade)
        {
            return;
        }

        if (_game.HasUpgrade(upgrade.Id))
        {
            AddLog(
                $"❌ {upgrade.Name} is al gekocht.");

            return;
        }

        if (!_game.BuyUpgrade(upgrade))
        {
            AddLog(
                $"❌ Niet genoeg credits voor {upgrade.Name}.");

            return;
        }

        AddLog(
            $"⬆️ Upgrade gekocht: {upgrade.Name}");

        UpdateUI();
    }

    private async void PrestigeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!_game.CanPrestige())
        {
            return;
        }

        ContentDialog dialog = new ContentDialog
        {
            Title = "🌌 Nieuwe Melkweg",
            Content =
                "Weet je zeker dat je een nieuwe Melkweg wilt starten?\n\n" +
                "Je verliest je credits, gebouwen en upgrades.\n\n" +
                "Je krijgt 1 Galaxy Point en daarmee +10% permanente productie.",
            PrimaryButtonText = "Nieuwe Melkweg",
            CloseButtonText = "Annuleren",
            XamlRoot = Content.XamlRoot
        };

        ContentDialogResult result =
            await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (_game.Prestige())
        {
            AddLog(
                "🌌 Nieuwe Melkweg gestart! +1 Galaxy Point.");

            SaveGame();

            UpdateUI();
        }
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SaveGame();

        AddLog("💾 Game handmatig opgeslagen.");
    }

    private void SaveGame()
    {
        try
        {
            _saveService.Save(_game.Player);
        }
        catch (Exception ex)
        {
            AddLog(
                $"❌ Opslaan mislukt: {ex.Message}");
        }
    }

    private void AddLog(string message)
    {
        LogList.Items.Insert(
            0,
            $"{DateTime.Now:HH:mm:ss} - {message}");

        while (LogList.Items.Count > 30)
        {
            LogList.Items.RemoveAt(
                LogList.Items.Count - 1);
        }
    }

    private void MainWindow_Closed(
        object sender,
        WindowEventArgs args)
    {
        _gameTimer.Stop();
        _saveTimer.Stop();

        SaveGame();
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Idle_Game.Models;

namespace Idle_Game.Services;

public class SaveService
{
    private readonly string _savePath;

    public SaveService()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "SpaceTycoon");

        Directory.CreateDirectory(folder);

        _savePath = Path.Combine(folder, "save.json");
    }

    public void Save(PlayerData player)
    {
        player.LastSaved = DateTime.Now;

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(player, options);

        File.WriteAllText(_savePath, json);
    }

    public PlayerData Load()
    {
        try
        {
            if (!File.Exists(_savePath))
            {
                return new PlayerData();
            }

            string json = File.ReadAllText(_savePath);

            PlayerData? player =
                JsonSerializer.Deserialize<PlayerData>(json);

            if (player == null)
            {
                return new PlayerData();
            }

            player.Buildings ??= new Dictionary<string, int>();
            player.PurchasedUpgrades ??= new List<string>();

            return player;
        }
        catch
        {
            return new PlayerData();
        }
    }

    public bool SaveExists()
    {
        return File.Exists(_savePath);
    }
}

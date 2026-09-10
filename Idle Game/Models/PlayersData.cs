using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idle_Game.Models;
public class PlayerData
{
    public double Credits { get; set; } = 0;

    public double ManualPower { get; set; } = 10;

    public int GalaxyPoints { get; set; } = 0;

    public DateTime LastSaved { get; set; } = DateTime.Now;

    public Dictionary<string, int> Buildings { get; set; } = new();

    public List<string> PurchasedUpgrades { get; set; } = new();
}

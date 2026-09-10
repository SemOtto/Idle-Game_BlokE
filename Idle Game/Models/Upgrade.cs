using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idle_Game.Models;

public class Upgrade
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public double Price { get; set; }

    public Upgrade()
    {
    }

    public Upgrade(
        string id,
        string name,
        string description,
        double price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
}
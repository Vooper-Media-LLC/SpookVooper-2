using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Stocks;

public class StockObject : Entity
{
    // Owner of this stock object
    public string Owner_Id { get; set; }
    public string Ticker { get; set;}

    public int Amount { get; set;}
}
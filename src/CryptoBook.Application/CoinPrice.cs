using System.Text;

namespace CryptoBook.Application;

public class CoinPrice
{
    public string Id { get; init; }
    public IEnumerable<Price> Prices { get; init; }

    public override string ToString()
    {
        var suitableForDisplay = new StringBuilder(Id);
        suitableForDisplay.Append('\n');
        foreach (var price in Prices)
        {
            suitableForDisplay.Append(price.Currency).Append('=').Append(price.Value);
            suitableForDisplay.Append(' ');
        }
        
        return suitableForDisplay.ToString().TrimEnd();
    }
}
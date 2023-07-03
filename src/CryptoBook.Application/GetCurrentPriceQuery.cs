namespace CryptoBook.Application;

public class GetCurrentPriceQuery
{
    public IEnumerable<string> Coins { get; set; }
    public IEnumerable<string> Currencies { get; set; }
}
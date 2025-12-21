using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class AuctionModel : PageModel
{
    private readonly AuctionDb _db;
    public AuctionModel(AuctionDb db) => _db = db;
    
    public Auction? Auction { get; set; }
    public int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    public string UserName => HttpContext.Session.GetString("UserName") ?? "";
    public decimal UserWallet { get; set; }  // ← AGGIUNGI QUESTA
    public string? ErrorMessage { get; set; }  // ← AGGIUNGI QUESTA
    
    [BindProperty] public decimal Amount { get; set; }

    public async Task OnGetAsync(int id)
    {
        Auction = await _db.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id);

        // Carica wallet
        var user = await _db.Users.FindAsync(UserId);
        UserWallet = user?.Wallet ?? 0;
    }

    public async Task<IActionResult> OnPostAsync(int id)
{
    var user = await _db.Users.FindAsync(UserId);
    var auction = await _db.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id);
    
    if (auction == null || user == null)
        return RedirectToPage();

    // CONTROLLO 1: L'offerta deve essere superiore a quella corrente
    if (Amount <= auction.CurrentPrice)
    {
        ErrorMessage = "L'offerta deve essere superiore all'offerta corrente!";
        Auction = auction;
        UserWallet = user.Wallet;
        return Page();
    }

    // CONTROLLO 2: Controlla se ha abbastanza soldi
    if (Amount > user.Wallet)
    {
        ErrorMessage = $"Fondi insufficienti! Hai solo €{user.Wallet} nel portafoglio.";
        Auction = auction;
        UserWallet = user.Wallet;
        return Page();
    }

    // Trova l'offerta precedente di questo utente per questa asta
    var previousBid = auction.Bids
        .Where(b => b.UserId == UserId)
        .OrderByDescending(b => b.Amount)
        .FirstOrDefault();

    // Se aveva già fatto un'offerta, restituisci quei soldi
    if (previousBid != null)
    {
        user.Wallet += previousBid.Amount;
    }

    // Sottrai la nuova offerta dal wallet
    user.Wallet -= Amount;

    // Crea la nuova offerta
    var bid = new Bid
    {
        AuctionId = id,
        UserId = UserId,
        UserName = UserName,
        Amount = Amount
    };
    
    auction.CurrentPrice = Amount;
    _db.Bids.Add(bid);
    await _db.SaveChangesAsync();
    
    return RedirectToPage(new { id });
}
}
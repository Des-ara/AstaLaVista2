using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class IndexModel : PageModel
{
    private readonly AuctionDb _db;
    public IndexModel(AuctionDb db) => _db = db;
    
    public List<Auction> Auctions { get; set; } = new();
    public string UserName => HttpContext.Session.GetString("UserName") ?? "";
    public int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;
    public decimal UserWallet { get; set; }

    public async Task OnGetAsync()
    {
        if (UserId == 0) { Response.Redirect("/Login"); return; }

     // Carica wallet utente
        var user = await _db.Users.FindAsync(UserId);
        UserWallet = user?.Wallet ?? 0;

        Auctions = await _db.Auctions.Include(a => a.Bids).Where(a => a.Active).ToListAsync();
    }
}
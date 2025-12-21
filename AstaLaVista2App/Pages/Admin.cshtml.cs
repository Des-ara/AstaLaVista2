using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class AdminModel : PageModel
{
    private readonly AuctionDb _db;
    public AdminModel(AuctionDb db) => _db = db;
    
    public List<Auction> Auctions { get; set; } = new();
    public List<User> Users { get; set; } = new();
    
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string Description { get; set; } = "";
    [BindProperty] public decimal StartPrice { get; set; }
    [BindProperty] public int Days { get; set; } = 7;
    [BindProperty] public string? ImageUrl { get; set; } = "";

    public async Task OnGetAsync()
    {
        // Verifica se è admin
    if (HttpContext.Session.GetString("IsAdmin") != "True")
    {
        Response.Redirect("/Index");
        return;
    }

        Auctions = await _db.Auctions.Include(a => a.Bids).OrderByDescending(a => a.Id).ToListAsync();
        Users = await _db.Users.OrderBy(u => u.Name).ToListAsync(); 
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (HttpContext.Session.GetString("IsAdmin") != "True")
        return RedirectToPage("/Index");

        var auction = new Auction
        {
            Title = Title,
            Description = Description,
            StartPrice = StartPrice,
            CurrentPrice = StartPrice,
            EndDate = DateTime.Now.AddDays(Days),
            ImageUrl = ImageUrl
        };
        _db.Auctions.Add(auction);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "True")
        return RedirectToPage("/Index");

        var auction = await _db.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id);
        if (auction != null)
        {
            _db.Bids.RemoveRange(auction.Bids);
            _db.Auctions.Remove(auction);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCloseAsync(int id)
    {
        if (HttpContext.Session.GetString("IsAdmin") != "True")
        return RedirectToPage("/Index");

        var auction = await _db.Auctions.FindAsync(id);
        if (auction != null)
        {
            auction.Active = false;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
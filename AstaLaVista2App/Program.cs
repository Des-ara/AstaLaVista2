using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSession();  // ← AGGIUNTO PER LE SESSIONI
builder.Services.AddDbContext<AuctionDb>(opt => opt.UseSqlite("Data Source=auctions.db"));

var app = builder.Build();

// Crea DB se non esiste
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDb>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseSession();  // ← AGGIUNTO PER LE SESSIONI
app.UseRouting();
app.MapRazorPages();

app.Run();

// ==================== Database ====================
public class AuctionDb : DbContext
{
    public AuctionDb(DbContextOptions<AuctionDb> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsAdmin { get; set; } = false;
    public decimal Wallet { get; set; } = 3700m;
}

public class Auction
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal StartPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime EndDate { get; set; }
    public bool Active { get; set; } = true;
    public string? ImageUrl { get; set; } = "";
    public List<Bid> Bids { get; set; } = new();
    
}

public class Bid
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}

// ==================== Helper ====================
public static class Helper
{
    public static string Hash(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
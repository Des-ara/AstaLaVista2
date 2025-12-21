using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSession();  // ← AGGIUNTO PER LE SESSIONI
// Configura Data Protection per Fly.io
var dataProtectionPath = "/data/keys";

// Prova a creare la directory, se fallisce usa una temp
try
{
    if (!Directory.Exists("/data"))
    {
        Directory.CreateDirectory("/data");
    }
    if (!Directory.Exists(dataProtectionPath))
    {
        Directory.CreateDirectory(dataProtectionPath);
    }
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("AstaLaVista2App");
}
catch (Exception ex)
{
    // Se /data non è scrivibile, usa directory temporanea
    Console.WriteLine($"Warning: Cannot write to /data, using temp directory: {ex.Message}");
    var tempPath = Path.Combine(Path.GetTempPath(), "dataprotection-keys");
    Directory.CreateDirectory(tempPath);
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(tempPath))
        .SetApplicationName("AstaLaVista2App");
}

// Database con percorso persistente
var dbPath = Path.Combine("/data", "auctions.db");
builder.Services.AddDbContext<AuctionDb>(opt => 
    opt.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

// Crea DB se non esiste
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDb>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.MapRazorPages();

// Configura la porta per Fly.io
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

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
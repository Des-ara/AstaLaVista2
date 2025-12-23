using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly AuctionDb _db;
    public LoginModel(AuctionDb db) => _db = db;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
{
    try
    {
        Console.WriteLine($"=== LOGIN START === Name: {Name}");
        
        var hash = Helper.Hash(Password);
        Console.WriteLine("Password hashed OK");
        
        // Cerca l'utente ignorando maiuscole/minuscole
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Name.ToLower() == Name.ToLower() && u.Password == hash);
        
        Console.WriteLine($"User found by credentials: {user != null}");
        
        if (user == null)
        {
            // Controlla se esiste già un utente con quel nome (case-insensitive)
            var existingUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Name.ToLower() == Name.ToLower());
            
            Console.WriteLine($"Existing user check: {existingUser != null}");
            
            if (existingUser != null)
            {
                // Utente esiste ma password sbagliata
                Error = "Password errata!";
                Console.WriteLine("Wrong password - returning Page");
                return Page();
            }
            
            // Controlla se è il primo utente
            var isFirstUser = !await _db.Users.AnyAsync();
            Console.WriteLine($"Is first user: {isFirstUser}");
            
            // Crea nuovo utente
            user = new User 
            { 
                Name = CapitalizeFirstLetter(Name),
                Password = hash,
                IsAdmin = isFirstUser,
                Wallet = 3700m
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            Console.WriteLine($"User created with ID: {user.Id}");
        }
        
        Console.WriteLine($"Setting session for user {user.Id}");
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
        await HttpContext.Session.CommitAsync();
        
        Console.WriteLine("Session set, redirecting to /Index");
        return RedirectToPage("/Index");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== LOGIN ERROR === {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        Error = "Errore durante il login";
        return Page();
    }
}

// Metodo helper per capitalizzare la prima lettera
private string CapitalizeFirstLetter(string text)
{
    if (string.IsNullOrEmpty(text))
        return text;
    
    return char.ToUpper(text[0]) + text.Substring(1).ToLower();
}
}
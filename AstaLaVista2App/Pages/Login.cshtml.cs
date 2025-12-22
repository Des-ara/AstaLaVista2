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
    var hash = Helper.Hash(Password);
    
    // Cerca l'utente ignorando maiuscole/minuscole
    var user = await _db.Users
        .FirstOrDefaultAsync(u => u.Name.ToLower() == Name.ToLower() && u.Password == hash);
    
    if (user == null)
    {
        // Controlla se esiste già un utente con quel nome (case-insensitive)
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Name.ToLower() == Name.ToLower());
        
        if (existingUser != null)
        {
            // Utente esiste ma password sbagliata
            Error = "Password errata!";
            return Page();
        }
        
        // Controlla se è il primo utente
        var isFirstUser = !await _db.Users.AnyAsync();
        
        // Crea nuovo utente - salva il nome con la prima lettera maiuscola
        user = new User 
        { 
            Name = CapitalizeFirstLetter(Name),  // Standardizza il nome
            Password = hash,
            IsAdmin = isFirstUser,
            Wallet = 3700m
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
    
    HttpContext.Session.SetInt32("UserId", user.Id);
    HttpContext.Session.SetString("UserName", user.Name);
    HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
    return RedirectToPage("/Index");
}

// Metodo helper per capitalizzare la prima lettera
private string CapitalizeFirstLetter(string text)
{
    if (string.IsNullOrEmpty(text))
        return text;
    
    return char.ToUpper(text[0]) + text.Substring(1).ToLower();
}
}
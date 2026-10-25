using BookSeeder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a command.");
            return;
        }

        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddUserSecrets<Program>();
        var contactEmail = builder.Configuration["OpenLibrary:ContactEmail"];
        builder.Services.AddHttpClient("OpenLibrary", client =>
        {
            client.BaseAddress = new Uri("https://openlibrary.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"BookSeederApp/1.0 ({contactEmail})");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        using var host = builder.Build();

        string command = args[0].ToLower();

        if (command == "pull")
        {
            var clientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
            await PullBooksAsync(clientFactory);
        }
        else if (command == "seed")
        {
            var adminEmail = builder.Configuration["Seed:AdminEmail"] ?? string.Empty;
            var adminPassword = builder.Configuration["Seed:AdminPassword"] ?? string.Empty;
            await Seed(adminEmail, adminPassword);
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}");
        }
    }

    private static async Task PullBooksAsync(IHttpClientFactory clientFactory)
    {
        Console.WriteLine("Fetching books from Open Library...");

        var client = clientFactory.CreateClient("OpenLibrary");
        var genres = new Dictionary<string, (string Query, GenreType Genre)>
        {
            // { "Fantasy", ("subject:fantasy", GenreType.Fantasy) },
            // { "Romance", ("subject:romance", GenreType.Romance) },
            // { "Mystery", ("subject:mystery", GenreType.Mystery) },
            // { "Thriller", ("subject:thriller", GenreType.Thriller) },
            // { "Sci-Fi", ("subject:science_fiction", GenreType.ScienceFiction) },
            { "Manga", ("subject:manga", GenreType.Manga) },
            // { "Non-Fiction", ("subject:nonfiction", GenreType.NonFiction) }
        };

        var allDocs = new List<OpenLibraryBook>();
        foreach (var genre in genres)
        {
            Console.WriteLine($"Fetching 300 {genre.Key} books...");
            var url = $"search.json?q={genre.Value.Query}&limit=300&fields=title,author_name,isbn";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch {genre.Key}. Status: {response.StatusCode}. Skipping...");
                continue; // Move to the next genre if one fails
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserialize the response into our OpenLibraryResponse model
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OpenLibraryResponse>(jsonString, options);

            if (result?.Docs != null)
            {
                foreach (var doc in result.Docs)
                {
                    doc.Genre = genre.Value.Genre;
                }

                allDocs.AddRange(result.Docs);
                Console.WriteLine($"-> Successfully added {result.Docs.Count} {genre.Key} books.");
            }

            // Be polite to Open Library's servers: wait 1.5 seconds between requests
            await Task.Delay(1500);
        }

        allDocs.RemoveAt(123);
        // Package all collected books back into our response structure
        var allBooks = new OpenLibraryResponse { Docs = allDocs };

        // Save the combined list into seed_books.json
        string finalJson = JsonSerializer.Serialize(allBooks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("seed_books.json", finalJson);

        Console.WriteLine($"\nSuccessfully saved a total of {allDocs.Count} books across all genres to seed_books.json!");
    }

    private static async Task Seed(string adminEmail, string adminPassword)
    {
        var url = "https://localhost:7213/api/Book/bulk";
        if (!File.Exists("seed_books.json"))
        {
            Console.WriteLine("Error: 'seed_books.json' not found! Run 'dotnet run -- pull' first.");
            return;
        }

        Console.WriteLine($"Reading books from seed_books.json...");
        var jsonContent = await File.ReadAllTextAsync("seed_books.json");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<OpenLibraryResponse>(jsonContent, options);

        if (data?.Docs == null || data.Docs.Count == 0)
        {
            Console.WriteLine("No books found in seed_books.json.");
            return;
        }

        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        var loginSuccess = await LoginAsync(client, adminEmail, adminPassword);
        if (!loginSuccess)
        {
            return;
        }

        var existing = await GetExistingIsbnsAsync(client);
        if (existing == null)
        {
            Console.WriteLine("Stopping to avoid duplicates.");
            return;
        }

        Console.WriteLine($"Found {existing.Count} books already in the database.");

        var validBooks = data.Docs
            .Select(doc => (doc, isbn: PickIsbn13(doc.Isbns)))
            .Where(x => !string.IsNullOrWhiteSpace(x.doc.Title) && x.doc.Authors?.Count > 0 && x.isbn != null)
            .GroupBy(x => x.isbn!)
            .Select(g => new CreateBookDto
            {
                Name = g.First().doc.Title!,
                Author = g.First().doc.Authors![0],
                Isbn = g.Key,
                Cost = 20,
                Stock = 10,
                GenreIds = g.Select(x => x.doc.Genre).Distinct().ToHashSet()
            })
            .Where(b => !existing.Contains(b.Isbn))
            .ToList();

        Console.WriteLine($"Prepared {validBooks.Count} valid books for seeding.");

        try
        {
            var response = await client.PostAsJsonAsync(url, validBooks);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{validBooks.Count} books successfully sent.");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error] Failed to send the books. Status: {response.StatusCode}");
                Console.WriteLine($"Details: {errorResponse}");
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Not authorized. Check the admin account has the Admin role, or the token may have expired. Rerun to log in again.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Exception] Error connecting to API: {ex.Message}");
            return;
        }

        Console.WriteLine("Seeding process finished!");
    }

    private static async Task<bool> LoginAsync(HttpClient client, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Admin email or password is not set in configuration.");
            return false;
        }

        using var response = await client.PostAsJsonAsync(
            "https://localhost:7213/api/Auth/login", new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Login failed: {response.StatusCode}");
            return false;
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (string.Equals(prop.Name, "token", StringComparison.OrdinalIgnoreCase))
            {
                var token = prop.Value.GetString();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Login failed. Please check your credentials.");
                    return false;
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return true;
            }
        }
        return false;
    }

    private static async Task<HashSet<string>?> GetExistingIsbnsAsync(HttpClient client)
    {
        var existing = new HashSet<string>();
        using var response = await client.GetAsync("https://localhost:7213/api/Book");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Could not load existing books: {response.StatusCode}");
            return null;
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var list = doc.RootElement;
        if (list.ValueKind == JsonValueKind.Object)
        {
            var arrayProp = list.EnumerateObject().FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
            if (arrayProp.Value.ValueKind != JsonValueKind.Array) return existing;
            list = arrayProp.Value;
        }

        foreach (var item in list.EnumerateArray())
        {
            foreach (var prop in item.EnumerateObject())
            {
                if (string.Equals(prop.Name, "isbn", StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.String)
                    existing.Add(NormalizeIsbn(prop.Value.GetString()!));
            }
        }
        return existing;
    }

    private static string NormalizeIsbn(string s) =>
    s.Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();

    private static bool IsValidIsbn10(string s)
    {
        if (s.Length != 10 || !s[..9].All(c => c is >= '0' and <= '9')) return false;
        if (!(s[9] is >= '0' and <= '9' || s[9] == 'X')) return false;
        var sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (10 - i) * (s[i] == 'X' ? 10 : s[i] - '0');
        return sum % 11 == 0;
    }

    private static string Isbn10To13(string s)
    {
        var core = "978" + s[..9];
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (core[i] - '0') * (i % 2 == 0 ? 1 : 3);
        return core + ((10 - sum % 10) % 10);
    }

    private static string? PickIsbn13(IEnumerable<string>? raw)
    {
        if (raw == null) return null;
        var cleaned = raw.Where(r => !string.IsNullOrWhiteSpace(r)).Select(NormalizeIsbn).ToList();

        foreach (var s in cleaned)
            if (s.StartsWith("978") && IsValidIsbn13(s)) return s;

        foreach (var s in cleaned)
            if (IsValidIsbn10(s)) return Isbn10To13(s);

        return null;
    }

    private static bool IsValidIsbn13(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        if (s.Length != 13 || !s.All(char.IsDigit)) return false;
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (s[i] - '0') * (i % 2 == 0 ? 1 : 3);
        return (10 - sum % 10) % 10 == s[12] - '0';
    }
}
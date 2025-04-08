using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Text.Json;
using System.IO;

namespace TheMiddleImdb
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting to crawl The Middle episodes from IMDb using Playwright...");
            
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            
            var result = new Dictionary<string, List<Episode>>();
            
            // Crawl all 9 seasons
            for (int season = 1; season <= 9; season++)
            {
                Console.WriteLine($"Crawling season {season}...");
                
                try
                {
                    // Navigate to the IMDb page for the current season
                    await page.GotoAsync($"https://www.imdb.com/title/tt1442464/episodes/?season={season}");
                    
                    // Wait for the content to load
                    await page.WaitForSelectorAsync("div.ipc-title__text");
                    
                    // Get all episode titles and descriptions using the exact selectors from Task-IMDb.md
                    var titles = await page.QuerySelectorAllAsync("div.ipc-title__text");
                    var descriptions = await page.QuerySelectorAllAsync("div.ipc-html-content-inner-div[role=\"presentation\"]");
                    
                    var episodes = new List<Episode>();
                    
                    // Match titles with descriptions
                    for (int i = 0; i < titles.Count; i++)
                    {
                        if (i < descriptions.Count)
                        {
                            var titleElement = titles[i];
                            var descElement = descriptions[i];
                            
                            if (titleElement != null && descElement != null)
                            {
                                var title = await titleElement.TextContentAsync() ?? "";
                                var description = await descElement.TextContentAsync() ?? "";
                                
                                episodes.Add(new Episode
                                {
                                    title = title.Trim(),
                                    description = description.Trim()
                                });
                                
                                Console.WriteLine($"Found episode: {title.Trim()}");
                            }
                        }
                    }
                    
                    // Add the episodes to the result
                    result[season.ToString()] = episodes;
                    
                    // Add a delay to avoid rate limiting
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error crawling season {season}: {ex.Message}");
                    result[season.ToString()] = new List<Episode>();
                }
            }
            
            // Output the result in the format specified in Task-IMDb.md
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var jsonString = JsonSerializer.Serialize(result, options);
            Console.WriteLine(jsonString);
            
            // Save to file
            await File.WriteAllTextAsync("TheMiddleImdb.json", jsonString);
            Console.WriteLine("\nJSON data has been saved to TheMiddleImdb.json");
            
            Console.WriteLine("\nCrawling completed. Press any key to exit.");
            Console.ReadKey();
        }
    }
    
    public class Episode
    {
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}

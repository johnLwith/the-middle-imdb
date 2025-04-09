using System.Text.Json;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace SyncToDb;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Error: Connection string 'DefaultConnection' not found in appsettings.json");
            return;
        }

        // Read the JSON file
        var jsonContent = await File.ReadAllTextAsync("TheMiddleImdb.json");
        var episodesDict = JsonSerializer.Deserialize<Dictionary<string, List<EpisodeData>>>(jsonContent);

        if (episodesDict == null)
        {
            Console.WriteLine("Failed to parse JSON data");
            return;
        }

        // Create table
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Read and execute the SQL script
        var createTableSql = await File.ReadAllTextAsync("create_episodes_table.sql");
        await using (var cmd0 = new NpgsqlCommand(createTableSql, connection))
        {
            await cmd0.ExecuteNonQueryAsync();
        }

        // Prepare the insert command
        const string insertSql = @"
            INSERT INTO episodes (id, season_number, episode_number, title, description)
            VALUES (@id, @season, @episode, @title, @description)
            ON CONFLICT (id) DO UPDATE 
            SET title = EXCLUDED.title, description = EXCLUDED.description";

        await using var cmd = new NpgsqlCommand(insertSql, connection);

        // Add parameters
        cmd.Parameters.AddWithValue("id", "");
        cmd.Parameters.AddWithValue("season", 0);
        cmd.Parameters.AddWithValue("episode", 0);
        cmd.Parameters.AddWithValue("title", "");
        cmd.Parameters.AddWithValue("description", "");

        // Insert episodes
        foreach (var (seasonStr, episodes) in episodesDict)
        {
            if (!int.TryParse(seasonStr, out int seasonNumber))
                continue;

            foreach (var episode in episodes)
            {
                // Debug: Print the raw title
                Console.WriteLine($"Processing title: {episode.title}");

                if(episode.title.Length < 1)
                {
                    continue;
                }
                
                // Parse season and episode numbers from title
                // Format: "S1.E1 \u2219 Pilot"
                var titleParts = episode.title.Substring(0, episode.title.IndexOf(' '));
                
                // Debug: Print the split parts
                Console.WriteLine($"Title parts: {string.Join(", ", titleParts)}");
                
                if (titleParts.Length < 1)
                {
                    Console.WriteLine($"Warning: Invalid title format: {episode.title}");
                    continue;
                }

                var episodeCode = titleParts; // "S1.E1"
                Console.WriteLine($"Episode code: {episodeCode}");
                
                var codeParts = episodeCode.Split('.');
                Console.WriteLine($"Code parts: {string.Join(", ", codeParts)}");
                
                if (codeParts.Length != 2)
                {
                    Console.WriteLine($"Warning: Invalid episode code format: {episodeCode}");
                    continue;
                }

                // Extract season number from "S1"
                if (!codeParts[0].StartsWith("S") || !int.TryParse(codeParts[0].Substring(1), out int parsedSeason))
                {
                    Console.WriteLine($"Warning: Could not parse season number from: {codeParts[0]}");
                    continue;
                }

                // Extract episode number from "E1"
                if (!codeParts[1].StartsWith("E") || !int.TryParse(codeParts[1].Substring(1), out int parsedEpisode))
                {
                    Console.WriteLine($"Warning: Could not parse episode number from: {codeParts[1]}");
                    continue;
                }

                // Verify the parsed season matches the dictionary key
                if (parsedSeason != seasonNumber)
                {
                    Console.WriteLine($"Warning: Season number mismatch in title {episode.title}");
                    continue;
                }

                cmd.Parameters["id"].Value = $"tm{parsedSeason}{parsedEpisode}";
                cmd.Parameters["season"].Value = parsedSeason;
                cmd.Parameters["episode"].Value = parsedEpisode;
                // "S1.E1 ∙ Pilot"
                cmd.Parameters["title"].Value = episode.title.Remove(0, titleParts.Length + 3);
                cmd.Parameters["description"].Value = episode.description;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        Console.WriteLine("Data import completed successfully!");
    }
}

// Class to match the JSON structure
public class EpisodeData
{
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
}

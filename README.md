# The Middle IMDb Crawler

A .NET application that crawls episode information for "The Middle" TV show from IMDb using Playwright.

## Prerequisites

- .NET 6.0 SDK or later
- Node.js (for Playwright installation)

## Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd the-middle-imdb
```

2. Install .NET dependencies:
```bash
dotnet restore
```

3. Install Playwright browsers:
```bash
# Install Playwright browsers
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

## Running the Application

To run the application:

```bash
dotnet run
```

The application will:
1. Launch a browser window (visible by default)
2. Crawl episode information from IMDb
3. Save the results to `TheMiddleImdb.json`

## Output

The application generates a JSON file (`TheMiddleImdb.json`) containing episode information in the following format:

```json
{
  "seasons": [
    {
      "seasonNumber": 1,
      "episodes": [
        {
          "episodeNumber": 1,
          "title": "Episode Title",
          "rating": 8.5,
          "votes": 1234
        }
      ]
    }
  ]
}
```

## Troubleshooting

If you encounter any issues with Playwright:

1. Make sure you've installed the browsers:
```bash
playwright install
```

```powershell
pwsh bin/Debug/net8.0/playwright.ps1 install
```

2. If you get any browser-related errors, try reinstalling:
```bash
playwright uninstall
playwright install
```

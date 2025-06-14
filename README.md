
# Wei Life Logger - Developer Documentation
## Table of Contents

1. [Project Overview](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#project-overview)
2. [Architecture](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#architecture)
3. [Development Setup](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#development-setup)
4. [Core Components](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#core-components)
5. [Data Flow](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#data-flow)
6. [API Reference](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#api-reference)
7. [Frontend Development](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#frontend-development)
8. [Extending the System](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#extending-the-system)
9. [Testing & Debugging](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#testing--debugging)
10. [Deployment](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#deployment)
11. [Troubleshooting](https://claude.ai/chat/5f4c606f-729c-4360-b8b4-bf17f9269116#troubleshooting)

---

## Project Overview

Wei Life Logger is a local-first life logging system designed to process and visualize personal journal data from the DataJar iOS app. The system features:

- **Privacy-First**: All data remains on local infrastructure
- **Self-Contained**: No external cloud dependencies
- **Terminal Aesthetic**: Retro-futuristic UI design
- **Rich Visualizations**: Maps, charts, and timeline views
- **Agent-Friendly**: Designed for autonomous AI maintenance

### Technology Stack

|Component|Technology|Purpose|
|---|---|---|
|**Backend**|ASP.NET Core (.NET 9)|Web application framework|
|**Database**|SQLite|Local data storage|
|**ETL**|C# Console App|DataJar JSON/CSV processing|
|**Frontend**|Razor Pages + TypeScript|Server-side rendering with interactive elements|
|**Visualization**|Chart.js, Leaflet|Data visualization and mapping|
|**Styling**|Custom CSS|Terminal/retro aesthetic|

---

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                      DataJar iOS App                        │
│                    (External Data Source)                   │
└─────────────────────┬───────────────────────────────────────┘
                      │ Export (CSV/JSON)
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  DataJar Importer (C#)                      │
│  • Parse CSV/JSON  • Validate Data  • Store in SQLite      │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                    SQLite Database                          │
│  • JournalRaw (staging)  • JournalEntries (normalized)     │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core Web Application                   │
│  • Razor Pages  • REST API  • Entity Framework Core        │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  Web Browser Client                         │
│  • Terminal UI  • Chart.js  • Leaflet Maps  • TypeScript   │
└─────────────────────────────────────────────────────────────┘
```

### Directory Structure

```
/life_logger_package/
├── /app/                    # ASP.NET Razor Web Application
│   ├── /Data/              # DbContext and database configuration
│   ├── /Models/            # Data models
│   ├── /Pages/             # Razor pages
│   ├── /Services/          # Business logic services
│   ├── /wwwroot/           # Static assets
│   │   ├── /css/          # Stylesheets
│   │   ├── /js/           # JavaScript/TypeScript
│   │   │   ├── /src/      # TypeScript source
│   │   │   └── /dist/     # Compiled JavaScript
│   │   └── /lib/          # Third-party libraries
│   ├── Program.cs          # Application entry point
│   └── appsettings.json    # Configuration
├── /DataJarImporter/       # C# ETL Application
│   ├── Program.cs          # Main importer logic
│   ├── DataJarConverter.cs # CSV to JSON converter
│   └── /Models/           # Data models
├── /journal_exports/       # Raw DataJar exports
├── /journal_db/           # SQLite database files
├── /logs/                 # Application logs
└── /backups/              # Database backups
```

---

## Development Setup

### Prerequisites

- .NET 9 SDK or later
- Node.js 18+ and npm
- SQLite
- Visual Studio 2022 / VS Code / Rider
- Git

### Initial Setup

1. **Clone the repository**
    
    ```bash
    git clone [repository-url]
    cd life_logger_package
    ```
    
2. **Install .NET dependencies**
    
    ```bash
    # For the web app
    cd app
    dotnet restore
    
    # For the importer
    cd ../DataJarImporter
    dotnet restore
    ```
    
3. **Install npm dependencies**
    
    ```bash
    cd ../app
    npm install
    ```
    
4. **Build TypeScript assets**
    
    ```bash
    npm run build
    ```
    
5. **Initialize the database**
    
    ```bash
    cd ../DataJarImporter
    dotnet run
    ```
    
6. **Run the web application**
    
    ```bash
    cd ../app
    dotnet run
    ```
    

The application will be available at `http://localhost:5270`

### Development Workflow

1. **Backend Changes**: The application uses hot reload, so most C# changes will be reflected immediately
2. **TypeScript Changes**: Run `npm run watch` for automatic compilation
3. **Database Changes**: Update models and run migrations if using EF Core migrations

---

## Core Components

### 1. DataJar Importer

**Purpose**: Processes DataJar exports (CSV/JSON) and imports them into SQLite.

**Key Classes**:

- `Program.cs`: Main entry point, orchestrates the import process
- `DataJarConverter.cs`: Converts CSV format to JSON
- `Models/JournalEntry.cs`: Data model for journal entries

**Usage**:

```bash
cd DataJarImporter
dotnet run -- --input ../journal_exports/store.json
```

**Key Features**:

- Auto-detects CSV vs JSON format
- Handles complex entry formats (e.g., "{ Entry: ... }")
- Robust date/time parsing for multiple formats
- Stores raw JSON for data recovery

### 2. Database Layer

**Schema**:

```sql
-- Staging table for raw imports
CREATE TABLE JournalRaw (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    raw_json TEXT NOT NULL,
    ingestion_datetime TEXT NOT NULL,
    file_source TEXT,
    processing_status TEXT DEFAULT 'pending'
);

-- Normalized entries table
CREATE TABLE JournalEntries (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entry TEXT NOT NULL,
    action TEXT,
    datetime TEXT NOT NULL,
    date TEXT NOT NULL,
    time TEXT NOT NULL,
    location TEXT,
    weather TEXT,
    phone_mode TEXT,
    device TEXT,
    steps INTEGER,
    hrv INTEGER,
    device_battery INTEGER,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Performance indexes
CREATE INDEX idx_journal_date ON JournalEntries(date);
CREATE INDEX idx_journal_datetime ON JournalEntries(datetime);
CREATE INDEX idx_journal_location ON JournalEntries(location);
CREATE INDEX idx_journal_action ON JournalEntries(action);
```

### 3. Web Application

**Services**:

- `IJournalService` / `JournalService`: Core CRUD operations for journal entries
- `IAnalyticsService` / `AnalyticsService`: Statistical analysis and aggregations

**Key Pages**:

- `Index`: Dashboard with system statistics
- `Entries`: Browse all journal entries with filtering
- `Summary`: Daily summary view
- `Analytics`: Advanced data visualizations
- `Maps`: Geographic visualization using Leaflet
- `Timeline`: Chronological event visualization
- `Search`: Full-text search functionality

### 4. Frontend Architecture

**TypeScript Structure** (`wwwroot/js/src/`):

```
/utils/
  api.ts         # API client and data processing utilities
/visualizations/
  charts.ts      # Chart.js wrapper with terminal styling
/maps/
  leafletMaps.ts # Leaflet map functionality
/simple/
  api.ts         # Simplified API for demos
  basicMaps.ts   # Basic map functionality
  basicCharts.ts # Basic chart functionality
```

---

## Data Flow

### 1. Import Process

```
DataJar Export → CSV/JSON File → DataJarImporter → SQLite Database
```

The importer:

1. Reads the export file
2. Detects format (CSV or JSON)
3. Converts CSV to JSON if needed
4. Stores raw JSON in `JournalRaw` table
5. Parses and normalizes data
6. Inserts into `JournalEntries` table
7. Updates processing status

### 2. Web Application Data Flow

```
User Request → Razor Page → Service Layer → Entity Framework → SQLite → Response
```

Example flow for viewing entries:

1. User navigates to `/Entries`
2. `EntriesModel.OnGetAsync()` is called
3. `JournalService.GetEntriesAsync()` queries database
4. Entity Framework translates to SQL
5. Results are mapped to models
6. Razor renders the page with data

---

## API Reference

### REST API Endpoints

#### Get Entries

```http
GET /api/entries?page=1&pageSize=50
```

Returns paginated journal entries.

**Response**:

```json
[
  {
    "id": 1,
    "entry": "Journal entry text",
    "action": "driving",
    "datetime": "2025-06-13 10:30:00",
    "location": "Ann Arbor, MI",
    "steps": 1500
  }
]
```

#### Get Daily Summary

```http
GET /api/summary/{date}
```

Returns summary for a specific date (format: yyyy-MM-dd).

**Response**:

```json
{
  "date": "2025-06-13",
  "entryCount": 15,
  "locations": ["Home", "Office"],
  "totalSteps": 8500,
  "mostFrequentLocation": "Home"
}
```

#### Get Location Frequency

```http
GET /api/locations
```

Returns location visit frequency data.

#### Search Entries

```http
GET /api/search?q=searchterm
```

Full-text search across entries.

#### Health Check

```http
GET /health
```

Returns system health status.

### JavaScript API Client

```typescript
// Import the API client
import { LifeLoggerAPI } from '/js/dist/utils/api.js';

// Get entries
const response = await LifeLoggerAPI.getEntries();
if (response.success) {
    console.log(response.data);
}

// Get summary
const summary = await LifeLoggerAPI.getSummary('2025-06-13');

// Search
const searchResults = await LifeLoggerAPI.search('coffee');
```

---

## Frontend Development

### Terminal UI Styling

The application uses a custom retro-terminal aesthetic defined in `site.css`:

```css
:root {
    --terminal-bg: #0a0a0a;
    --terminal-green: #00ff41;
    --terminal-cyan: #00ffff;
    --terminal-yellow: #ffff00;
    --terminal-red: #ff0040;
}
```

### Chart.js Configuration

Terminal-styled charts:

```typescript
TerminalCharts.createTimeSeriesChart('canvasId', data, 'entries');
```

### Leaflet Map Integration

```typescript
// Create a map
TerminalMaps.createMap('mapId');

// Add location markers
TerminalMaps.createLocationMarkers('mapId', entries);

// Add routes
TerminalMaps.createRouteVisualization('mapId', entries);
```

### TypeScript Development

1. **Add new TypeScript files** in `wwwroot/js/src/`
2. **Import types** from `@types/` packages
3. **Build**: `npm run build`
4. **Watch mode**: `npm run watch`

---

## Extending the System

### Adding a New Data Field

1. **Update the database schema**:
    
    ```sql
    ALTER TABLE JournalEntries ADD COLUMN new_field TEXT;
    ```
    
2. **Update the model** (`Models/JournalEntry.cs`):
    
    ```csharp
    public string? NewField { get; set; }
    ```
    
3. **Update the importer** (`DataJarImporter/Program.cs`):
    
    ```csharp
    command.Parameters.AddWithValue("@new_field", GetStringValue(entry, "new_field"));
    ```
    
4. **Update the UI** to display the new field
    

### Adding a New Page

1. **Create Razor Page**:
    
    ```bash
    dotnet new page -n NewFeature -o Pages
    ```
    
2. **Add navigation** in `_Layout.cshtml`:
    
    ```html
    <li class="nav-item">
        <a class="nav-link" asp-page="/NewFeature">NEW FEATURE</a>
    </li>
    ```
    
3. **Implement page logic** in `NewFeature.cshtml.cs`
    
4. **Style with terminal aesthetic** in `NewFeature.cshtml`
    

### Adding a New Visualization

1. **Create TypeScript module**:
    
    ```typescript
    // wwwroot/js/src/visualizations/newViz.ts
    export class NewVisualization {
        static create(elementId: string, data: any[]) {
            // Implementation
        }
    }
    ```
    
2. **Import in page**:
    
    ```javascript
    import { NewVisualization } from '/js/dist/visualizations/newViz.js';
    ```
    

---

## Testing & Debugging

### Unit Testing

Create test projects:

```bash
dotnet new xunit -n LifeLoggerApp.Tests
dotnet add reference ../app/LifeLoggerApp.csproj
```

Example test:

```csharp
[Fact]
public async Task GetEntriesAsync_ReturnsEntries()
{
    // Arrange
    var service = new JournalService(context, logger);
    
    // Act
    var entries = await service.GetEntriesAsync();
    
    // Assert
    Assert.NotEmpty(entries);
}
```

### Debugging Tips

1. **Enable detailed logging**:
    
    ```json
    "Logging": {
      "LogLevel": {
        "Default": "Debug"
      }
    }
    ```
    
2. **Check logs**:
    
    ```bash
    tail -f logs/webapp.log
    ```
    
3. **Database queries**:
    
    ```bash
    sqlite3 journal_db/life_logger.db
    .tables
    SELECT COUNT(*) FROM JournalEntries;
    ```
    
4. **Browser DevTools**:
    
    - Check Network tab for API calls
    - Console for JavaScript errors
    - Use breakpoints in TypeScript

---

## Deployment

### Local Development Build

```bash
# Build for development
cd app
dotnet build
npm run build
```

### Production Build

```bash
# Self-contained deployment for Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux

# For Synology NAS
scp -r ./publish/linux/* user@synology-nas:/volume1/life_logger_package/
```

### Systemd Service Setup

Create service file `/etc/systemd/system/wei-life-logger.service`:

```ini
[Unit]
Description=Wei Life Logger Web Application
After=network.target

[Service]
Type=simple
User=wei
WorkingDirectory=/volume1/life_logger_package/app
ExecStart=/volume1/life_logger_package/app/LifeLoggerApp --urls "http://0.0.0.0:5000"
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl enable wei-life-logger
sudo systemctl start wei-life-logger
```

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Errors

```bash
# Check database exists
ls -la journal_db/life_logger.db

# Check permissions
chmod 644 journal_db/life_logger.db

# Verify connection string
grep DefaultConnection app/appsettings.json
```

#### 2. Import Failures

```bash
# Validate JSON
python3 -m json.tool journal_exports/store.json

# Check importer logs
tail -f logs/importer.log

# Manually inspect raw data
sqlite3 journal_db/life_logger.db "SELECT raw_json FROM JournalRaw ORDER BY id DESC LIMIT 1;"
```

#### 3. TypeScript Build Errors

```bash
# Clean and rebuild
npm run clean
npm install
npm run build

# Check TypeScript version
npx tsc --version
```

#### 4. Web App Not Loading

```bash
# Check if port is in use
lsof -i :5270

# Run with different port
dotnet run --urls "http://localhost:5001"
```

### Performance Optimization

1. **Database**:
    
    ```sql
    -- Vacuum to reclaim space
    VACUUM;
    
    -- Analyze for query optimization
    ANALYZE;
    ```
    
2. **Caching**: Consider implementing memory caching for frequent queries
    
3. **Pagination**: Always use pagination for large datasets
    

---

## Security Considerations

1. **Local-Only Design**: No external API calls or cloud dependencies
2. **Input Validation**: All user inputs are validated and parameterized
3. **File Permissions**: Restrict access to database and config files
4. **Authentication**: Consider adding authentication for network deployments
5. **HTTPS**: Use HTTPS in production environments

---

## Contributing Guidelines

1. **Code Style**: Follow C# and TypeScript conventions
2. **Terminal Aesthetic**: Maintain the retro-futuristic design
3. **Local-First**: No external dependencies or cloud services
4. **Documentation**: Update docs for significant changes
5. **Testing**: Add tests for new functionality

---

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Chart.js Documentation](https://www.chartjs.org/docs)
- [Leaflet Documentation](https://leafletjs.com/reference)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

---

**Version**: 2.0.0  
**Last Updated**: June 2025  
**Maintainer**: Wei Life Logger Team

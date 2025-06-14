# Wei Life Logger v1.0 - Claude Agentic README

---

## SYSTEM OVERVIEW

This document provides a comprehensive operational guide for Wei's local life-logging system, designed specifically for Claude or other LLM agents to understand, maintain, debug, and extend the system autonomously.

**Core Principles:**
- **Local-First**: No external cloud APIs, all data remains on local infrastructure
- **Self-Contained**: Complete system bootstrapping from this document
- **Agent-Friendly**: Designed for autonomous AI maintenance and operation

---

## TECHNOLOGY STACK

| Component | Technology | Purpose |
|-----------|------------|---------|
| **ETL Pipeline** | C# (.NET 8+) | JSON parsing and data ingestion |
| **Database** | SQLite | Local data storage |
| **Web Interface** | ASP.NET Razor Pages | User dashboard and data visualization |
| **Deployment** | Self-contained publish | Portable cross-platform builds |
| **Host Platform** | Synology NAS | Primary deployment target |

---

## DIRECTORY ARCHITECTURE

```
/life_logger_package/
├── /app/                    # Published ASP.NET Razor Web Application
│   ├── /Controllers/        # Web controllers
│   ├── /Models/            # Data models
│   ├── /Views/             # Razor views
│   ├── /wwwroot/           # Static assets
│   ├── appsettings.json    # Configuration
│   └── Program.cs          # Application entry point
├── /DataJarImporter/       # C# ETL Application
│   ├── Program.cs          # Importer logic
│   ├── Models/             # Data models
│   └── *.csproj           # Project configuration
├── /journal_exports/       # Raw DataJar Export Storage
│   └── store.json         # Latest export from DataJar iOS app
├── /journal_db/           # SQLite Database Files
│   └── life_logger.db     # Primary database
├── /logs/                 # System Operation Logs
│   ├── importer.log       # ETL operation logs
│   └── webapp.log         # Web application logs
├── /configs/              # Configuration Files
│   └── connection_strings.json
└── /backups/              # Database Backups
    └── life_logger_backup_YYYYMMDD.db
```

---

## DATA FLOW PIPELINE

### 1. Data Export (Manual)
```
DataJar iOS App → Export → store.json → /journal_exports/
```

### 2. ETL Processing (Automated)
```
C# DataJarImporter → Read store.json → Parse & Validate → SQLite Ingestion
```

### 3. Data Access (Web Interface)
```
Razor Web App → SQLite Queries → Dashboard Visualization
```

---

## DATABASE SCHEMA

### JournalRaw (Staging Table)
```sql
CREATE TABLE JournalRaw (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    raw_json TEXT NOT NULL,
    ingestion_datetime TEXT NOT NULL,
    file_source TEXT,
    processing_status TEXT DEFAULT 'pending'
);
```

### JournalEntries (Normalized Table)
```sql
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
```

### Indexes for Performance
```sql
CREATE INDEX idx_journal_date ON JournalEntries(date);
CREATE INDEX idx_journal_datetime ON JournalEntries(datetime);
CREATE INDEX idx_journal_location ON JournalEntries(location);
CREATE INDEX idx_journal_action ON JournalEntries(action);
```

---

## AGENT OPERATIONAL COMMANDS

### System Bootstrap
```bash
# Navigate to system directory
cd /life_logger_package/

# Build ETL Importer
cd DataJarImporter
dotnet restore
dotnet build -c Release

# Build Web Application
cd ../app
dotnet restore
dotnet build -c Release
```

### Data Processing
```bash
# Process new DataJar export
cd /life_logger_package/DataJarImporter
dotnet run --project . --input ../journal_exports/store.json

# Verify data ingestion
sqlite3 ../journal_db/life_logger.db "SELECT COUNT(*) FROM JournalEntries;"
```

### Web Application Launch
```bash
cd /life_logger_package/app
dotnet run --urls "http://localhost:5000"
```

### Database Maintenance
```bash
# Create backup
cp /life_logger_package/journal_db/life_logger.db \
   /life_logger_package/backups/life_logger_backup_$(date +%Y%m%d).db

# Vacuum database
sqlite3 /life_logger_package/journal_db/life_logger.db "VACUUM;"

# Analyze for optimization
sqlite3 /life_logger_package/journal_db/life_logger.db "ANALYZE;"
```

---

## TROUBLESHOOTING GUIDE

### Common Issues and Solutions

**1. ETL Import Failures**
```bash
# Check logs
tail -f /life_logger_package/logs/importer.log

# Validate JSON format
python3 -m json.tool /life_logger_package/journal_exports/store.json

# Manual inspection of raw data
sqlite3 /life_logger_package/journal_db/life_logger.db \
"SELECT raw_json FROM JournalRaw ORDER BY id DESC LIMIT 1;"
```

**2. Web Application Issues**
```bash
# Check application logs
tail -f /life_logger_package/logs/webapp.log

# Verify database connectivity
sqlite3 /life_logger_package/journal_db/life_logger.db ".tables"

# Test database queries
sqlite3 /life_logger_package/journal_db/life_logger.db \
"SELECT COUNT(*) as total_entries FROM JournalEntries;"
```

**3. Database Corruption**
```bash
# Check integrity
sqlite3 /life_logger_package/journal_db/life_logger.db "PRAGMA integrity_check;"

# Restore from backup
cp /life_logger_package/backups/life_logger_backup_YYYYMMDD.db \
   /life_logger_package/journal_db/life_logger.db
```

---

## DEPLOYMENT INSTRUCTIONS

### Local Development (macOS)
```bash
# Build self-contained executable
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/osx

# Run locally
./publish/osx/DataJarImporter
./publish/osx/WebApp
```

### Synology NAS Deployment
```bash
# Build for Linux
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux

# Deploy via SCP
scp -r ./publish/linux/* user@synology-nas:/volume1/life_logger_package/

# Set executable permissions
ssh user@synology-nas "chmod +x /volume1/life_logger_package/DataJarImporter"
ssh user@synology-nas "chmod +x /volume1/life_logger_package/WebApp"
```

### System Service Setup (Synology)
```bash
# Create systemd service file
sudo tee /etc/systemd/system/wei-life-logger.service > /dev/null <<EOF
[Unit]
Description=Wei Life Logger Web Application
After=network.target

[Service]
Type=simple
User=wei
WorkingDirectory=/volume1/life_logger_package/app
ExecStart=/volume1/life_logger_package/app/WebApp --urls "http://0.0.0.0:5000"
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
sudo systemctl enable wei-life-logger
sudo systemctl start wei-life-logger
```

---

## WEB INTERFACE FEATURES

### Dashboard Components
- **Daily Summary**: Entry counts, locations, activities
- **Timeline View**: Chronological journal entries
- **Location Analytics**: Most visited places, travel patterns
- **Activity Tracking**: Driving logs, step counts, HRV data
- **Weather Correlation**: Weather patterns with activities
- **Search Interface**: Full-text search across all entries
- **Export Tools**: CSV, JSON data export capabilities

### API Endpoints (Internal)
- `GET /api/entries` - Retrieve journal entries
- `GET /api/summary/{date}` - Daily summary data
- `GET /api/locations` - Location frequency data
- `GET /api/search?q={query}` - Search entries

---

## AGENT DECISION TREE

### When New Data Arrives
```
1. Check /journal_exports/ for new store.json
2. If found → Run DataJarImporter
3. Monitor logs for success/failure
4. If failure → Inspect raw_json in JournalRaw table
5. If success → Verify normalization in JournalEntries
6. Update processing logs
```

### For System Maintenance
```
1. Daily: Check disk space, log rotation
2. Weekly: Database backup, vacuum operation
3. Monthly: Performance analysis, index optimization
4. Quarterly: Full system backup, dependency updates
```

### For Data Analysis Requests
```
1. Query JournalEntries for structured data
2. Use SQLite aggregation functions
3. Export results as needed
4. Create visualizations via web interface
```

---

## EXTENSIBILITY FRAMEWORK

### Adding New Data Sources
1. Create new importer in `/DataJarImporter/Importers/`
2. Extend database schema if needed
3. Update web interface models and views
4. Add processing logic to ETL pipeline

### Custom Analytics
1. Add computed columns to existing tables
2. Create new summary tables for complex aggregations
3. Implement scheduled data processing jobs
4. Extend web API with new endpoints

### Integration Points
- **Health Data**: Apple Health, Google Fit integration
- **Calendar Data**: Calendar event correlation
- **Photo Data**: Image metadata extraction
- **Location Data**: Enhanced GPS tracking

---

## SECURITY CONSIDERATIONS

- **Local-Only**: No external network dependencies
- **File Permissions**: Restrict access to database files
- **Web Interface**: Consider authentication for network access
- **Backup Security**: Encrypt sensitive backup files
- **Log Privacy**: Sanitize logs of personal information

---

## RECOVERY PROCEDURES

### Complete System Recovery
1. Restore `/life_logger_package/` from backup
2. Rebuild applications: `dotnet build -c Release`
3. Verify database integrity
4. Restart web service
5. Test data ingestion pipeline

### Data Recovery
1. Restore database from latest backup
2. Re-run ETL import from `/journal_exports/`
3. Verify data consistency
4. Update web interface cache

---

## MONITORING AND HEALTH CHECKS

### System Health Indicators
- ETL processing success rate
- Database query performance
- Web application response times
- Disk space utilization
- Log file sizes

### Automated Health Checks
```bash
#!/bin/bash
# Health check script
echo "Checking Life Logger System Health..."

# Database connectivity
sqlite3 /life_logger_package/journal_db/life_logger.db "SELECT 1;" > /dev/null 2>&1
echo "Database: $([[ $? -eq 0 ]] && echo "OK" || echo "ERROR")"

# Web application
curl -s http://localhost:5000/health > /dev/null 2>&1
echo "Web App: $([[ $? -eq 0 ]] && echo "OK" || echo "ERROR")"

# Disk space
df -h /life_logger_package/ | awk 'NR==2 {print "Disk Usage: " $5}'
```

---

## FINAL NOTES FOR AGENTS

**Critical Agent Guidelines:**
- ✅ Always preserve data integrity during operations
- ✅ Create backups before major changes
- ✅ Log all significant operations
- ✅ Verify data consistency after modifications
- ✅ Maintain local-first architecture
- ✅ Document any system modifications

**Prohibited Actions:**
- ❌ Never connect to external APIs without explicit approval
- ❌ Never modify raw data without backup
- ❌ Never expose personal data beyond local network
- ❌ Never bypass error handling mechanisms

This README serves as the definitive operational guide. Any AI agent with access to this document and the `/life_logger_package/` directory can fully maintain, extend, debug, and operate the system autonomously.

---

**Wei Life Logger v1.0**  
*Agent-Ready Documentation*  
*Last Updated: June 2025*
# Journal Exports Directory

This directory contains journal data exports from DataJar iOS app and conversion scripts.

## Data Protection

**ALL PERSONAL JOURNAL DATA IS PROTECTED BY `.gitignore`**

The following files are automatically excluded from git commits:
- `*.json` - All JSON export files
- `*.txt` - Text export files  
- `raw_*.json` - Raw export files
- `converted_*.json` - Converted data files
- `clean_*.json` - Cleaned data files
- `store*.json` - DataJar store files

## File Types

### Personal Data (Not Committed)
- `store.json` - Original DataJar export
- `raw_store.json` - Raw imported data
- `converted_store.json` - Converted structured data
- `clean_store.json` - Final cleaned data for import
- `entries_content.txt` - Text extraction of entries

### Scripts (Committed)
- `*.py` - Data conversion and processing scripts
- `README.md` - This documentation file

### Demo Data (Safe to Commit)
- `sample_*.json` - Anonymized sample data
- `demo_*.json` - Demo data for testing
- `example_*.json` - Example data formats

## Usage

1. Export your journal data from DataJar iOS app as `store.json`
2. Run conversion scripts to process the data
3. Import cleaned data using the DataJarImporter
4. Personal data stays local and is never committed to git

## Privacy First

This system is designed to be **local-first** and **privacy-first**. Your personal journal data never leaves your local environment and is protected from accidental commits.
EOF < /dev/null
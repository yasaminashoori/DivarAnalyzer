# Divar Real Estate Analyzer 🏠

A powerful web-based tool for analyzing Tehran real estate market data from Divar.ir. Built with C# (.NET 8) backend and vanilla JavaScript frontend.

## See Online 🔗
https://divarrealstatesinthesedays.netlify.app/

## Features 📊

- **Real-time Data Scraping**: Extract property listings from Divar.ir
- **Interactive Analysis**: Filter by date, district, and price ranges
- **Visualizations**: Charts, maps, and trend analysis
- **Export Functionality**: Download filtered data as CSV
- **Responsive Design**: Works on desktop and mobile devices
- **RESTful API**: Clean API endpoints for data access

## Quick Start 🚀

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Modern web browser

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/divar-analyzer.git
   cd divar-analyzer
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Open your browser**
   Navigate to `https://localhost:5001` or `http://localhost:5000`

## Usage 📖

### Web Interface

1. **Load Data**: 
   - Click "Load Sample Data" for demo purposes
   - Or upload your own CSV file with property data

2. **Filter Data**:
   - Set date ranges using the date pickers
   - Select specific districts (1, 2, 3, 6, 15)

3. **Analyze**:
   - Click "Analyze" to generate charts and statistics
   - View price trends, district comparisons, and market insights

4. **Export**:
   - Download filtered data as CSV for further analysis

### API Endpoints

```http
GET  /api/divar/sample-data?count=100    # Generate sample data
POST /api/divar/analyze                  # Analyze property data
GET  /api/divar/scrape                   # Scrape live data (advanced)
POST /api/divar/export-csv               # Export data as CSV
```

## Architecture 🏗️

### Backend (C#/.NET)
- **ASP.NET Core 8**: Web API framework
- **CsvHelper**: CSV file processing
- **AngleSharp**: HTML parsing for web scraping
- **System.Text.Json**: JSON serialization

### Frontend
- **Vanilla JavaScript**: No framework dependencies
- **Chart.js**: Interactive charts and visualizations
- **Leaflet**: Interactive maps
- **PapaParse**: Client-side CSV processing


## Thanks to:

- **Divar.ir**: Data source for Tehran real estate market
- **Chart.js**: Excellent charting library
- **Leaflet**: Open-source mapping solution
- **OpenStreetMap**: Map data provider

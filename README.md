# 🏠 Divar Real Estate Analyzer

<div align="center">
  
  [![Live Demo](https://img.shields.io/badge/🌐_Live_Demo-Visit_Site-blue?style=for-the-badge)](https://divarrealstatesinthesedays.netlify.app/)
  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/)
  [![JavaScript](https://img.shields.io/badge/JavaScript-ES6+-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
  [![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

  **A powerful web-based tool for analyzing Tehran real estate market data from Divar.ir**
  
  *Discover market trends • Analyze property prices • Visualize data insights*
</div>

---
## See online:
[https://divarrealstatesinthesedays.netlify.app/](Demo)


## ✨ Features

### 📊 **Comprehensive Data Analysis**
- Real-time property data processing
- Interactive filtering by date, district, and price ranges
- Advanced statistical metrics and market insights
- Trend analysis with growth calculations

### 📈 **Rich Visualizations** 
- **Time Series Charts**: Track property listings over time
- **Price Distribution**: Compare average prices across districts
- **Interactive Maps**: Geographic visualization with Leaflet
- **Statistical Dashboard**: Key market indicators at a glance

### 🎯 **Smart Filtering**
- Date range selection with Persian calendar support
- District-based filtering (Districts 1, 2, 3, 6, 15)
- Real-time data updates
- Export filtered results to CSV

### 🌍 **User Experience**
- Responsive design for all devices
- Persian/Farsi number formatting
- Dark theme with modern UI
- Fast client-side processing

---

## 🚀 Quick Start

### 🌐 **Try Online**
Visit the live demo: **[divarrealstatesinthesedays.netlify.app](https://divarrealstatesinthesedays.netlify.app/)**

### 💻 **Local Development**

**Prerequisites:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Modern web browser

**Installation:**

```bash
# Clone the repository
git clone https://github.com/yourusername/divar-analyzer.git
cd divar-analyzer

# Navigate to backend
cd Backend/DivarAnalyzer

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Open browser
# Navigate to https://localhost:5001
```

---

## 📱 How to Use

### 1. **Load Data**
```
🎯 Click "Click to load sample data" to generate demo data
📁 Or upload your own CSV file with property listings
```

### 2. **Apply Filters**
```
📅 Set date ranges using the date pickers
🏘️ Select specific districts (1, 2, 3, 6, 15)
🔍 Filter by price ranges and property types
```

### 3. **Analyze & Visualize**
```
📊 Click "Analyze" to generate comprehensive analysis
📈 View interactive charts and market trends
🗺️ Explore geographic distribution on the map
📋 Review detailed statistics and insights
```

### 4. **Export Results**
```
💾 Download filtered data as CSV
📑 Generate detailed market reports
📊 Share analysis results
```

---

## 🏗️ Architecture

### **Backend Stack**
```
🎯 ASP.NET Core 8.0        # Web API Framework
📄 CsvHelper              # CSV Processing
🌐 AngleSharp             # Web Scraping
📊 System.Text.Json       # JSON Serialization
🗄️ Entity Framework       # Data Access (Future)
```

### **Frontend Stack**
```
⚡ Vanilla JavaScript      # Core Logic
📊 Chart.js               # Interactive Charts
🗺️ Leaflet               # Interactive Maps
📄 PapaParse              # CSV Processing
🎨 Custom CSS             # Modern UI Design
```

### **Key Components**
- **DivarDataAnalyzer**: Core analysis engine
- **DivarScraper**: Data collection service
- **DivarController**: RESTful API endpoints
- **Interactive Dashboard**: Real-time visualization

---

## 🎯 API Reference

### **Core Endpoints**

```http
GET  /api/divar/sample-data?count=100
```
Generate sample real estate data for testing

```http
POST /api/divar/analyze
Content-Type: application/json

{
  "data": [...],
  "fromDate": "2024-01-01",
  "toDate": "2024-12-31",
  "district": "all"
}
```

```http
POST /api/divar/export-csv
Content-Type: application/json

[...propertyData]
```

### **Response Format**
```json
{
  "success": true,
  "data": {
    "rawData": [...],
    "aggregatedData": [...],
    "metrics": {
      "totalListings": 1250,
      "avgTotal": 15000000000,
      "avgSqm": 85000000
    },
    "insights": [...]
  },
  "message": "Analysis completed successfully"
}
```

---

## 📊 Data Model

### **RealEstateData**
```csharp
public class RealEstateData
{
    public DateTime ScrapedDate { get; set; }
    public string District { get; set; }
    public int? Size { get; set; }
    public long? TotalPrice { get; set; }
    public long? PricePerSqm { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Title { get; set; }
    public int? Age { get; set; }
}
```

### **Supported Districts**
- **District 1**: Shemiran (شمیران)
- **District 2**: Vanak (ونک)
- **District 3**: Zaferaniyeh (زعفرانیه)
- **District 6**: Yusefabad (یوسف‌آباد)
- **District 15**: Shahrak (شهرک)

---

## 🔧 Configuration

### **Backend Settings** (`appsettings.json`)
```json
{
  "DivarScraper": {
    "RequestDelayMs": 2000,
    "MaxRetryAttempts": 3,
    "RateLimitPerMinute": 30
  },
  "DataAnalysis": {
    "MaxRecordsPerAnalysis": 50000,
    "DefaultSampleSize": 100,
    "BaselineDate": "2024-06-12"
  }
}
```

---

## 🚀 Deployment

### **Frontend (Netlify)**
The frontend is deployed on Netlify with automatic builds:
- **Live URL**: https://divarrealstatesinthesedays.netlify.app/
- **Build Command**: None (static files)
- **Publish Directory**: `Frontend/`

### **Backend Deployment Options**
```bash
# Azure App Service
az webapp up --name divar-analyzer --resource-group myResourceGroup

# Docker
docker build -t divar-analyzer .
docker run -p 5000:5000 divar-analyzer

# Railway/Heroku
git push railway main
```

---

## 📈 Features Roadmap

### **Phase 1** ✅
- [x] Basic data analysis and visualization
- [x] Interactive charts and maps
- [x] CSV export functionality
- [x] Responsive design

### **Phase 2**
- [ ] Real-time data scraping
- [ ] Advanced filtering options
- [ ] Price prediction models
- [ ] Historical trend analysis

### **Phase 3** 
- [ ] User authentication
- [ ] Saved searches and alerts
- [ ] API rate limiting
- [ ] Chatbot using LLM Agent for asking about the infrmations
- [ ] Database integration

---

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### **Development Guidelines**
- Follow C# coding conventions
- Write meaningful commit messages
- Add tests for new features
- Update documentation

---

## 🙏 Acknowledgments

- **[Divar.ir](https://divar.ir)** 
- **[Chart.js](https://www.chartjs.org/)** - Excellent charting library
- **[Leaflet](https://leafletjs.com/)** - Open-source mapping solution
- **[OpenStreetMap](https://www.openstreetmap.org/)** - Map data provider
- **[Netlify](https://netlify.com)** - Frontend hosting platform

---

## 📞 Support

- **🐛 Bug Reports**: [Create an Issue](https://github.com/yourusername/divar-analyzer/issues)
- **💡 Feature Requests**: [Start a Discussion](https://github.com/yourusername/divar-analyzer/discussions)
- **📧 Email**: iamyasaminaho@gmail.com

---

<div align="center">
  
  [![GitHub stars](https://img.shields.io/github/stars/yourusername/divar-analyzer?style=social)](https://github.com/yourusername/divar-analyzer)
  [![GitHub forks](https://img.shields.io/github/forks/yourusername/divar-analyzer?style=social)](https://github.com/yourusername/divar-analyzer)
</div>

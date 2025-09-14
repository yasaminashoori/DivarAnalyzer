# 🏠 Divar Real Estate Analyzer

<div align="center">

[🌐 Live Demo](https://divarrealstatesinthesedays.netlify.app/) |  
[.NET 8.0](https://dotnet.microsoft.com/) |  
[JavaScript ES6+](https://developer.mozilla.org/en-US/docs/Web/JavaScript) |  
[License MIT](LICENSE)

*Analyze Tehran real estate trends, visualize data, and export results.*

</div>

---

## 🌐 Live Demo
[https://divarrealstatesinthesedays.netlify.app/](https://divarrealstatesinthesedays.netlify.app/)

---

## 🚀 Quick Start

### Local Development
```bash
# Clone repository
git clone https://github.com/yourusername/divar-analyzer.git
cd divar-analyzer

# Backend
cd Backend/DivarAnalyzer
dotnet restore
dotnet run
```
### Using Docker Compose

```bash
docker compose up --build
```

### Default Ports 
```
API HTTP  -> http://localhost:5000
API HTTPS -> https://localhost:5001
Frontend  -> http://localhost:3000
```

---

## 📊 Features

* **Real-time Analysis**: Filter by date, district, price, property type
* **Interactive Charts & Maps**: Chart.js + Leaflet for insights
* **Export Options**: CSV & Markdown report generation
* **User-Friendly**: Responsive, dark theme, Persian number formatting

---

## 🔧 Stack

| Backend                   | Frontend   |
| ------------------------- | ---------- |
| ASP.NET Core 8.0          | Vanilla JS |
| CsvHelper                 | Chart.js   |
| AngleSharp                | Leaflet    |
| System.Text.Json          | PapaParse  |
| Entity Framework (future) | Custom CSS |

---

## 🗂️ API Overview

### Endpoints

| Method | Endpoint                           | Description                      |
| ------ | ---------------------------------- | -------------------------------- |
| GET    | `/api/divar/sample-data?count=100` | Generate sample real estate data |
| POST   | `/api/divar/analyze`               | Analyze provided property data   |
| POST   | `/api/divar/export-csv`            | Export filtered data to CSV      |

### Sample `/api/divar/analyze` Request

```json
{
  "data": [...],
  "fromDate": "2024-01-01",
  "toDate": "2024-12-31",
  "district": "all"
}
```

### Sample Response

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

```csharp
public class RealEstateData {
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

### Supported Districts

* District 1: Shemiran (شمیران)
* District 2: Vanak (ونک)
* District 3: Zaferaniyeh (زعفرانیه)
* District 6: Yusefabad (یوسف‌آباد)
* District 15: Shahrak (شهرک)

---

## 📈 Roadmap

* **Phase 1 ✅**: Charts, maps, CSV export, responsive design
* **Phase 2**: Real-time scraping, advanced filters, price prediction
* **Phase 3**: User accounts, saved searches, LLM chatbot, DB integration

---

## 🤝 Contributing

1. Fork → Create branch → Commit → Push → Pull Request
2. Follow C# conventions, write tests, update docs

---

## 📞 Support

* **Bugs**: [Issues](https://github.com/yourusername/divar-analyzer/issues)
* **Features**: [Discussions](https://github.com/yourusername/divar-analyzer/discussions)
* **Email**: [iamyasaminaho@gmail.com](mailto:iamyasaminaho@gmail.com)

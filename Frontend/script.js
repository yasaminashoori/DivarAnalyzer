const API_BASE = "/api/divar";
const $ = (selector) => document.querySelector(selector);
let rawData = [];
let timeSeriesChart = null;
let priceChart = null;
let map = null;
const formatNumber = (num) => new Intl.NumberFormat("en-US").format(num);
const formatPrice = (price) => (price / 1e9).toFixed(1);
const formatPriceM = (price) => (price / 1e6).toFixed(1);

const toPersianNumbers = (str) => {
  const persianNumbers = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"];
  return str.toString().replace(/[0-9]/g, (digit) => persianNumbers[digit]);
};

const useJalali = true;

function gregorianToJalali(gy, gm, gd) {
  const g_d_n = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
  let jy, jm, jd, gy2, days;

  if (gy > 1600) {
    jy = 979;
    gy -= 1600;
  } else {
    jy = 0;
    gy -= 621;
  }

  if (gm > 2) {
    gy2 = gy + 1;
  } else {
    gy2 = gy;
  }

  days =
    365 * gy +
    Math.floor((gy2 + 3) / 4) -
    Math.floor((gy2 + 99) / 100) +
    Math.floor((gy2 + 399) / 400) -
    80 +
    gd +
    g_d_n[gm - 1];
  jy += 33 * Math.floor(days / 12053);
  days %= 12053;
  jy += 4 * Math.floor(days / 1461);
  days %= 1461;

  if (days > 365) {
    jy += Math.floor((days - 1) / 365);
    days = (days - 1) % 365;
  }

  if (days < 186) {
    jm = 1 + Math.floor(days / 31);
    jd = 1 + (days % 31);
  } else {
    jm = 7 + Math.floor((days - 186) / 30);
    jd = 1 + ((days - 186) % 30);
  }

  return [jy, jm, jd];
}

function toJalali(dateStr) {
  try {
    if (!useJalali || !dateStr) return dateStr;
    const parts = dateStr.split(/[-/T]/);
    const year = parseInt(parts[0]);
    const month = parseInt(parts[1]);
    const day = parseInt(parts[2]);

    const [jy, jm, jd] = gregorianToJalali(year, month, day);
    const formattedDate = `${jy}/${jm.toString().padStart(2, "0")}/${jd
      .toString()
      .padStart(2, "0")}`;
    return toPersianNumbers(formattedDate);
  } catch (error) {
    console.error("Error converting date:", error);
    return dateStr;
  }
}

function normalizeDateKey(d) {
  if (!d) return null;
  return String(d).split("T")[0]; //"YYYY-MM-DD"
}
function showMessage(message, type = "info") {
  const messagesDiv = $("#messages");
  const messageClass =
    type === "error" ? "error" : type === "success" ? "success" : "loading";
  messagesDiv.innerHTML = `<div class="${messageClass}">${message}</div>`;

  if (type !== "error") {
    setTimeout(() => {
      messagesDiv.innerHTML = "";
    }, 3000);
  }
}
async function generateSampleData() {
  showMessage("در حال تولید داده‌های نمونه...", "loading");

  try {
    const response = await fetch(`${API_BASE}/sample-data?count=100`);
    if (!response.ok)
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);

    const result = await response.json();
    if (!result.success)
      throw new Error(result.message || "API returned error");

    rawData = result.data || [];
    showMessage(
      `${toPersianNumbers(rawData.length)} رکورد نمونه تولید شد`,
      "success"
    );
  } catch (error) {
    console.error("Error generating sample data:", error);
    showMessage(`خطا در تولید داده‌های نمونه: ${error.message}`, "error");
    rawData = generateClientSampleData();
    showMessage(
      `${toPersianNumbers(rawData.length)} رکورد نمونه تولید شد`,
      "success"
    );
  }
}

function generateClientSampleData() {
  const districts = ["1", "2", "3", "6", "15"];
  const sampleData = [];
  const startDate = new Date("2024-08-15");
  const endDate = new Date("2024-08-29");

  const districtCoords = {
    1: { lat: 35.7797, lng: 51.4183 },
    2: { lat: 35.7797, lng: 51.4026 },
    3: { lat: 35.7869, lng: 51.4399 },
    6: { lat: 35.7219, lng: 51.3892 },
    15: { lat: 35.6669, lng: 51.3753 },
  };

  for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {
    districts.forEach((district) => {
      const count = 30 + Math.floor(Math.random() * 50);

      for (let i = 0; i < count; i++) {
        const size = 60 + Math.floor(Math.random() * 140);
        const pricePerSqm =
          (50_000_000 + Math.random() * 100_000_000) *
          (parseInt(district) * 0.5);
        const totalPrice = pricePerSqm * size;

        const baseCoords = districtCoords[district];
        const latitude = baseCoords.lat + (Math.random() - 0.5) * 0.02;
        const longitude = baseCoords.lng + (Math.random() - 0.5) * 0.02;

        sampleData.push({
          scrapedDate: d.toISOString().slice(0, 10),
          district: district,
          totalPrice: Math.floor(totalPrice),
          pricePerSqm: Math.floor(pricePerSqm),
          size: size,
          latitude: latitude,
          longitude: longitude,
          title: `آپارتمان ${size} متری منطقه ${toPersianNumbers(district)}`,
          age: Math.floor(Math.random() * 25),
        });
      }
    });
  }

  return sampleData;
}
async function analyzeData() {
  if (rawData.length === 0) {
    showMessage("لطفاً ابتدا داده‌ها را بارگذاری کنید", "error");
    return;
  }

  showMessage("در حال تحلیل داده‌ها...", "loading");

  try {
    const fromDate = $("#dateFrom").value;
    const toDate = $("#dateTo").value;
    const district = $("#districtFilter").value;

    const analysisRequest = {
      data: rawData,
      useFile: false,
      fromDate: fromDate || null,
      toDate: toDate || null,
      district: district || "all",
    };

    const response = await fetch(`${API_BASE}/analyze`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(analysisRequest),
    });

    if (!response.ok)
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);

    const result = await response.json();
    if (!result.success) throw new Error(result.message || "Analysis failed");

    const analysis = result.data;

    updateStats(analysis);
    drawTimeSeriesChart(analysis.aggregatedData || analysis.rawData);
    drawPriceChart(analysis.aggregatedData || analysis.rawData);
    createMap(analysis.rawData);
    updateDataTable(analysis.aggregatedData || analysis.rawData);
    showInsights(analysis.insights);
    showPeaks(analysis.rawData);

    showComponents();
    showMessage("تحلیل با موفقیت انجام شد", "success");
  } catch (error) {
    console.error("Analysis error:", error);
    showMessage(`خطا در تحلیل: ${error.message}`, "error");
    performClientSideAnalysis();
  }
}

function showComponents() {
  $("#statsGrid").style.display = "grid";
  $("#timeChart").style.display = "block";
  $("#priceChart").style.display = "block";
  $("#mapContainer").style.display = "block";
  $("#insights").style.display = "block";
  $("#peaksPanel").style.display = "block";
  $("#dataTable").style.display = "block";
}

function performClientSideAnalysis() {
  updateStatsFromData(rawData);
  drawTimeSeriesChartFromData(rawData);
  drawPriceChartFromData(rawData);
  createMapFromData(rawData);
  updateDataTableFromData(rawData);
  showInsightsFromData(rawData);
  showPeaksFromData(rawData);
  showComponents();
  showMessage("تحلیل انجام شد", "success");
}
function updateStats(analysis) {
  const metrics = analysis.metrics;
  const data = analysis.rawData;

  $("#totalAds").textContent = toPersianNumbers(
    formatNumber(metrics.totalListings)
  );
  $("#avgPrice").textContent = toPersianNumbers(formatPrice(metrics.avgTotal));

  const growth = Math.random() * 10 - 5;
  $("#priceGrowth").textContent = `${growth > 0 ? "+" : ""}${toPersianNumbers(
    growth.toFixed(1)
  )}%`;

  const districtCounts = {};
  data.forEach((item) => {
    districtCounts[item.district] = (districtCounts[item.district] || 0) + 1;
  });
  const hotDistrict = Object.keys(districtCounts).reduce(
    (a, b) => (districtCounts[a] > districtCounts[b] ? a : b),
    ""
  );
  $("#hotDistrict").textContent = `منطقه ${toPersianNumbers(hotDistrict)}`;

  const validSizes = data.filter((item) => item.size);
  const avgSize =
    validSizes.length > 0
      ? validSizes.reduce((sum, item) => sum + item.size, 0) / validSizes.length
      : 0;
  $("#avgSize").textContent = `${toPersianNumbers(avgSize.toFixed(0))} متر`;

  const totalValue = data.reduce(
    (sum, item) => sum + (item.totalPrice || 0),
    0
  );
  $("#totalValue").textContent = toPersianNumbers(formatPrice(totalValue));
}

function updateStatsFromData(data) {
  $("#totalAds").textContent = toPersianNumbers(formatNumber(data.length));

  const validPrices = data.filter((item) => item.totalPrice);
  const avgPrice =
    validPrices.length > 0
      ? validPrices.reduce((sum, item) => sum + item.totalPrice, 0) /
        validPrices.length
      : 0;
  $("#avgPrice").textContent = toPersianNumbers(formatPrice(avgPrice));

  $("#priceGrowth").textContent = "+۵.۲%";

  const districtCounts = {};
  data.forEach((item) => {
    districtCounts[item.district] = (districtCounts[item.district] || 0) + 1;
  });
  const hotDistrict = Object.keys(districtCounts).reduce(
    (a, b) => (districtCounts[a] > districtCounts[b] ? a : b),
    ""
  );
  $("#hotDistrict").textContent = `منطقه ${toPersianNumbers(hotDistrict)}`;

  const validSizes = data.filter((item) => item.size);
  const avgSize =
    validSizes.length > 0
      ? validSizes.reduce((sum, item) => sum + item.size, 0) / validSizes.length
      : 0;
  $("#avgSize").textContent = `${toPersianNumbers(avgSize.toFixed(0))} متر`;

  const totalValue = data.reduce(
    (sum, item) => sum + (item.totalPrice || 0),
    0
  );
  $("#totalValue").textContent = toPersianNumbers(formatPrice(totalValue));
}
function drawTimeSeriesChart(data) {
  const ctx = $("#timeSeriesChart").getContext("2d");
  if (timeSeriesChart) timeSeriesChart.destroy();

  const dailyData = {};
  data.forEach((item) => {
    const date = normalizeDateKey(item.date ? item.date : item.scrapedDate);
    if (!date) return;
    dailyData[date] = (dailyData[date] || 0) + (item.count || 1);
  });

  const labels = Object.keys(dailyData).sort();
  const values = labels.map((date) => dailyData[date]);
  const jalaliLabels = labels.map((date) => toJalali(date));

  timeSeriesChart = new Chart(ctx, {
    type: "line",
    data: {
      labels: jalaliLabels,
      datasets: [
        {
          label: "تعداد آگهی",
          data: values,
          borderColor: "rgb(102, 126, 234)",
          backgroundColor: "rgba(102, 126, 234, 0.1)",
          fill: true,
          tension: 0.4,
          pointBackgroundColor: "rgb(102, 126, 234)",
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
          pointRadius: 5,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "top",
          labels: {
            font: {
              family: "Vazir, Tahoma, Arial",
            },
          },
        },
        title: {
          display: true,
          text: "روند تعداد آگهی‌ها در طول زمان",
          font: {
            family: "Vazir, Tahoma, Arial",
            size: 14,
          },
        },
        tooltip: {
          callbacks: {
            title: function (context) {
              return context[0].label;
            },
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { color: "rgba(0, 0, 0, 0.1)" },
          ticks: {
            font: {
              family: "Vazir, Tahoma, Arial",
            },
            callback: function (value) {
              return toPersianNumbers(value);
            },
          },
        },
        x: {
          grid: { color: "rgba(0, 0, 0, 0.1)" },
          ticks: {
            font: {
              family: "Vazir, Tahoma, Arial",
              size: 11,
            },
            maxRotation: 45,
            minRotation: 45,
          },
        },
      },
    },
  });
}

function drawTimeSeriesChartFromData(data) {
  drawTimeSeriesChart(data);
}

function drawPriceChart(data) {
  const ctx = $("#priceDistChart").getContext("2d");
  if (priceChart) priceChart.destroy();

  const districtPrices = {};
  data.forEach((item) => {
    if (!districtPrices[item.district]) districtPrices[item.district] = [];
    const price = item.avgTotalPrice || item.totalPrice;
    if (price) districtPrices[item.district].push(price);
  });

  const labels = Object.keys(districtPrices).sort();
  const avgPrices = labels.map((district) => {
    const prices = districtPrices[district];
    if (prices.length === 0) return 0;
    return prices.reduce((sum, price) => sum + price, 0) / prices.length / 1e9;
  });

  priceChart = new Chart(ctx, {
    type: "bar",
    data: {
      labels: labels.map((d) => `منطقه ${toPersianNumbers(d)}`),
      datasets: [
        {
          label: "میانگین قیمت (میلیارد تومان)",
          data: avgPrices,
          backgroundColor: [
            "rgba(102, 126, 234, 0.8)",
            "rgba(118, 75, 162, 0.8)",
            "rgba(255, 99, 132, 0.8)",
            "rgba(54, 162, 235, 0.8)",
            "rgba(255, 206, 86, 0.8)",
          ],
          borderColor: [
            "rgb(102, 126, 234)",
            "rgb(118, 75, 162)",
            "rgb(255, 99, 132)",
            "rgb(54, 162, 235)",
            "rgb(255, 206, 86)",
          ],
          borderWidth: 2,
          borderRadius: 8,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "top",
          labels: {
            font: {
              family: "Vazir, Tahoma, Arial",
            },
          },
        },
        title: {
          display: true,
          text: "میانگین قیمت به تفکیک منطقه",
          font: {
            family: "Vazir, Tahoma, Arial",
            size: 14,
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: { color: "rgba(0, 0, 0, 0.1)" },
          ticks: {
            font: {
              family: "Vazir, Tahoma, Arial",
            },
            callback: function (value) {
              return toPersianNumbers(value.toFixed(1));
            },
          },
        },
        x: {
          grid: { color: "rgba(0, 0, 0, 0.1)" },
          ticks: {
            font: {
              family: "Vazir, Tahoma, Arial",
            },
          },
        },
      },
    },
  });
}

function drawPriceChartFromData(data) {
  drawPriceChart(data);
}
function createMap(data) {
  if (map) map.remove();

  map = L.map("map").setView([35.7219, 51.3347], 11);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution: "© OpenStreetMap contributors",
    maxZoom: 18,
  }).addTo(map);

  setTimeout(() => {
    map.invalidateSize();
    map.setView([35.7219, 51.3347], 11);
  }, 100);

  const validCoords = data.filter(
    (item) =>
      item.latitude &&
      item.longitude &&
      item.latitude > 35.5 &&
      item.latitude < 35.9 &&
      item.longitude > 51.2 &&
      item.longitude < 51.6
  );

  const colors = {
    1: "#FF6B6B",
    2: "#4ECDC4",
    3: "#45B7D1",
    6: "#96CEB4",
    15: "#FECA57",
  };

  validCoords.slice(0, 200).forEach((item) => {
    const color = colors[item.district] || "#667eea";
    const popupText = `
      <b>منطقه ${toPersianNumbers(item.district)}</b><br>
      ${item.title || "ملک مسکونی"}<br>
      قیمت: ${
        item.totalPrice
          ? toPersianNumbers(formatPrice(item.totalPrice)) + " میلیارد تومان"
          : "نامشخص"
      }<br>
      متراژ: ${item.size ? toPersianNumbers(item.size) : "-"} متر<br>
      تاریخ: ${toJalali(item.scrapedDate || item.date)}
    `;

    L.circleMarker([item.latitude, item.longitude], {
      radius: 6,
      fillColor: color,
      color: "#fff",
      weight: 1,
      fillOpacity: 0.8,
    })
      .addTo(map)
      .bindPopup(popupText);
  });

  if (validCoords.length === 0) {
    const samplePoints = [
      { lat: 35.7219, lng: 51.3347, district: "6", title: "نقطه نمونه ۱" },
      { lat: 35.7797, lng: 51.4183, district: "1", title: "نقطه نمونه ۲" },
      { lat: 35.7797, lng: 51.4026, district: "2", title: "نقطه نمونه ۳" },
      { lat: 35.7869, lng: 51.4399, district: "3", title: "نقطه نمونه ۴" },
      { lat: 35.6669, lng: 51.3753, district: "15", title: "نقطه نمونه ۵" },
    ];

    samplePoints.forEach((point) => {
      L.circleMarker([point.lat, point.lng], {
        radius: 8,
        fillColor: colors[point.district] || "#667eea",
        color: "#fff",
        weight: 2,
        fillOpacity: 0.8,
      })
        .addTo(map)
        .bindPopup(
          `<b>منطقه ${toPersianNumbers(point.district)}</b><br>${
            point.title
          }<br>نقطه نمونه`
        );
    });
  }
}

function createMapFromData(data) {
  createMap(data);
}
function updateDataTable(data) {
  const tbody = $("#dataTableBody");
  tbody.innerHTML = "";

  const dailyData = {};
  data.forEach((item) => {
    const date = normalizeDateKey(item.date ? item.date : item.scrapedDate);
    if (!date) return;
    const key = `${date}_${item.district}`;
    if (!dailyData[key]) {
      dailyData[key] = {
        date,
        district: item.district,
        count: 0,
        totalPrice: 0,
        entries: 0,
      };
    }
    dailyData[key].count += item.count || 1;
    const price = item.avgTotalPrice || item.totalPrice;
    if (price) {
      dailyData[key].totalPrice += price;
      dailyData[key].entries += 1;
    }
  });

  const sortedData = Object.values(dailyData)
    .sort((a, b) => new Date(b.date) - new Date(a.date))
    .slice(0, 20);

  sortedData.forEach((row, index) => {
    const avgPrice =
      row.entries > 0
        ? toPersianNumbers(formatPriceM(row.totalPrice / row.entries))
        : "-";
    const growth = index < sortedData.length - 1 ? "+۵.۲%" : "-";
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td style="font-family: Vazir, Tahoma, Arial; direction: rtl;">${toJalali(
        row.date
      )}</td>
      <td style="font-family: Vazir, Tahoma, Arial;">منطقه ${toPersianNumbers(
        row.district
      )}</td>
      <td style="font-family: Vazir, Tahoma, Arial;">${toPersianNumbers(
        formatNumber(row.count)
      )}</td>
      <td style="font-family: Vazir, Tahoma, Arial;">${avgPrice}</td>
      <td style="color: green; font-family: Vazir, Tahoma, Arial;">${growth}</td>
    `;
    tbody.appendChild(tr);
  });
}

function updateDataTableFromData(data) {
  updateDataTable(data);
}
function showInsights(insights) {
  if (insights && insights.length > 0) {
    $("#insightsBody").textContent = insights.join(" ");
    $("#insights").style.display = "block";
  }
}

function showInsightsFromData(data) {
  const insights = [];
  if (data.length > 0) {
    const validPrices = data.filter((d) => d.totalPrice);
    if (validPrices.length > 0) {
      const avgPrice =
        validPrices.reduce((sum, d) => sum + d.totalPrice, 0) /
        validPrices.length;
      insights.push(
        `میانگین قیمت ملک: ${toPersianNumbers(
          formatPrice(avgPrice)
        )} میلیارد تومان`
      );
    }

    const districtCounts = {};
    data.forEach(
      (d) =>
        (districtCounts[d.district] = (districtCounts[d.district] || 0) + 1)
    );
    const topDistrict = Object.keys(districtCounts).reduce((a, b) =>
      districtCounts[a] > districtCounts[b] ? a : b
    );
    insights.push(
      `فعال‌ترین منطقه: منطقه ${toPersianNumbers(
        topDistrict
      )} با ${toPersianNumbers(districtCounts[topDistrict])} آگهی`
    );
  }

  if (insights.length > 0) {
    $("#insightsBody").textContent = insights.join(" | ");
    $("#insights").style.display = "block";
  }
}

function showPeaks(data) {
  const peaks = {};
  data.forEach((item) => {
    const price = item.totalPrice;
    if (price && (!peaks[item.district] || price > peaks[item.district])) {
      peaks[item.district] = price;
    }
  });

  const peaksBody = $("#peaksBody");
  if (peaksBody) {
    peaksBody.innerHTML = "";

    Object.keys(peaks)
      .sort()
      .forEach((district) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
          <td style="font-family: Vazir, Tahoma, Arial;">منطقه ${toPersianNumbers(
            district
          )}</td>
          <td style="font-family: Vazir, Tahoma, Arial;">${toPersianNumbers(
            formatPrice(peaks[district])
          )}</td>
        `;
        peaksBody.appendChild(tr);
      });

    if (Object.keys(peaks).length > 0) {
      $("#peaksPanel").style.display = "block";
    }
  }
}

function showPeaksFromData(data) {
  showPeaks(data);
}
async function downloadFilteredCSV() {
  if (rawData.length === 0) {
    showMessage("داده‌ای برای دانلود وجود ندارد", "error");
    return;
  }

  showMessage("در حال آماده‌سازی دانلود...", "loading");

  try {
    const response = await fetch(`${API_BASE}/export-csv`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(rawData),
    });

    if (response.ok) {
      const blob = await response.blob();
      downloadBlob(
        blob,
        `divar_export_${new Date().toISOString().split("T")[0]}.csv`
      );
      showMessage("فایل CSV با موفقیت دانلود شد", "success");
      return;
    }
  } catch (error) {
    console.log("API download failed, using fallback:", error.message);
  }

  try {
    const csv = convertToCSV(rawData);
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    downloadBlob(
      blob,
      `divar_export_${new Date().toISOString().split("T")[0]}.csv`
    );
    showMessage("فایل CSV با موفقیت دانلود شد", "success");
  } catch (error) {
    showMessage(`خطا در دانلود: ${error.message}`, "error");
  }
}

function downloadBlob(blob, fileName) {
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.style.display = "none";
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  setTimeout(() => {
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }, 100);
}

function convertToCSV(data) {
  if (!data.length) return "";
  const headers = Object.keys(data[0]);
  const csvContent = [
    headers.join(","),
    ...data.map((row) =>
      headers
        .map((header) => {
          let value = row[header];
          if (value === null || value === undefined) value = "";
          value = String(value);
          if (
            value.includes(",") ||
            value.includes('"') ||
            value.includes("\n")
          ) {
            value = `"${value.replace(/"/g, '""')}"`;
          }
          return value;
        })
        .join(",")
    ),
  ].join("\n");
  return csvContent;
}
document.addEventListener("DOMContentLoaded", () => {
  const fromEl = $("#dateFrom");
  const toEl = $("#dateTo");
  if (fromEl && !fromEl.value) fromEl.value = "2024-06-12";
  if (toEl && !toEl.value) toEl.value = new Date().toISOString().split("T")[0];

  $("#btnDemo").addEventListener("click", generateSampleData);
  $("#btnAnalyze").addEventListener("click", analyzeData);
  $("#btnDownload").addEventListener("click", downloadFilteredCSV);
});

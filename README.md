# 📆 BookingSystem

Et komplet booking- og kundestyringssystem lavet med C# (.NET), FullCalendar, Bootstrap og lokal SQL Server. Understøtter både konsol- og webversion.

## 💻 Funktioner

### ✅ Web-UI (index.html + script.js)
- Dynamisk kalender med [FullCalendar](https://fullcalendar.io/)
- Mørkt tema med Bootstrap 5
- Opret og slet bookinger
- Opret kunder
- Søgning (ressource, bookingID, customerID)
- Søgning med både Enter og klik
- Visning i kort og kalender

### ✅ Konsolapplikation
- CRUD på bookinger
- CRUD på kunder
- Generér CSV-rapport
- Generér PDF-rapport (med [QuestPDF](https://www.questpdf.com/))
- Send e-mail notifikationer

### ✅ Teknologier
- .NET 8 (Console + Minimal API)
- FullCalendar 6.1.8
- Bootstrap 5.3
- SQL Server Express (lokal)
- Vanilla JavaScript
- QuestPDF (PDF eksport)
- SMTP (e-mail notifikation)

---

## ⚙️ Kom i gang

### 1. Start backend (.NET)
```bash
dotnet run

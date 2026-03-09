# 🚕 NYC Taxi ETL

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Database Schema & Indexes](#-database-schema--indexes)
- [Results](#-results)
- [Assumptions](#-assumptions)
- [Scaling to 10 GB+ CSV Files](#-scaling-to-10-gb-csv-files)

---

## 🔍 Overview

This tool reads a CSV file of NYC taxi trips, cleans and transforms the data, removes duplicates, and bulk-inserts records into a SQL Server table. Duplicate rows are written to a separate `duplicates.csv` file.

**Key features:**
- ⚡ Bulk insertion via `SqlBulkCopy` (configurable batch size)
- 🔁 Deduplication based on `pickup_datetime` + `dropoff_datetime` + `passenger_count`
- 🌍 Timezone conversion: EST → UTC
- 🧹 Data sanitization: whitespace trimming, flag normalization (`Y`/`N` → `Yes`/`No`)
- 📊 Optimized indexes for the most common query patterns

---

## 🛠 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full instance)

---

## 🚀 Getting Started

### 1. Create the database and table

**Option A — SQL script:**
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -i scripts/create_database.sql
```

**Option B — EF Core migration:**
```bash
dotnet ef database update --project NycTaxiEtl
```

### 2. Place the input CSV

Put your CSV file at the path defined in `appsettings.json` → `TaxiEtl:InputCsvPath`  
(default: `data/taxi_rides.csv`)

### 3. Run

```bash
dotnet run --project NycTaxiEtl
```

---

## ⚙️ Configuration

All settings live in `appsettings.json`:

| Setting | Description | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | LocalDB |
| `TaxiEtl:InputCsvPath` | Path to the input CSV file | `data/taxi_rides.csv` |
| `TaxiEtl:DuplicatesCsvPath` | Path to write duplicate rows | `output/duplicates.csv` |
| `TaxiEtl:BatchSize` | Rows per bulk insert batch | `50000` |
| `TaxiEtl:BulkCopyTimeoutSeconds` | SqlBulkCopy timeout | `600` |
| `TaxiEtl:SourceTimeZoneId` | Timezone of input data | `Eastern Standard Time` |

---

## 🗄 Database Schema & Indexes

The table is optimized for four query patterns from the requirements:

| Query | Index |
|---|---|
| AVG `tip_amount` grouped by `PULocationID` | `IX_TaxiRides_PULocationID` |
| TOP 100 by `trip_distance` | `IX_TaxiRides_TripDistance_Desc` _(descending)_ |
| TOP 100 by time spent traveling | `IX_TaxiRides_Pickup_Dropoff` _(composite)_ |
| Search with `PULocationID` as condition | `IX_TaxiRides_PULocationID` |

---

## 📊 Results

After running the ETL on the provided dataset:

| Metric | Value |
|---|---|
| ✅ Rows inserted | **29 840** |
| 🔁 Duplicates removed | **111** |
| ⚠️ Invalid rows skipped | **49** |
| 📦 Total processed | **30 000** |

### Console output

<img width="1115" height="628" alt="Screenshot 2026-03-09 174355" src="https://github.com/user-attachments/assets/3755f7f6-6fa5-4f2d-b868-79eff5959523" />

### Data in database

<img width="1137" height="410" alt="Screenshot 2026-03-09 174423" src="https://github.com/user-attachments/assets/807d5f39-c6ef-4ff8-852a-d9cab2b74bf3" />

### Verification queries

```sql
-- Total rows in table
SELECT COUNT(*) FROM TaxiRides;

-- PULocationID with highest average tip
SELECT TOP 1 PULocationID, AVG(TipAmount) AS AvgTip
FROM TaxiRides
GROUP BY PULocationID
ORDER BY AvgTip DESC;

-- Top 100 longest trips by distance
SELECT TOP 100 * FROM TaxiRides ORDER BY TripDistance DESC;

-- Top 100 longest trips by duration
SELECT TOP 100 *,
  DATEDIFF(MINUTE, PickupDatetimeUtc, DropoffDatetimeUtc) AS DurationMinutes
FROM TaxiRides
ORDER BY DurationMinutes DESC;
```

---

## 📌 Assumptions

- Duplicate detection uses the combination of `tpep_pickup_datetime`, `tpep_dropoff_datetime`, and `passenger_count` as specified.
- `store_and_fwd_flag` values other than `Y` / `N` are treated as invalid and the row is skipped.
- All datetime values in the source CSV are in Eastern Standard Time and are converted to UTC on insert.
- The `Id` column uses `IDENTITY` since it is not present in the source data.
- Rows with unparseable, out-of-range, or missing numeric fields are skipped (counted as invalid).

---

## 📈 Scaling to 10 GB+ CSV Files

For a 10 GB input file, the following changes would be necessary:

1. **Parallel pipeline with `Channel<T>`** — Use a producer–consumer pattern: one thread reads/parses CSV rows while multiple consumer threads handle transformation, deduplication, and bulk insertion concurrently.

2. **Database-side deduplication** — The in-memory `HashSet` will consume too much RAM at scale. Replace it with a staging table + `INSERT ... WHERE NOT EXISTS`, a `MERGE` statement, or a Bloom filter for a fast probabilistic first-pass with exact dedup on the DB side.

3. **Minimal logging** — Switch the database to `BULK_LOGGED` recovery model during import and use the `TABLOCK` hint in `SqlBulkCopy` to enable minimal logging, dramatically improving insert throughput.

4. **Drop and rebuild indexes** — Remove non-clustered indexes before the bulk load and recreate them afterward to eliminate per-batch index maintenance overhead.

5. **File partitioning** — Pre-scan the file to find newline offsets and split it into partitions that can be read in parallel, avoiding the sequential I/O bottleneck of a single `StreamReader`.

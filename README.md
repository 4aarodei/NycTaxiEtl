# NYC Taxi ETL

Console ETL application that imports NYC Yellow Taxi trip data from a CSV file into SQL Server.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or full instance)

## How to Run

1. **Create the database and table** using the provided SQL script:

   ```
   sqlcmd -S (localdb)\MSSQLLocalDB -i scripts/create_database.sql
   ```

   Or apply the EF Core migration:

   ```
   dotnet ef database update --project NycTaxiEtl
   ```

2. **Place the input CSV** at the path configured in `appsettings.json` ? `TaxiEtl:InputCsvPath` (default: `data/taxi_rides.csv`).

3. **Run the application:**

   ```
   dotnet run --project NycTaxiEtl
   ```

## Configuration

All settings are in `appsettings.json`:

| Setting | Description | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | LocalDB |
| `TaxiEtl:InputCsvPath` | Path to the input CSV file | `data/taxi_rides.csv` |
| `TaxiEtl:DuplicatesCsvPath` | Path to write duplicate rows | `output/duplicates.csv` |
| `TaxiEtl:BatchSize` | Rows per bulk insert batch | `50000` |
| `TaxiEtl:BulkCopyTimeoutSeconds` | SqlBulkCopy timeout | `600` |
| `TaxiEtl:SourceTimeZoneId` | Timezone of input data | `Eastern Standard Time` |

## Results

After running the ETL pipeline on the dataset:

Rows inserted into TaxiRides table: <ACTUAL_COUNT>

Duplicate rows written to duplicates.csv: <ACTUAL_DUP_COUNT>

## Database Schema & Indexing Strategy

The table is optimized for the following queries specified in the requirements:

| Query | Index |
|---|---|
| AVG `tip_amount` grouped by `PULocationID` | `IX_TaxiRides_PULocationID` |
| TOP 100 longest fares by `trip_distance` | `IX_TaxiRides_TripDistance_Desc` (descending) |
| TOP 100 longest fares by time spent | `IX_TaxiRides_Pickup_Dropoff` (composite) |
| Search with `PULocationID` as part of conditions | `IX_TaxiRides_PULocationID` |

## Assumptions

- Duplicate detection is based on a combination of `tpep_pickup_datetime`, `tpep_dropoff_datetime`, and `passenger_count` as specified in the requirements.
- The `store_and_fwd_flag` values other than `Y` / `N` are treated as invalid rows and skipped.
- All datetime values in the source CSV are in Eastern Standard Time (EST/EDT) and are converted to UTC on insert.
- Rows with unparseable or missing numeric fields are skipped (counted as invalid).

## Scaling to 10 GB+ CSV Files

For a 10 GB input file, the following changes would be necessary:

1. **Parallel pipeline with `Channel<T>`**: Use a producer–consumer pattern where one thread reads and parses CSV rows, while multiple consumer threads handle transformation, deduplication, and bulk insertion concurrently.
2. **Database-side deduplication**: The in-memory `HashSet` will consume too much RAM at scale. Replace it with a staging table + `INSERT ... WHERE NOT EXISTS` or use a `MERGE` statement. Alternatively, use a Bloom filter for a fast probabilistic first-pass, with exact dedup on the database side.
3. **Minimal logging**: Switch the database to `BULK_LOGGED` recovery model during import and use `TABLOCK` hint in `SqlBulkCopy` to enable minimal logging, which dramatically improves insert throughput.
4. **Drop and rebuild indexes**: Remove non-clustered indexes before the bulk load and recreate them afterward to eliminate per-batch index maintenance overhead.
5. **File partitioning**: Pre-scan the file to find newline offsets and split it into partitions that can be read in parallel, avoiding the sequential I/O bottleneck of a single `StreamReader`.

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NycTaxiEtlDatabase')
    CREATE DATABASE NycTaxiEtlDatabase;
GO

USE NycTaxiEtlDatabase;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaxiRides')
BEGIN
    CREATE TABLE [dbo].[TaxiRides] (
        [PickupDatetimeUtc]  DATETIME2      NOT NULL,
        [DropoffDatetimeUtc] DATETIME2      NOT NULL,
        [PassengerCount]     INT            NOT NULL,
        [TripDistance]       DECIMAL(10,2)  NOT NULL,
        [StoreAndFwdFlag]    NVARCHAR(3)    NOT NULL,
        [PULocationID]       INT            NOT NULL,
        [DOLocationID]       INT            NOT NULL,
        [FareAmount]         DECIMAL(10,2)  NOT NULL,
        [TipAmount]          DECIMAL(10,2)  NOT NULL,

        CONSTRAINT [PK_TaxiRides]
            PRIMARY KEY ([PickupDatetimeUtc], [DropoffDatetimeUtc], [PassengerCount])
    );

    -- Query: Find PULocationId with highest average tip_amount + search with PULocationId
    CREATE NONCLUSTERED INDEX [IX_TaxiRides_PULocationID]
        ON [dbo].[TaxiRides] ([PULocationID]);

    -- Query: Top 100 longest fares by trip_distance
    CREATE NONCLUSTERED INDEX [IX_TaxiRides_TripDistance_Desc]
        ON [dbo].[TaxiRides] ([TripDistance] DESC);

    -- Query: Top 100 longest fares by time spent traveling
    CREATE NONCLUSTERED INDEX [IX_TaxiRides_Pickup_Dropoff]
        ON [dbo].[TaxiRides] ([PickupDatetimeUtc], [DropoffDatetimeUtc]);
END
GO

# IBNR Calculator

A lightweight .NET 8 console utility that mirrors the essential chain-ladder workflow seen in tools like WTW ResQ. It can ingest claim transactions from an Access database, build triangles at different origin and development grains, and calculate ultimates/IBNR with selectable link ratios.

## Features
- Load transactions from an Access database (or the bundled sample data for quick demos).
- Choose accident year or underwriting year as the origin basis.
- Switch origin grain between yearly and quarterly and choose any development period length (in months).
- Automatically build incremental and cumulative paid triangles.
- Compute chain-ladder ultimates and IBNR per origin with the ability to include/exclude individual development link ratios.

## Expected data shape
The Access table should expose the following columns (names are configurable through CLI flags):
- `AccidentDate` (date)
- `UnderwritingDate` (date)
- `PaymentDate` (date)
- `IncrementalPaid` (numeric/decimal)

## Running the tool
> Note: `System.Data.OleDb` requires the ACE provider available on Windows. On non-Windows environments you can still use the `--sample` flag to exercise the calculations.

```bash
# From the repository root
# Using an Access database
# dotnet run -- --database /path/to/data.accdb --table Claims --origin accident --origin-grain year --development-months 12

# With bundled sample data (no Access database required)
dotnet run -- --sample --origin accident --origin-grain year --development-months 12
```

### Key options
- `--origin <accident|underwriting>`: switch origin basis.
- `--origin-grain <year|quarter>`: change origin aggregation.
- `--development-months <n>`: set development period size.
- `--use-steps <list>`: comma-separated development step indices (e.g. `0,1,2`) to include in the IBNR projection. Steps not listed are treated as neutral (factor of 1).
- Column overrides: `--accident-column`, `--underwriting-column`, `--payment-column`, `--amount-column`.
- `--sample`: run with bundled synthetic data.

### Output
- Incremental and cumulative triangles with the chosen grains.
- Selected link ratios per development step (reflecting any exclusions).
- Origin-level latest cumulative, projected ultimate, and IBNR.

## Extending
The code is organized into small calculation and data source components (`src/Calculations` and `src/Services`) so you can plug in alternate data sources or add more reserving methods alongside the chain ladder implementation.

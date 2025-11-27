# IBNR Calculator

A lightweight .NET 10 console utility that mirrors the essential chain-ladder workflow seen in tools like WTW ResQ. It can ingest claim transactions from an Access database, build triangles at different origin and development grains, and calculate ultimates/IBNR with selectable link ratios. A WPF/XAML template is included to host these capabilities in a desktop UI.

## Features
- Load transactions from an Access database (or the bundled sample data for quick demos).
- Choose accident year or underwriting year as the origin basis.
- Switch origin grain between yearly and quarterly and choose any development period length (in months).
- Automatically build incremental and cumulative paid triangles.
- Compute chain-ladder ultimates and IBNR per origin with the ability to include/exclude individual development link ratios.

## Expected data shape
The Access table should expose the following logical fields (map them to your column names with CLI flags):
- `AccidentDate` (date/text)
- `UnderwritingDate` (date/text)
- `PaymentDate` (date/text)
- `IncrementalPaid` (numeric/decimal)

## Running the tool
> Note: `System.Data.OleDb` requires the ACE provider available on Windows. On non-Windows environments you can still use the `--sample` flag to exercise the calculations.

```bash
# From the repository root
# Using an Access database
# dotnet run -- --database /path/to/data.accdb --table Claims --origin accident --origin-grain year --development-months 12 \
#   --accident-column AY --underwriting-column UW --payment-column PaidDate --amount-column Payment \
#   --date-format yyyyQQ   # example of parsing a text quarter code like 200601 => 2006-01-01

# With bundled sample data (no Access database required)
dotnet run -- --sample --origin accident --origin-grain year --development-months 12
```

### Key options
- `--origin <accident|underwriting>`: switch origin basis.
- `--origin-grain <year|quarter>`: change origin aggregation.
- `--development-months <n>`: set development period size.
- `--use-steps <list>`: comma-separated development step indices (e.g. `0,1,2`) to include in the IBNR projection. Steps not listed are treated as neutral (factor of 1).
- Column overrides: `--accident-column`, `--underwriting-column`, `--payment-column`, `--amount-column`.
- `--date-format <pattern>`: interpret Access date columns using the supplied pattern (e.g. `yyyyMMdd` or `yyyyQQ` where `200601` is parsed as 2006 Q1).
- `--sample`: run with bundled synthetic data.

### Output
- Incremental and cumulative triangles with the chosen grains.
- Selected link ratios per development step (reflecting any exclusions).
- Origin-level latest cumulative, projected ultimate, and IBNR.

## WPF template
- A ready-to-bind XAML shell lives at `templates/MainWindow.xaml`. Its `DataContext` targets `ReservingViewModel`, which already orchestrates loading Access data (including column overrides and custom date formats), building triangles, and calculating chain-ladder ultimates.
- Bind the template into a WPF application by copying the XAML and code-behind, keeping the `IBNRCalculator.ViewModels` namespace available. The `Calculate` button triggers `RunCommand`, which pulls the user-specified mappings, date format, origin/development selections, and allowed link-ratio steps before updating the on-screen grids.

## Extending
The code is organized into small calculation and data source components (`src/Calculations` and `src/Services`) so you can plug in alternate data sources or add more reserving methods alongside the chain ladder implementation.

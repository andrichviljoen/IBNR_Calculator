# IBNR Calculator

A lightweight .NET 10 desktop utility that mirrors the essential chain-ladder workflow seen in tools like WTW ResQ. It can ingest claim transactions from an Access database, build triangles at different origin and development grains, and calculate ultimates/IBNR with selectable link ratios. A WPF/XAML shell is included so running the project immediately opens a UI (with a `--console` switch to use the previous terminal experience).

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

### WPF UI (default)
```bash
# Restore and build will happen automatically when you run
 dotnet run

# Pass any arguments (such as --sample) and the UI will still launch
 dotnet run -- --sample
```

### Console mode
```bash
# Using an Access database
 dotnet run -- --console --database /path/to/data.accdb --table Claims --origin accident --origin-grain year --development-months 12 \
   --accident-column AY --underwriting-column UW --payment-column PaidDate --amount-column Payment \
   --date-format yyyyQQ   # example of parsing a text quarter code like 200601 => 2006-01-01

# With bundled sample data (no Access database required)
 dotnet run -- --console --sample --origin accident --origin-grain year --development-months 12
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

## WPF experience
- `App.xaml` starts the WPF window by default; use `--console` if you want the command-line output instead.
- `MainWindow.xaml` binds directly to `ReservingViewModel`, which orchestrates loading Access data (including column overrides and custom date formats), building triangles, and calculating chain-ladder ultimates.
- The `Calculate` button triggers `RunCommand`, which pulls the user-specified mappings, date format, origin/development selections, and allowed link-ratio steps before updating the on-screen grids.

## Using Visual Studio
1) **Open the project**
   - Clone the repository and open `IBNRCalculator.csproj` in Visual Studio 2022 (17.10+) or newer with .NET 10 installed.
2) **Restore and build**
   - Visual Studio performs package restore automatically when you build or run. The project also triggers a restore if `project.assets.json` is missing, so normal builds should recover from missing packages without extra steps.
   - If you want to force restore first, right-click the solution and choose **Restore NuGet Packages** (or run `dotnet restore`).
   - Build with `Build > Build Solution` (or `Ctrl+Shift+B`).
3) **Run the WPF UI (default)**
   - Set `IBNRCalculator` as the startup project.
   - Press `F5` (Debug) or `Ctrl+F5` (Start Without Debugging) and the window will open. Use the fields to browse for an Access database, map columns, set date format, origin/development grains, and optional allowed link-ratio steps. Click **Calculate** to update the link ratios and IBNR grid.
4) **Run the console experience (optional)**
   - In `Project > Properties > Debug`, set application arguments to start with `--console` followed by your flags (database path, table name, column overrides, `--date-format`, `--origin`, `--origin-grain`, `--development-months`, `--use-steps`).
   - Run with `F5`/`Ctrl+F5` to view the triangle/IBNR output in the terminal pane.

## Extending
The code is organized into small calculation and data source components (`src/Calculations` and `src/Services`) so you can plug in alternate data sources or add more reserving methods alongside the chain ladder implementation.

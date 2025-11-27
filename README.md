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

## Using Visual Studio
1) **Open the project**
   - Clone the repository and open `IBNRCalculator.csproj` in Visual Studio 2022 (17.10+) or newer with .NET 10 installed.
2) **Restore and build**
   - First restore packages (required to generate `obj/project.assets.json`): choose `Project > Manage NuGet Packages > Restore` (or right-click the solution and select **Restore NuGet Packages**). You can also run `dotnet restore` from the Package Manager Console in Visual Studio.
   - Then use `Build > Build Solution` (or `Ctrl+Shift+B`) to compile the console app.
3) **Run the console experience**
   - Set the project as the startup project.
   - Open `Project > Properties > Debug` and add your command-line arguments (e.g., database path, table name, column overrides, `--date-format`, `--origin`, `--origin-grain`, `--development-months`, `--use-steps`).
   - Press `F5` (Debug) or `Ctrl+F5` (Start Without Debugging) to run and view the triangle/IBNR output in the terminal pane.
4) **Use the WPF template**
   - Create a new WPF App project targeting .NET 10 in the same solution, then add a project reference to this calculator library.
   - Copy `templates/MainWindow.xaml` into the WPF project (replacing its default `MainWindow.xaml`) and ensure the `DataContext` is set to `new ReservingViewModel()` in the code-behind.
   - Build and run the WPF app; the UI will let you browse for an Access file, map columns, supply a date format (including quarter codes like `yyyyQQ`), choose origin/development grains, and select link ratios before calculating IBNR.

## Extending
The code is organized into small calculation and data source components (`src/Calculations` and `src/Services`) so you can plug in alternate data sources or add more reserving methods alongside the chain ladder implementation.

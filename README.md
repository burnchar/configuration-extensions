# configuration-extensions
Extensions to IConfiguration, such as visualizers to help understand and debug your program configuration.

## usage
- Add ConfigurationExtensions.cs and ConfigurationVisualizer.cs do your project
- Change the namespace to suit your program
- Call the `.Visualize()` or `.VisualizeSimple()` extension on your configuration:
```c#
var builder = Host.CreateApplicationBuilder(args);
var config = app.Services.GetRequiredService<IConfiguration>();
var app = builder.Build();

// Graphical representation using unicode icons
Console.WriteLine(config.Visualize());

// ASCII (no icons) visualization for basic terminals
Console.WriteLine(config.VisualizeSimple());

// Add "excludeVendorSections: false" to show everything (some noise is excluded by default)
Console.WriteLine(config.Visualize(excludeVendorSections: false));

// Add "excludeVendorSections: false" to show everything (some noise is excluded by default)
Console.WriteLine(config.Visualize(excludeVendorSections: false));

// Ignore keys starting with specific strings (overrides default ignores)
var ignorePaths = new List<string> { "ConnectionStrings", "contentRoot" };
Console.WriteLine(config.Visualize(ignorePaths));
```

## Example visualization (dark theme console)
![image](https://github.com/user-attachments/assets/f65a5a2a-437c-42ea-ac60-447c36d7f79f)

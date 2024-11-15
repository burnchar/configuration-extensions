/*
* Configuration Visualizer
* Creates a visual of your program configuration (IConfiguration)
* (C) 2024 Charles Burns
* MIT license
* This code is meant as a small public service for use by the fine C# community.
*/


using System.Text;
using Microsoft.Extensions.Configuration;

namespace YourNamespace.ExtensionMethodLogic;

/// <summary>Builds a text visualization of an IConfiguration object</summary>
internal static class ConfigurationVisualizer
{
	private readonly static HashSet<string> DefaultVendorPrefixes = new(StringComparer.OrdinalIgnoreCase) {
		"Microsoft",
		"System",
		"Windows",
		"Logging",
		"AllowedHosts",
		"Authentication",
		"DataProtection",
		"Routes"
	};


	/// <summary>The visualization uses several symbols. This method prints the legend of those symbols.</summary>
	/// <param name="stringBuilder">The visualization is build into this</param>
	/// <param name="symbols">The symbols for which to generate a legend</param>
	private static void AppendLegend(StringBuilder stringBuilder, IVisualizationSymbols symbols)
	{
		stringBuilder.AppendLine("\nConfiguration Details:");
		stringBuilder.AppendLine("====================");
		stringBuilder.AppendLine($"{symbols.Section} = Section");
		stringBuilder.AppendLine($"{symbols.Provider} = Configuration Source");
		stringBuilder.AppendLine($"{symbols.Active} = Active Value");
		stringBuilder.AppendLine($"{symbols.Overridden} = Overridden Value");
		stringBuilder.AppendLine("Indentation indicates nesting level");
	}


	/// <summary>Adds a configuration providers list to the visualization. Providers could be data files with configuration information, such as appsettings.json, or they might be middleware that converts other data sources into configuration information, such as EnvironmentVariablesConfigurationProvider.</summary>
	/// <param name="stringBuilder"></param>
	/// <param name="providers"></param>
	/// <param name="symbols"></param>
	private static void AppendProvidersList(
		StringBuilder stringBuilder,
		Dictionary<string, string> providers,
		IVisualizationSymbols symbols)
	{
		stringBuilder.AppendLine("\nConfiguration Providers (in order of precedence):");
		stringBuilder.AppendLine("============================================");
		if(providers.Any()) {
			foreach(var provider in providers)
				stringBuilder.AppendLine($"{symbols.Provider} {provider.Value}");
		}
		else
			stringBuilder.AppendLine("No provider information available");

		stringBuilder.AppendLine();
	}


	/// <summary>Appends the section header to the visualization output.</summary>
	/// <param name="stringBuilder">StringBuilder collecting the visualization output</param>
	/// <param name="indentation">Current indentation string</param>
	/// <param name="section">The configuration section being processed</param>
	/// <param name="symbols">Symbols to use for visualization</param>
	private static void AppendSectionHeader(
		StringBuilder stringBuilder,
		string indentation,
		IConfigurationSection section,
		IVisualizationSymbols symbols)
	{
		stringBuilder.Append($"{indentation}{symbols.Section} {section.Key}");
	}


	/// <summary>Appends a section's value and its provider history to the visualization output.</summary>
	/// <param name="stringBuilder">StringBuilder collecting the visualization output</param>
	/// <param name="indentation">Current indentation string</param>
	/// <param name="currentPath">Full configuration path of the current section</param>
	/// <param name="value">The section's value</param>
	/// <param name="valueProviders">Dictionary mapping configuration paths to their provider history</param>
	/// <param name="providers">Dictionary mapping provider keys to their display names</param>
	/// <param name="symbols">Symbols to use for visualization</param>
	private static void AppendSectionValue(
		StringBuilder stringBuilder,
		string indentation,
		string currentPath,
		string value,
		Dictionary<string, List<(string Provider, string Value)>> valueProviders,
		Dictionary<string, string> providers,
		IVisualizationSymbols symbols)
	{
		stringBuilder.Append($" = {value}");

		if(!valueProviders.TryGetValue(currentPath, out var sources))
			return;

		if(sources.Count > 1)
			AppendValueHistory(stringBuilder, indentation, sources, providers, value, symbols);
		else if(sources.Count == 1) {
			var (providerKey, _) = sources[0];
			stringBuilder.Append($" {symbols.Provider} [{providers[providerKey]}]");
		}

		stringBuilder.AppendLine();
	}


	/// <summary>Appends the history of a specific value to the visualization. This is useful because .NET configuration values can be set and then overridden. For example, a default connection string might be set in appsettings.json, and then overridden in appsettings.Development.json. This may be confusing to a developer who sets the value in appsettings.json and expects it to be honored. With this history, the evolution of each value is clear.</summary>
	private static void AppendValueHistory(
		StringBuilder stringBuilder,
		string indentation,
		List<(string Provider, string Value)> sources,
		Dictionary<string, string> providers,
		string currentValue,
		IVisualizationSymbols symbols)
	{
		stringBuilder.AppendLine();
		stringBuilder.Append($"{indentation}  {symbols.HistoryArrow} Value history (most recent first):");

		var isFirst = true;
		foreach(var (providerKey, historicalValue) in sources) {
			stringBuilder.AppendLine();
			stringBuilder.Append($"{indentation}    {symbols.Provider} ");
			if(isFirst)
				stringBuilder.Append($"({symbols.Active} active)  [{providers[providerKey]}] = {currentValue}");
			else
				stringBuilder.Append($"({symbols.Overridden} overridden)  [{providers[providerKey]}] = {historicalValue}");
			isFirst = false;
		}
	}


	/// <summary>Builds a full configuration path by combining a parent path and key.</summary>
	/// <param name="parentPath">Full configuration path of the parent section, or null if root</param>
	/// <param name="key">The key of the current section</param>
	/// <returns>The full configuration path for the current section</returns>
	private static string BuildPath(string? parentPath, string key) =>
		string.IsNullOrEmpty(parentPath) ? key : $"{parentPath}:{key}";


	/// <summary>Collects information about each provider which contributed to the configuration information</summary>
	/// <param name="configRoot">This is the root of the IConfiguration hierarchy</param>
	private static (Dictionary<string, string> Providers, Dictionary<string, List<(string Provider, string Value)>> ValueProviders) CollectProviderInformation(IConfigurationRoot configRoot)
	{
		var providers = new Dictionary<string, string>();
		var valueProviders = new Dictionary<string, List<(string Provider, string Value)>>();

		foreach(var provider in configRoot.Providers.Reverse()) {
			var providerKey = provider.ToString();
			var providerName = provider switch {
				FileConfigurationProvider fileProvider => fileProvider.Source.Path,
				_ => provider.GetType().Name
			};
			if(providerKey is null || providerName is null)
				continue;
			providers[providerKey] = providerName;

			foreach(var child in configRoot.GetChildren())
				CollectValues(child, "", provider, providerKey, valueProviders);
		}

		return (providers, valueProviders);
	}


	private static void CollectValues(
		IConfigurationSection section,
		string parentPath,
		IConfigurationProvider provider,
		string providerKey,
		Dictionary<string, List<(string Provider, string Value)>> valueProviders)
	{
		var currentPath = string.IsNullOrEmpty(parentPath) ? section.Key : $"{parentPath}:{section.Key}";
		if(provider.TryGet(currentPath, out var value)) {
			if(!valueProviders.ContainsKey(currentPath))
				valueProviders[currentPath] = new List<(string, string)>();
			if(value is not null)
				valueProviders[currentPath].Add((providerKey, value));
		}

		foreach(var child in section.GetChildren())
			CollectValues(child, currentPath, provider, providerKey, valueProviders);
	}


	internal static string GenerateVisualization(IConfiguration configuration, bool excludeVendorSections, IVisualizationSymbols symbols, IEnumerable<string>? excludePrefixes = null)
	{
		if(symbols is VisualizationSymbols.Unicode)
			Console.OutputEncoding = Encoding.UTF8;
		var vendorPrefixes = excludePrefixes?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? DefaultVendorPrefixes;
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Configuration Structure:\n=======================");
		var (providers, valueProviders) = configuration is IConfigurationRoot configRoot
			? CollectProviderInformation(configRoot)
			: (new Dictionary<string, string>(), new Dictionary<string, List<(string Provider, string Value)>>());
		AppendProvidersList(stringBuilder, providers, symbols);
		foreach(var child in configuration.GetChildren())
			TraverseSection(
				child, 0, null, stringBuilder,
				valueProviders, providers, vendorPrefixes, excludeVendorSections, symbols);
		AppendLegend(stringBuilder, symbols);
		return stringBuilder.ToString();
	}


	/// <summary>Determines if a configuration section is a leaf node with a value.</summary>
	/// <param name="section">The configuration section to check</param>
	/// <returns>True if the section has a value and no children, false otherwise</returns>
	private static bool IsLeafWithValue(IConfigurationSection section) => !string.IsNullOrEmpty(section.Value) && !section.GetChildren().Any();


	/// <summary>Determines if a configuration section should be skipped based on vendor prefix rules.</summary>
	/// <param name="section">The configuration section to check</param>
	/// <param name="parentPath">Full configuration path of the parent section, or null if root</param>
	/// <param name="vendorPrefixes">Set of configuration path prefixes to potentially exclude</param>
	/// <param name="excludeVendorSections">Whether to exclude sections matching vendor prefixes</param>
	/// <returns>True if the section should be skipped, false otherwise</returns>
	private static bool ShouldSkipSection(IConfigurationSection section, string? parentPath, HashSet<string> vendorPrefixes, bool excludeVendorSections)
	{
		if(!excludeVendorSections)
			return false;
		var currentPath = BuildPath(parentPath, section.Key);
		return vendorPrefixes.Any(prefix => currentPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
	}


	/// <summary>Recursively traverses and visualizes a configuration section, its value, and any children.</summary>
	/// <param name="section">The configuration section to process</param>
	/// <param name="depth">Current indentation depth</param>
	/// <param name="parentPath">Full configuration path of the parent section, or null if root</param>
	/// <param name="stringBuilder">StringBuilder collecting the visualization output</param>
	/// <param name="valueProviders">Dictionary mapping configuration paths to their provider history</param>
	/// <param name="providers">Dictionary mapping provider keys to their display names</param>
	/// <param name="vendorPrefixes">Set of configuration path prefixes to potentially exclude</param>
	/// <param name="excludeVendorSections">Whether to exclude sections matching vendor prefixes</param>
	/// <param name="symbols">Symbols to use for visualization</param>
	private static void TraverseSection(
		IConfigurationSection section,
		int depth,
		string? parentPath,
		StringBuilder stringBuilder,
		Dictionary<string, List<(string Provider, string Value)>> valueProviders,
		Dictionary<string, string> providers,
		HashSet<string> vendorPrefixes,
		bool excludeVendorSections,
		IVisualizationSymbols symbols)
	{
		if(ShouldSkipSection(section, parentPath, vendorPrefixes, excludeVendorSections))
			return;
		var currentPath = BuildPath(parentPath, section.Key);
		var indentation = new string(' ', depth * 2);
		AppendSectionHeader(stringBuilder, indentation, section, symbols);
		if(IsLeafWithValue(section))
			AppendSectionValue(stringBuilder, indentation, currentPath, section.Value!, valueProviders, providers, symbols);
		foreach(var child in section.GetChildren())
			TraverseSection(child, depth + 1, currentPath, stringBuilder, valueProviders, providers, vendorPrefixes, excludeVendorSections, symbols);
	}
}

internal interface IVisualizationSymbols
{
	string Section { get; }
	string Provider { get; }
	string Active { get; }
	string Overridden { get; }
	string HistoryArrow { get; }
}

internal static class VisualizationSymbols
{
	/// <summary>These symbols will be used in the output to attempt to graphically represent it as much as possible for text. Terminals which do not support emoticons will need to use the Ascii symbols.</summary>
	internal class Unicode : IVisualizationSymbols
	{
		public string Section => "📂";
		public string Provider => "📄";
		public string Active => "✓";
		public string Overridden => "↪";
		public string HistoryArrow => "↳";
	}

	/// <summary>These symbols will be used in the output to attempt to graphically represent it as much as possible for ASCII text. Terminals that support unicode characters should opt for the Unicode symbols as they look much nicer.</summary>
	internal class Ascii : IVisualizationSymbols
	{
		public string Section => "+";
		public string Provider => "*";
		public string Active => ">";
		public string Overridden => "-";
		public string HistoryArrow => "\\";
	}
}

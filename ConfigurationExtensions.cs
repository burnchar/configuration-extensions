using FastCoreLib.LaunchPad.ExtensionMethodLogic;
using Microsoft.Extensions.Configuration;

namespace YourNamespace.ExtensionMethods;

/// <summary>Extension methods for <see cref="IConfiguration" /></summary>
public static class ConfigurationExtensions
{
	/// <summary>Visualize the program configuration into a human-readable string</summary>
	/// <remarks><see cref="Visualize(Microsoft.Extensions.Configuration.IConfiguration,System.Collections.Generic.IEnumerable{string})" /> will set the console output encoding to UTF-8. Use <see cref="VisualizeSimple(Microsoft.Extensions.Configuration.IConfiguration,bool)" /> to keep the output encoding unchanged.</remarks>
	/// <param name="configuration">The program configuration</param>
	/// <param name="excludePrefixes">Exclude configuration sections beginning with these prefixes. For example, to eliminate Microsoft internal configuration logic, use "Microsoft". These are case-insensitive</param>
	/// <returns>A string with a beautiful visualization of your program configuration</returns>
	public static string Visualize(this IConfiguration configuration, IEnumerable<string> excludePrefixes) =>
		ConfigurationVisualizer.GenerateVisualization(configuration, true, new VisualizationSymbols.Unicode(), excludePrefixes);


	/// <inheritdoc cref="Visualize(Microsoft.Extensions.Configuration.IConfiguration,System.Collections.Generic.IEnumerable{string})" />
	/// <param name="excludeVendorSections">True to exclude common internal configuration sections which are often of little interest, or false to show the entire configuration object in all its verbose glory</param>
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
	public static string Visualize(this IConfiguration configuration, bool excludeVendorSections = true) =>
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		ConfigurationVisualizer.GenerateVisualization(configuration, excludeVendorSections, new VisualizationSymbols.Unicode());


	/// <inheritdoc cref="Visualize(Microsoft.Extensions.Configuration.IConfiguration,System.Boolean)" />
	/// <returns>A string with a highly compatible ASCII visualization of your program configuration</returns>
	public static string VisualizeSimple(this IConfiguration configuration, bool excludeVendorSections = true) =>
		ConfigurationVisualizer.GenerateVisualization(configuration, excludeVendorSections, new VisualizationSymbols.Ascii());


	/// <inheritdoc cref="Visualize(Microsoft.Extensions.Configuration.IConfiguration,System.Collections.Generic.IEnumerable{string})" />
	/// <returns>A string with a highly compatible ASCII visualization of your program configuration</returns>
	public static string VisualizeSimple(this IConfiguration configuration, IEnumerable<string> excludePrefixes) =>
		ConfigurationVisualizer.GenerateVisualization(configuration, true, new VisualizationSymbols.Ascii(), excludePrefixes);
}

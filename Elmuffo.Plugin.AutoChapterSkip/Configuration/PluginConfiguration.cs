using MediaBrowser.Model.Plugins;

namespace Elmuffo.Plugin.AutoChapterSkip.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        Match = string.Empty;
    }

    /// <summary>
    /// Gets or sets the regex to match.
    /// </summary>
    public string Match { get; set; }
}

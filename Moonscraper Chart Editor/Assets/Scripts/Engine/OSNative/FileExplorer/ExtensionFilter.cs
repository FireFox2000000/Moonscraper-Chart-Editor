public struct ExtensionFilter
{
    public string name;
    public string[] extensions;

    public ExtensionFilter(string filterName, params string[] filterExtensions)
    {
        name = filterName;
        extensions = filterExtensions;
    }
}

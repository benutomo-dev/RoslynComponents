namespace Benutomo.SourceGeneratorCommons
{
    class NameSpaceInfo : ITypeContainer, IEquatable<NameSpaceInfo?>
    {
        public string Name { get; }

        public NameSpaceInfo(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as NameSpaceInfo);
        }

        public bool Equals(NameSpaceInfo? other)
        {
            return other is not null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }

}

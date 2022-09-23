using System.Collections.Immutable;

namespace Benutomo.SourceGeneratorCommons
{
    class TypeDefinitionInfo : ITypeContainer, IEquatable<TypeDefinitionInfo?>
    {
        public ITypeContainer Container { get; }

        public string Name { get; }

        public bool IsValueType { get; }

        public bool IsNullableAnoteted { get; }

        public ImmutableArray<string> GenericTypeArgs { get; }

        public TypeDefinitionInfo(ITypeContainer container, string name, bool isValueType, bool isNullableAnoteted, ImmutableArray<string> genericTypeArgs)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsValueType = isValueType;
            IsNullableAnoteted = isNullableAnoteted;
            GenericTypeArgs = genericTypeArgs;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeDefinitionInfo);
        }

        public bool Equals(TypeDefinitionInfo? other)
        {
            return other is not null &&
                   EqualityComparer<ITypeContainer>.Default.Equals(Container, other.Container) &&
                   Name == other.Name &&
                   IsValueType == other.IsValueType &&
                   IsNullableAnoteted == other.IsNullableAnoteted &&
                   GenericTypeArgs.SequenceEqual(other.GenericTypeArgs);
        }

        public override int GetHashCode()
        {
            int hashCode = 319546947;
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeContainer>.Default.GetHashCode(Container);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + IsValueType.GetHashCode();
            hashCode = hashCode * -1521134295 + IsNullableAnoteted.GetHashCode();
            hashCode = hashCode * -1521134295 + GenericTypeArgs.Aggregate(0, (hash, v) => hash ^ v.GetHashCode());
            return hashCode;
        }
    }

}

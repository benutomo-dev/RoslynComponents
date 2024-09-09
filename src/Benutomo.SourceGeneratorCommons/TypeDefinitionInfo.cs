using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons
{
    sealed class TypeDefinitionInfo : ITypeContainer, IEquatable<TypeDefinitionInfo?>
    {
        public ITypeContainer Container { get; }

        public string Name { get; }

        public bool IsValueType { get; }

        public bool IsNullableAnoteted { get; }

        public ImmutableArray<string> GenericTypeArgs { get; }

        public string NameWithGenericArgs
        {
            get
            {
                if (_nameWithGenericArgs is null)
                {
                    if (GenericTypeArgs.IsEmpty)
                    {
                        _nameWithGenericArgs = Name;
                    }
                    else
                    {
                        _nameWithGenericArgs = $"{Name}<{string.Join(",", GenericTypeArgs)}>";
                    }
                }

                return _nameWithGenericArgs;
            }
        }

        public string? _nameWithGenericArgs;

        public TypeDefinitionInfo(ITypeContainer container, string name, bool isValueType, bool isNullableAnoteted, ImmutableArray<string> genericTypeArgs)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsValueType = isValueType;
            IsNullableAnoteted = isNullableAnoteted;
            GenericTypeArgs = genericTypeArgs;
        }

        public string MakeHintName()
        {
            var builder = new StringBuilder(256);
            append(builder, this);
            return builder.ToString();

            static void append(StringBuilder builder, ITypeContainer container)
            {
                if (container is TypeDefinitionInfo typeDefinitionInfo)
                {
                    if (typeDefinitionInfo.Container is not null)
                    {
                        append(builder, typeDefinitionInfo.Container);
                        builder.Append('.');
                    }

                    builder.Append(typeDefinitionInfo.Name);

                    if (typeDefinitionInfo.GenericTypeArgs.Length > 0)
                    {
                        foreach (var  genericArgument in typeDefinitionInfo.GenericTypeArgs)
                        {
                            builder.Append('_');
                            builder.Append(genericArgument);
                        }
                    }
                }
                else
                {
                    Debug.Assert(container is NameSpaceInfo);

                    builder.Append(container.Name);
                }
            }
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

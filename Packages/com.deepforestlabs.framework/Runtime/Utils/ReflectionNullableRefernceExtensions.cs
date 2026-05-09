#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DeepForestLabs.Utils
{
    public static class ReflectionNullabilityInfoContext
    {
        public static bool IsNullableReference(this PropertyInfo property) =>
            IsNullableReference(property.PropertyType, property.DeclaringType, property.CustomAttributes);

        public static bool IsNullableReference(this FieldInfo field) =>
            IsNullableReference(field.FieldType, field.DeclaringType, field.CustomAttributes);

        public static bool IsNullableReference(this ParameterInfo parameter) =>
            IsNullableReference(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);
        
        public static bool IsListOfNullableReferenceType(this FieldInfo field)
        {
            if (!field.FieldType.IsGenericType || field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
            {
                return false;
            }

            Type elementType = field.FieldType.GetGenericArguments()[0];
            if (elementType.IsValueType)
            {
                return false;
            }
            
            return IsNullableReference(elementType,  field.DeclaringType, field.GetCustomAttributesData());
        }

        public static bool IsArrayOfNullableReferenceType(this FieldInfo field)
        {
            if (!field.FieldType.IsArray)
            {
                return false;
            }

            Type? elementType = field.FieldType.GetElementType();
            if (elementType == null || elementType.IsValueType)
            {
                return false;
            }

            return IsNullableReference(elementType, field.DeclaringType, field.GetCustomAttributesData());
        }

        private static bool IsNullableReference(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
        {
            if (memberType.IsPrimitive)
            {
                return false;
            }

            if (memberType.IsValueType)
            {
                return Nullable.GetUnderlyingType(memberType) != null;
            }

            CustomAttributeData? nullable = customAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == kNullableContextAttributeInternal ||
                                     x.AttributeType.FullName == kNullableAttributeInternal);
            if (nullable is { ConstructorArguments: { Count: 1 } })
            {
                CustomAttributeTypedArgument attributeArgument = nullable.ConstructorArguments[0];
                if (attributeArgument.ArgumentType == typeof(byte[]))
                {
                    ReadOnlyCollection<CustomAttributeTypedArgument> args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                    {
                        return (byte)args[0].Value! == 2;
                    }
                }
                else if (attributeArgument.ArgumentType == typeof(byte))
                {
                    return (byte)attributeArgument.Value! == 2;
                }
            }

            for (MemberInfo? type = declaringType; type != null; type = type.DeclaringType)
            {
                CustomAttributeData? context = type.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == kNullableContextAttributeInternal ||
                                         x.AttributeType.FullName == kNullableAttributeInternal);
                if (context is { ConstructorArguments: { Count: 1 } } &&
                    context.ConstructorArguments[0].ArgumentType == typeof(byte))
                {
                    return (byte)context.ConstructorArguments[0].Value! == 2;
                }
            }

            // Couldn't find a suitable attribute
            return false;
        }

        // Internal to System.Reflections and cannot be reference directly.
        private const string kNullableContextAttributeInternal = "System.Runtime.CompilerServices.NullableContextAttribute";
        private const string kNullableAttributeInternal = "System.Runtime.CompilerServices.NullableAttribute";
    }
}
#nullable disable
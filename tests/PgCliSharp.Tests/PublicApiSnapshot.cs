using System.Globalization;
using System.Reflection;

namespace PgCliSharp.Tests;

// Deliberately independent of assembly-qualified BCL names and reflection order.
internal static class PublicApiSnapshot
{
    private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    internal static IEnumerable<Type> ContractTypes(Assembly assembly) => assembly.GetTypes().Where(ExternallyAccessible);

    private static bool ExternallyAccessible(Type type) => type.IsPublic ||
        ((type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamORAssem) && ExternallyAccessible(type.DeclaringType!));

    internal static string[] Capture(IEnumerable<Type> types)
    {
        var lines = new List<string>();
        foreach (Type type in types)
        {
            string owner = Name(type);
            lines.Add("TYPE " + owner + " " + AttributeFlags(type.Attributes) + " base=" +
                (type.BaseType is null ? "-" : Name(type.BaseType)) + " interfaces=" +
                string.Join(",", type.GetInterfaces().Except(type.BaseType?.GetInterfaces() ?? Array.Empty<Type>())
                    .Select(Name).OrderBy(name => name, StringComparer.Ordinal)) + Metadata(type.GetCustomAttributesData()));
            foreach (Type argument in type.GetGenericArguments().Where(argument => argument.IsGenericParameter))
                lines.Add("CONSTRAINT " + owner + " " + Constraint(argument));
            foreach (ConstructorInfo constructor in type.GetConstructors(Declared).Where(Visible))
                lines.Add("CTOR " + owner + " " + AttributeFlags(constructor.Attributes) + Parameters(constructor.GetParameters()));
            foreach (MethodInfo method in type.GetMethods(Declared).Where(Visible))
            {
                string generics = method.IsGenericMethod ? "<" + string.Join(",", method.GetGenericArguments().Select(Constraint)) + ">" : "";
                lines.Add("METHOD " + owner + "." + method.Name + generics + " " + AttributeFlags(method.Attributes) +
                    " returns=" + ParameterType(method.ReturnParameter) + Parameters(method.GetParameters()) + Metadata(method.GetCustomAttributesData()));
            }
            foreach (PropertyInfo property in type.GetProperties(Declared).Where(property => property.GetAccessors(true).Any(Visible)))
                lines.Add("PROPERTY " + owner + "." + property.Name + " " + Name(property.PropertyType) +
                    Parameters(property.GetIndexParameters()) + Metadata(property.GetCustomAttributesData()));
            foreach (FieldInfo field in type.GetFields(Declared).Where(field => !field.IsSpecialName && (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly)))
                lines.Add("FIELD " + owner + "." + field.Name + " " + AttributeFlags(field.Attributes) + " " + Name(field.FieldType) +
                    (field.IsLiteral ? " value=" + Value(field.GetRawConstantValue()) : "") + Metadata(field.GetCustomAttributesData()));
            foreach (EventInfo item in type.GetEvents(Declared).Where(item => item.GetAddMethod(true) is MethodInfo method && Visible(method)))
                lines.Add("EVENT " + owner + "." + item.Name + " " + Name(item.EventHandlerType!) + Metadata(item.GetCustomAttributesData()));
        }
        return lines.OrderBy(line => line, StringComparer.Ordinal).ToArray();
    }

    private static bool Visible(MethodBase method) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

    // Framework's Enum.ToString() may include names for zero-valued aliases.
    private static string AttributeFlags(Enum value) => string.Join(", ", value.ToString().Split(',')
        .Select(name => name.Trim()).Where(name => Convert.ToUInt64(Enum.Parse(value.GetType(), name), CultureInfo.InvariantCulture) != 0));

    private static string Parameters(ParameterInfo[] parameters) => "(" + string.Join(",", parameters.Select(parameter =>
        parameter.Name + ":" + ParameterType(parameter) + " flags=" + parameter.Attributes +
        (parameter.HasDefaultValue ? " default=" + Value(parameter.DefaultValue) : "") +
        (parameter.GetCustomAttributesData().Any(attribute => attribute.AttributeType == typeof(ParamArrayAttribute)) ? " params" : ""))) + ")";

    private static string ParameterType(ParameterInfo parameter) => Name(parameter.ParameterType) +
        " modreq=" + string.Join(",", parameter.GetRequiredCustomModifiers().Select(Name)) +
        " modopt=" + string.Join(",", parameter.GetOptionalCustomModifiers().Select(Name)) + Metadata(parameter.GetCustomAttributesData());

    private static string Constraint(Type argument) => argument.Name + ":" + argument.GenericParameterAttributes + ":" +
        string.Join(",", argument.GetGenericParameterConstraints().Select(Name).OrderBy(name => name, StringComparer.Ordinal));

    private static string Name(Type type)
    {
        if (type.IsByRef) return Name(type.GetElementType()!) + "&";
        if (type.IsPointer) return Name(type.GetElementType()!) + "*";
        if (type.IsArray) return Name(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        if (type.IsGenericParameter) return "!" + type.Name;
        if (!type.IsGenericType) return type.FullName ?? type.Name;
        string name = type.GetGenericTypeDefinition().FullName!;
        return name + "<" + string.Join(",", type.GetGenericArguments().Select(Name)) + ">";
    }

    private static string Metadata(IList<CustomAttributeData> attributes) => string.Concat(attributes
        .Where(attribute => attribute.AttributeType.FullName is "System.Runtime.CompilerServices.NullableAttribute" or
            "System.Runtime.CompilerServices.NullableContextAttribute" or "System.ObsoleteAttribute" or "System.FlagsAttribute")
        .Select(attribute => " [" + attribute.AttributeType.FullName + "(" + string.Join(",", attribute.ConstructorArguments.Select(AttributeValue)) + ")]" )
        .OrderBy(value => value, StringComparer.Ordinal));

    private static string AttributeValue(CustomAttributeTypedArgument argument) => argument.Value is IEnumerable<CustomAttributeTypedArgument> values
        ? "[" + string.Join(",", values.Select(AttributeValue)) + "]" : Value(argument.Value);

    private static string Value(object? value) => value switch
    {
        null => "null",
        string text => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"",
        IFormattable number => number.ToString(null, CultureInfo.InvariantCulture)!,
        _ => value.ToString()!,
    };
}

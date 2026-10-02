using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace PgCliSharp.Tests;

public sealed class Phase8DocumentationAndDiagnosticsTests
{
    [Fact]
    public void PublicSignatures_DoNotLeakInternalOrCliWrapTypes()
    {
        foreach (Type type in typeof(PostgreSqlMajorVersion).Assembly.GetExportedTypes())
        {
            if (type.BaseType is not null)
                AssertPublicContractType(type.BaseType);

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                AssertPublicContractType(method.ReturnType);
                foreach (ParameterInfo parameter in method.GetParameters())
                    AssertPublicContractType(parameter.ParameterType);
            }

            foreach (ConstructorInfo constructor in type.GetConstructors())
                foreach (ParameterInfo parameter in constructor.GetParameters())
                    AssertPublicContractType(parameter.ParameterType);

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                AssertPublicContractType(field.FieldType);

            foreach (Type contract in type.GetInterfaces())
                AssertPublicContractType(contract);
        }
    }

    [Fact]
    public void BuiltXmlDocumentation_HasOrderedBilingualSummariesAndText()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "documentation", "PgCliSharp.xml");
        XDocument document = XDocument.Load(path);
        XElement[] members = document.Descendants("member").ToArray();
        Assert.NotEmpty(members);

        foreach (XElement member in members)
        {
            string name = member.Attribute("name")!.Value;
            if (member.Element("inheritdoc") is not null)
                continue;

            XElement? summary = member.Element("summary");
            Assert.True(summary is not null, $"Missing summary: {name}");
            AssertBilingual(summary!, name);

            foreach (XElement text in member.Elements().Where(element =>
                element.Name.LocalName is "remarks" or "param" or "returns" or "value" or "exception"))
            {
                AssertBilingual(text, name);
            }
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("ja-JP")]
    [InlineData("fr-FR")]
    [InlineData("ar-EG")]
    public void RejectedNumericValue_IsInvariantAcrossFormattingAndUiCultures(string cultureName)
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            var exception = new PgInvalidOptionValueException(PostgreSqlMajorVersion.V18, "--rate", 1234.5m);

            Assert.Equal("1234.5", exception.Value);
            Assert.Equal("--rate", exception.OptionName);
            Assert.Equal(PostgreSqlMajorVersion.V18, exception.SelectedVersion);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void RejectedValue_PreservesNullAndLiteralText()
    {
        var missing = new PgInvalidOptionValueException(PostgreSqlMajorVersion.V18, "--rate", null);
        var literal = new PgInvalidOptionValueException(PostgreSqlMajorVersion.V18, "--rate", "1,25");

        Assert.Null(missing.Value);
        Assert.Equal("1,25", literal.Value);
    }

    private static void AssertBilingual(XElement element, string memberName)
    {
        string text = element.Value;
        int english = text.IndexOf("EN:", StringComparison.Ordinal);
        int japanese = text.IndexOf("JA:", StringComparison.Ordinal);
        Assert.True(english >= 0 && japanese > english,
            $"Missing or incorrectly ordered EN/JA in {memberName}: {element.Name}");
        Assert.False(string.IsNullOrWhiteSpace(text.Substring(english + 3, japanese - english - 3)));
        Assert.False(string.IsNullOrWhiteSpace(text.Substring(japanese + 3)));
    }

    private static void AssertPublicContractType(Type type)
    {
        Assert.False(type.FullName?.StartsWith("CliWrap.", StringComparison.Ordinal) == true,
            $"CliWrap type leaked into public contract: {type}");
        Assert.False(type.FullName?.StartsWith("PgCliSharp.Internal.", StringComparison.Ordinal) == true,
            $"Internal type leaked into public contract: {type}");
        if (type.HasElementType)
            AssertPublicContractType(type.GetElementType()!);
        foreach (Type argument in type.GetGenericArguments())
            AssertPublicContractType(argument);
    }
}

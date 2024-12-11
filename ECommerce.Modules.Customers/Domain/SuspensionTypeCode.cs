
namespace ECommerce.Modules.Customers.Domain;

public class SuspensionTypeCode
{
    public string Code { get; set; }
    public string Description { get; set; }

    private static readonly Dictionary<string, SuspensionTypeCode> s_knownSuspensionTypes = new();

    private SuspensionTypeCode(string code, string description)
    {
        Code = code;
        Description = description;
    }

    private static SuspensionTypeCode Create(string code, string description)
    {
        if (s_knownSuspensionTypes.TryGetValue(code, out var existingSuspensionType))
        {
            return existingSuspensionType;
        }

        var newSuspensionType = new SuspensionTypeCode(code, description);
        s_knownSuspensionTypes.Add(code, newSuspensionType);
        return newSuspensionType;
    }

    public static SuspensionTypeCode FromCode(string code)
    {
        if (!s_knownSuspensionTypes.TryGetValue(code, out var suspensionType))
        {
            throw new InvalidOperationException($"Suspension type with code '{code}' is not known.");
        }

        return suspensionType;
    }

    public static void LoadFromDatabase(IEnumerable<SuspensionType> suspensionTypes)
    {
        foreach (var type in suspensionTypes)
        {
            Create(type.Code, type.Description);
        }
    }

    public static IEnumerable<SuspensionTypeCode> GetAllSuspensionTypes()
    {
        return s_knownSuspensionTypes.Values;
    }
}

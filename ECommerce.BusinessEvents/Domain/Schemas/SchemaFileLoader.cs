using System.Reflection;

namespace ECommerce.BusinessEvents.Domain.Schemas
{
    public static class SchemaFileLoader
    {
        public static string LoadSchema(string schemaFileName)
        {
            var schemaPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Domain",
                "Schemas",
                schemaFileName);

            if (!File.Exists(schemaPath))
                throw new FileNotFoundException($"Schema file not found: {schemaPath}");
            return File.ReadAllText(schemaPath);
        }

        public static string LoadEmbeddedSchema(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

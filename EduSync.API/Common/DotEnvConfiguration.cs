namespace EduSync.API.Common;

public static class DotEnvConfiguration
{
    public static Dictionary<string, string?> ReadEnvironment()
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        string[] keys =
        [
            "ConnectionStrings_DefaultConnection",
            "Jwt_Issuer",
            "Jwt_Audience",
            "Jwt_Key",
            "Jwt_AccessTokenMinutes",
            "Jwt_RefreshTokenDays",
            "Admin_Email",
            "Admin_Password",
            "Seed_DemoRequests",
            "Seed_HihiStudents",
            "Lecturer_Email",
            "Lecturer_Password",
            "Lecturer_Code",
            "Lecturer_FullName",
            "Lecturer_Phone"
        ];

        foreach (var key in keys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (value is not null)
            {
                values[key.Replace('_', ':')] = value;
            }
        }

        return values;
    }

    public static Dictionary<string, string?> Read(string path)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return values;
        }

        foreach (var line in File.ReadLines(path))
        {
            var entry = line.Trim().TrimStart('\uFEFF');
            if (entry.Length == 0 || entry.StartsWith('#'))
            {
                continue;
            }

            var separator = entry.IndexOf('=');
            if (separator <= 0)
            {
                throw new InvalidOperationException($"Invalid .env entry in {path}.");
            }

            var key = entry[..separator].Trim().Replace('_', ':');
            var value = entry[(separator + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            values[key] = value;
        }

        return values;
    }
}

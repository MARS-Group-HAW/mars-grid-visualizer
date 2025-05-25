using System.Text.Json;
using Godot;

namespace mmvp.src;

public static class DebugExtensions
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
    };

    public static T Dbg<T>(this T obj, string label = null,
                          [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                          [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
                          [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        string fileName = Path.GetFileName(filePath);
        string prefix = label != null ? $"{label}: " : "";

        try
        {
            string json = JsonSerializer.Serialize(obj, jsonOptions);
            GD.Print($"[DBG] {fileName}:{lineNumber} in {memberName}(): {prefix}");
            GD.Print(json);
        }
        catch
        {
            GD.Print($"[DBG] {fileName}:{lineNumber} in {memberName}(): {prefix}{obj}");
        }

        return obj;
    }
}


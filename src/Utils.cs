using System.Text.Json;
using Godot;

namespace MarsGridVisualizer;

public static class DebugExtensions
{
	private static readonly JsonSerializerOptions jsonOptions = new()
	{
		WriteIndented = true,
	};

	public static T Dbg<T>(this T obj, string? label = null,
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

class Names
{
	private static readonly List<string> names = [
			"Ada", "Alan", "Grace", "Dennis", "Linus", "Margaret",
			"Tim", "John", "Barbara", "Donald", "Frances", "Kathleen",
			"Jean", "Betty", "Marlyn", "Adele", "Hedy", "Radia",
			"Vint", "Bob", "Larry", "Sergey", "Mark", "Bill",
			"Steve", "Richard", "Brian", "Rob", "Ken",
			"Doug", "Andy", "James", "Michael", "David", "Scott",
			"Eric", "Guido", "Yukihiro", "Brendan", "Anders", "Bjarne",
			"Niklaus", "Edsger", "Tony", "Leslie", "Shafi", "Silvio",
			"Adi", "Ronald", "Whitfield", "Martin", "Peter", "Andrew",
			"Frodo", "Gandalf", "Aragorn", "Legolas", "Gimli", "Boromir",
			"Samwise", "Meriadoc", "Peregrin", "Arwen", "Elrond", "Galadriel",
			"Faramir", "Eowyn", "Theoden", "Eomer", "Denethor", "Isildur",
			"Elendil", "Celeborn", "Haldir", "Thranduil", "Bard", "Thorin",
			"Balin", "Dwalin", "Fili", "Kili", "Bilbo", "Smaug"
		];

	/// <summary>
	/// Returns a list of unique, randomly selected names.
	/// </summary>
	public static List<string> SelectRandomNames(int count)
	{
		var random = new Random();
		return names
			.OrderBy(x => random.Next())
			.Take(count)
			.ToList();
	}
}

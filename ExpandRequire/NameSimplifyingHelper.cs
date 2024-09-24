using JCNET.字符串处理;

namespace ExpandRequire;

/// <summary>
///		名称化简
/// </summary>
internal static class NameSimplifyingHelper
{
	public static string SimplifyName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> name_set = lua_code_content.CollectFunctionName();
		List<string> name_list = name_set.ToList();
		name_list.Sort(new StringLengthComparer(StringLengthComparer.Order.FromLongToShort));
		foreach (string name in name_list)
		{
			string simple_name = GetNewName();
			Console.WriteLine($"{name} => {simple_name}");
			lua_code_content = lua_code_content.Replace(name, simple_name);
		}

		return lua_code_content;
	}

	/// <summary>
	///		将名称化简为 "a + 数字" 的形式，这里用来提供数字。
	/// </summary>
	private static ulong _name_id = 0;

	private static string GetNewName()
	{
		return $"A{_name_id++}";
	}

	private static HashSet<string> CollectFunctionName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> name_set = [];

		// 先逐行读取，过一遍，把所有名称收集过来
		while (true)
		{
			string? line = reader.ReadLine();
			if (line is null)
			{
				break;
			}

			string function_name = line.Cut("function", "(").Trim();
			if (function_name == string.Empty)
			{
				continue;
			}

			name_set.Add(function_name);
		}

		return name_set;
	}
}

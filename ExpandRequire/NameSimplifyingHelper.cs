using JCNET.字符串处理;

namespace ExpandRequire;

/// <summary>
///		名称化简
/// </summary>
internal static class NameSimplifyingHelper
{
	private class StringLengthComparer : IComparer<string>
	{
		public int Compare(string? x, string? y)
		{
			if (x is null && y is null)
			{
				return 0;
			}

			if (x is null && y is not null)
			{
				return 1;
			}

			if (x is not null && y is null)
			{
				return -1;
			}

			if (x!.Length > y!.Length)
			{
				return -1;
			}

			if (x!.Length < y!.Length)
			{
				return 1;
			}

			return 0;
		}
	}

	public static string SimplifyName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> sub_name_set = lua_code_content.CollectSubFunctionName();
		List<string> sub_name_list = sub_name_set.ToList();

		// 将长的排到前面优先替换，避免长的名称中有一部分含有短名称
		sub_name_list.Sort(new StringLengthComparer());
		foreach (string sub_name in sub_name_list)
		{
			string simple_name = GetNewName();
			lua_code_content = lua_code_content.Replace(sub_name, simple_name);
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

	private static HashSet<string> CollectSubFunctionName(this string lua_code_content)
	{
		StringReader reader = new(lua_code_content);
		HashSet<string> sub_name_set = [];

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

			Console.WriteLine(function_name);

			string[] sub_names = function_name.Split('.',
				StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

			foreach (string sub_name in sub_names)
			{
				sub_name_set.Add(sub_name);
			}
		}

		return sub_name_set;
	}
}

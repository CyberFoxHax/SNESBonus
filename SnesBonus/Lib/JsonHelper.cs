using System.Collections.Generic;

/////
//  http://stackoverflow.com/a/20558693/1148434
/////
namespace SnesBonus.Lib{
	public class JsonHelper {
		private const int IndentSize = 1;

		public static string FormatJson(string str) {
			str = (str ?? "").Replace("{}", @"\{\}").Replace("[]", @"\[\]");

			var inserts = new List<int[]>();
			var quoted = false;
			var escape = false;
			var depth = 0/*-1*/;

			for (int i = 0, n = str.Length; i < n; i++) {
				var chr = str[i];

				if (!escape && !quoted)
					switch (chr) {
						case '{':
						case '[':
							inserts.Add(new[] { i, +1, 0, IndentSize * ++depth });
							//int n = (i == 0 || "{[,".Contains(str[i - 1])) ? 0 : -1;
							//inserts.Add(new[] { i, n, IndentSize * ++depth * -n, IndentSize - 1 });
							break;
						case ',':
							inserts.Add(new[] { i, +1, 0, IndentSize * depth });
							//inserts.Add(new[] { i, -1, IndentSize * depth, IndentSize - 1 });
							break;
						case '}':
						case ']':
							inserts.Add(new[] { i, -1, IndentSize * --depth, 0 });
							//inserts.Add(new[] { i, -1, IndentSize * depth--, 0 });
							break;
						case ':':
							inserts.Add(new[] { i, 0, 1, 1 });
							break;
					}

				quoted = chr == '"' ? !quoted : quoted;
				escape = (chr == '\\') && !escape;
			}

			if (inserts.Count <= 0) return str.Replace(@"\{\}", "{}").Replace(@"\[\]", "[]");
			var sb = new System.Text.StringBuilder(str.Length * 2);

			var lastIndex = 0;
			foreach (var insert in inserts) {
				var index = insert[0];
				var before = insert[2];
				var after = insert[3];
				var nlBefore = insert[1] == -1;
				var nlAfter = insert[1] == +1;

				sb.Append(str.Substring(lastIndex, index - lastIndex));

				if (nlBefore) sb.AppendLine();
				if (before > 0) sb.Append(new string('\t', before));

				sb.Append(str[index]);

				if (nlAfter) sb.AppendLine();
				if (after > 0) sb.Append(new string('\t', after));

				lastIndex = index + 1;
			}

			str = sb.ToString();

			return str.Replace(@"\{\}", "{}").Replace(@"\[\]", "[]");
		}
	}
}
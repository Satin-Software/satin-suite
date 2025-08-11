namespace satin_common
{
    public class Config
    {
        private Dictionary<string, string> _config { get; }

        public Config()
        {
            _config = load_ini(app_env.ini_file);

            string str = get("Logname", "");
            if (!string.IsNullOrEmpty(str))
            {
                app_env.log_file = Path.Combine(app_env.log_dir, $"{str}.log");
            }
        }

        private Dictionary<string, string> load_ini(string file_path)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(file_path))
                throw new FileNotFoundException($".ini file not found: {file_path}");

            foreach (var line in File.ReadAllLines(file_path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                    continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                    dict[parts[0].Trim()] = parts[1].Trim();
            }
            return dict;
        }

        public string get(string key, string defaultValue = "")
        {
            return _config.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public int get_int(string key, int defaultValue = 0)
        {
            return int.TryParse(get(key), out var result) ? result : defaultValue;
        }

        public bool get_bool(string key, bool defaultValue = false)
        {
            return bool.TryParse(get(key), out var result) ? result : defaultValue;
        }
    }
}

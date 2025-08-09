

namespace satin_common
{
    public class Logger : IDisposable
    {
        private StreamWriter _log_writer;
        public Logger()
        {
            if (!Directory.Exists(app_env.log_dir))
            {
                Directory.CreateDirectory(app_env.log_dir);
            }
            if (!File.Exists(app_env.tmp_dir))
            {
                Directory.CreateDirectory(app_env.tmp_dir);
            }

            _log_writer = new StreamWriter(app_env.log_file, append: true)
            {
                AutoFlush = true
            };
            write("Log Start");
        }


        public void write(string message)
        {
            if (_log_writer == null) throw new InvalidOperationException("Logger not initialized.");

            string log_line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {app_env.project_name} {app_env.project_version} {message}";
            _log_writer.WriteLine(log_line);
        }


        public void Dispose()
        {
            write("Log End");

            _log_writer?.Flush();
            _log_writer?.Close();
            _log_writer?.Dispose();

            // move file from 'tmp' to 'log'
            if (File.Exists(app_env.log_file))
            {
                File.Move(app_env.log_file, Path.Combine(app_env.log_dir, Path.GetFileName(app_env.log_file)));
            }
        }
    }
}

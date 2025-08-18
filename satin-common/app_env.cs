using System.Diagnostics;
using System.Reflection;

namespace satin_common
{
    public static class app_env
    {
        public static readonly string project_name;
        public static readonly string exe_file;
        public static readonly string home_dir;
        public static readonly string log_dir;
        public static readonly string tmp_dir;
        public static readonly string mq_dir;
        public static readonly string cert_dir;
        public static readonly string ini_file;
        public static string log_file;

        public static readonly string project_version = "";

        static app_env()
        {
            try
            {
                project_version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";
                exe_file = Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException("Could not determine exe name"); ;
                project_name = Path.GetFileNameWithoutExtension(exe_file);

                if (Debugger.IsAttached) { home_dir = "c:\\satin\\"; } 
                else {
                    home_dir = Path.GetDirectoryName(exe_file) ?? throw new InvalidOperationException("Could not determine root directory");
                }                
                
                log_dir = Path.Combine(home_dir, "log");
                tmp_dir = Path.Combine(home_dir, "tmp");
                cert_dir = Path.Combine(home_dir, "cert");
                mq_dir = Path.Combine(home_dir, "mq");

                Directory.CreateDirectory(log_dir);
                Directory.CreateDirectory(tmp_dir);
                Directory.CreateDirectory(cert_dir);
                Directory.CreateDirectory(mq_dir);

                ini_file = Path.Combine(home_dir, $"{project_name}.ini");

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                log_file = Path.Combine(log_dir, $"{project_name}_{timestamp}.log");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"app env init error: {ex.Message}");
            }
        }
    }
}

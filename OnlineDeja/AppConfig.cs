using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineDeja {
    public class AppSettings {
        public string Appid { get; set; }
        public string Secret { get; set; }
    }

    public class AppConfig {
        public string ConnectionString { get; set; }

        public AppSettings AppSettings { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Droppy_v1
{
    public class Declarations
    {
        public static readonly string logPath = @"data\logs.log";
        public readonly string dataPath = @"data";
        public readonly string queuePath = @"data\queue";
        public readonly string lobbiesPath = @"data\lobbies\";
        public readonly string droppyVersion = "Droppy 1.19";
        public readonly string userdbPath = @"data\userdb\";

        public void CreateFilesAndDirectories()
        {
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            if (!Directory.Exists(lobbiesPath))
                Directory.CreateDirectory(lobbiesPath);
            if (!Directory.Exists(userdbPath))
                Directory.CreateDirectory(userdbPath);
            if (!File.Exists(queuePath))
                File.Create(queuePath).Close();
            if (!File.Exists(logPath))
                File.Create(logPath).Close();
        }
    }
}

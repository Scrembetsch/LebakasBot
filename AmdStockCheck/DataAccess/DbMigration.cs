using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.DataAccess
{
    internal static class DbMigration
    {
        private const string _DbLocationV_0_0_1 = "../";
        private const string _DbLocationV_0_0_2 = "../data/db/";

        internal const string LatestDbLocation = _DbLocationV_0_0_2;
        internal const string DbName = "amdstock.db";

        internal static void Migrate()
        {
            MigrateFromV_0_0_1ToV_0_0_2();
        }

        private static void MigrateFromV_0_0_1ToV_0_0_2()
        {
            if (File.Exists(_DbLocationV_0_0_1 + DbName))
            {
                Directory.CreateDirectory(_DbLocationV_0_0_2);
                File.Move(_DbLocationV_0_0_1 + DbName, _DbLocationV_0_0_2 + DbName);
            }
        }
    }
}

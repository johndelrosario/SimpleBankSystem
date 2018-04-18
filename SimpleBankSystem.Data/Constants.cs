using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBankSystem.Data
{
    public class Constants
    {
        public static string DatabaseName => "SimpleBankSystem";

        public static string ConnectionString => $"Server=(localdb)\\mssqllocaldb;Database={DatabaseName};Trusted_Connection=True;MultipleActiveResultSets=true";
    }
}

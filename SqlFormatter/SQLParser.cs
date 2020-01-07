using Microsoft.SqlServer.Management.SqlParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlFormatter
{
    public static class SQLParser
    {
        public static bool IsValid(string query)
        {
            return Parser.Parse(query).Errors.Count() == 0;
        }
    }
}

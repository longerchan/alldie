using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.DataTable
{
    /// <summary>
    /// 数据表管理器。管理所有CSV配置表的加载和查询。
    /// </summary>
    public class DataTableManager : IService
    {
        private readonly Dictionary<string, List<TableRow>> _tables = new();

        public void Initialize() { }

        public void Shutdown()
        {
            _tables.Clear();
        }

        public void LoadTable(string tableName, string csvContent)
        {
            var rows = CsvParser.Parse(csvContent);
            _tables[tableName] = rows;
        }

        public List<TableRow> GetTable(string tableName)
        {
            _tables.TryGetValue(tableName, out var rows);
            return rows ?? new List<TableRow>();
        }

        public TableRow GetRow(string tableName, string keyColumn, string keyValue)
        {
            var rows = GetTable(tableName);
            return rows.Find(r => r.GetString(keyColumn) == keyValue);
        }

        public List<TableRow> GetRowsByCondition(string tableName, string column, string value)
        {
            var rows = GetTable(tableName);
            return rows.FindAll(r => r.GetString(column) == value);
        }

        public bool HasTable(string tableName) => _tables.ContainsKey(tableName);
    }
}

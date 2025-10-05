using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ITlab1
{
    public class Manager
    {
        Database? db;
        public async Task<Database?> LoadDatabaseAsync(FileResult result)
        {
            using var stream = await result.OpenReadAsync();
            return await JsonSerializer.DeserializeAsync<Database>(stream);
        }

        public Database CreateDatabase(string name)
        {
            db = new Database(name);
            return db;
        }

        public Table CreateTable(string name)
        {
            Table tbl = new Table(name);
            db?.Tables.Add(tbl);
            return tbl;
        }

        public bool CreateColumn(Table table, string columnName, string columnType)
        {
            Column col = new Column(columnName, columnType);
            table.Columns.Add(col);

            foreach (var row in table.Rows)
            {
                row.Values.Add("");
            }
            return true;
        }

        public void CreateRow(Table table)
        {
            var newRow = new Row();
            newRow.Values.AddRange(Enumerable.Repeat("", table.Columns.Count));
            table.Rows.Add(newRow);
        }

        public void MoveRows(Table table, int from, int to)
        {
            foreach (var r in table.Rows)
            {
                if (r.Values == null) continue;
                while (r.Values.Count < table.Columns.Count) r.Values.Add("");
                MoveInModel(r.Values, from, to);
            }
        }
        public void MoveInModel<T>(List<T> colOrRowList, int from, int to)
        {
            var colFrom = colOrRowList[from];
            colOrRowList.RemoveAt(from);
            colOrRowList.Insert(to, colFrom);
        }

        public bool DeleteColumn(Table table, int index)
        {
            table.Columns.RemoveAt(index);

            foreach (var row in table.Rows)
            {
                if (row.Values.Count > index)
                    row.Values.RemoveAt(index);
            }
            return true;
        }
    }
}

using NUnit.Framework;
using NUnit.Framework.Legacy;
using ITlab1;
using System.IO;

namespace UnitTests
{
    public class Test
    {
        [Test]
        public void TxtFileValidation_Success()
        {
            TxtPath txt = new();

            string tempTxtFilePath = Path.GetTempFileName() + ".txt";
            File.WriteAllText(tempTxtFilePath, "Hello World!");

            bool isValid = txt.Validation(tempTxtFilePath);
            ClassicAssert.IsTrue(isValid, "The .txt file should be valid.");

            File.Delete(tempTxtFilePath);
        }

        [Test]
        public void TxtFileValidation_Failure()
        {
            TxtPath txt = new();

            string tempTxtFilePath = Path.GetTempFileName() + ".txt";
            // \u0017\u0001 - керівні символи бінарного файлу
            File.WriteAllText(tempTxtFilePath, "\u0017\u0001");

            bool isValid = txt.Validation(tempTxtFilePath);
            ClassicAssert.IsFalse(isValid, "The .txt file should not be valid. There were used binary symbols.");

            File.Delete(tempTxtFilePath);
        }

        [Test]
        public void RearrangementOfColumns_Success() // для тестування індивідуальної операції - перестановка колонок
        {
            Manager manager = new Manager();
            manager.CreateDatabase("testDB1");
            Table table = manager.CreateTable("testTable1");

            manager.CreateColumn(table, "column1", "Integer");
            manager.CreateColumn(table, "column2", "Integer");
            manager.CreateColumn(table, "column3", "Integer");
            manager.CreateRow(table);
            table.Rows[0].Values[0] = "10";
            table.Rows[0].Values[1] = "20";
            table.Rows[0].Values[2] = "30";

            manager.MoveInModel(table.Columns, 0, 2); // колонку1 поставити на місце колонки3, а колонка3 зміщується вліво на 2 позицію
            manager.MoveRows(table, 0, 2);

            ClassicAssert.AreEqual("column2", table.Columns[0].Name, "The first column should be 'column2'.");
            ClassicAssert.AreEqual("column3", table.Columns[1].Name, "The second column should be 'column3'.");
            ClassicAssert.AreEqual("column1", table.Columns[2].Name, "The third column should be 'column1'.");
            ClassicAssert.AreEqual("20", table.Rows[0].Values[0], "The first value should be '20'.");
            ClassicAssert.AreEqual("30", table.Rows[0].Values[1], "The second value should be '30'.");
            ClassicAssert.AreEqual("10", table.Rows[0].Values[2], "The third value should be '10'.");
        }

        [Test]
        public void DeleteColumn_Success()
        {
            Manager manager = new Manager();
            manager.CreateDatabase("testDB2");
            Table table = manager.CreateTable("testTable2");

            manager.CreateColumn(table, "column1", "Integer");
            manager.CreateColumn(table, "column2", "Integer");
            manager.CreateRow(table);
            table.Rows[0].Values[0] = "10";
            table.Rows[0].Values[1] = "20";

            bool result = manager.DeleteColumn(table, 1);
            
            ClassicAssert.IsTrue(result, "Column deletion should succeed.");
            ClassicAssert.AreEqual(1, table.Rows[0].Values.Count, "К-сть значень у рядку має відповідати к-сті колонок.");
            ClassicAssert.AreEqual("10", table.Rows[0].Values[0], "Після видалення другої колонки має лишитися перше значення.");
            ClassicAssert.AreEqual(1, table.Columns.Count, "After deletion there should be only one column.");
        }

    }
}

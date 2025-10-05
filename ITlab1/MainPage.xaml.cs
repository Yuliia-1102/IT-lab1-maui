using CommunityToolkit.Maui.Storage;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataGrid.Helper;
using Syncfusion.Maui.TabView;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ITlab1
{
    public partial class MainPage : ContentPage
    {
        Database? db;
        Manager manager = new Manager();
        DataTable? dt;
        IFileSaver fileSaver;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Table? CurrentTable =>
            (tabView.SelectedIndex < 0 || tabView.SelectedIndex >= db?.Tables.Count)
                ? null
                : tabView.Items[(int)tabView.SelectedIndex].BindingContext as Table;
        
        string _cellValue = "";
        string _cellNewValue = "";
        int _selectedIndexForColDel = -1;
        string _selectedColumnName = "";
        public List<string> DataTypes { get; } = new List<string> { "Integer", "IntegerInvl", "Char", "String", "Real", "TxtPath" };
        //public ObservableCollection<string> tablesName { get; set; } = new ObservableCollection<string>(); // for comboBox filling of tables names
        //public ObservableCollection<string> SelectedTablesList { get; set; } = new ObservableCollection<string>(); // for selected tables names from combobox
        public MainPage(IFileSaver fileSaver)
        {
            InitializeComponent();
            comboBox.BindingContext = this;
            //comboBoxTables.BindingContext = this;
            this.fileSaver = fileSaver;
        }

        //private void UpdateTableList()
        //{
        //    comboBoxTables.Clear();
        //    tablesName.Clear();

        //    if (db == null) return;
        //    foreach (var n in db.Tables.Select(t => t.Name))
        //        tablesName.Add(n);
        //}
        private void DataGrid_QueryColumnDragging(object sender, DataGridQueryColumnDraggingEventArgs e) 
        {
            if (e.DraggingAction != DataGridDragAction.DragEnded)
                return;

            var table = CurrentTable;
            if (table == null || table.Columns == null || table.Columns.Count == 0)
                return;

            int from = e.From;
            int to = e.To;

            if (from < 0 || from >= table.Columns.Count || to < 0 || to >= table.Columns.Count)
                return;
            if (from == to) return;

            manager.MoveInModel(table.Columns, from, to);
            manager.MoveRows(table, from, to);
        }

        async private void OnLoadTxtPath(object sender, EventArgs e)
        {
            try
            {
                if (db == null) return;

                var txtType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                      { DevicePlatform.WinUI,       new[] { ".txt" } },                 
                      { DevicePlatform.Android,     new[] { "text/plain" } },           
                      { DevicePlatform.iOS,         new[] { "public.plain-text" } },    
                      { DevicePlatform.MacCatalyst, new[] { "public.plain-text" } },    
                      { DevicePlatform.Tizen,       new[] { "*/*" } }
                });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Please select a .txt file",
                    FileTypes = txtType
                });

                if (result == null) return;

                if (result.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = await result.OpenReadAsync())
                    {
                        if (!IsLikelyTextFile(stream))
                        {
                            await DisplayAlert("Error", "Файл виглядає як бінарний, а не текстовий.", "OK");
                            TxtPathEntry.Text = "";
                            return;
                        }
                    }
                    
                    await DisplayAlert("Information", "Валідація текстового файлу пройшла успішно.", "OK");
                    TxtPathEntry.Text = result.FullPath;
                }
                else
                {
                    await DisplayAlert("Error", "Extension of a file is not .txt", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"{ex.Message}", "OK");
                return;
            }
        }
        private bool IsLikelyTextFile(Stream stream) // перевірка чи файл текстовий, а не бінарний
        {
            using (var reader = new StreamReader(stream))
            {
                var buffer = new char[4096];
                var bytesRead = reader.Read(buffer, 0, buffer.Length);

                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == '\0') 
                        return false;
                    if (char.IsControl(buffer[i]) &&
                        buffer[i] != '\r' &&
                        buffer[i] != '\n' &&
                        buffer[i] != '\t')
                        return false;
                }
                return true;
            }
        }

        async private void OnLoadDatabase(object sender, EventArgs e)
        {
            try
            {
                if (db != null)
                {
                    bool answer = await DisplayAlert("Warning", "Loading a new database will lose all unsaved changes to the current database. Do you want to continue?", "Yes", "No");
                    if (!answer) return;
                }

                tabView.Items.Clear();
                dataGrid.Columns.Clear();
                dt?.Rows.Clear();
                dt?.Columns.Clear();
                //comboBoxTables.Clear();
                //tablesName.Clear();
                TxtPathEntry.Text = "";
                db = null;
                dt = null;
                DbNameLabel.Text = "Current database: ";

                var jsonType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                     { DevicePlatform.WinUI, new[] { ".json" } },       
                     { DevicePlatform.Android, new[] { "application/json", "text/json" } }, 
                     { DevicePlatform.iOS, new[] { "public.json", "public.text" } },
                     { DevicePlatform.MacCatalyst, new[] { "public.json", "public.text" } },    
                     { DevicePlatform.Tizen, new[] { "*/*" } }
                });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Please select a .json file",
                    FileTypes = jsonType
                });

                if (result == null) return;

                if (result.FileName.EndsWith("json", StringComparison.OrdinalIgnoreCase))
                {
                    db = await manager.LoadDatabaseAsync(result);
                    //using var stream = await result.OpenReadAsync();
                    //var _database = JsonSerializer.Deserialize<Database>(stream);

                    //db = _database;
                    if (db?.Name == null)
                    {
                        db = null;
                        MainScroll.IsVisible = false;
                        await DisplayAlert("Error", "Not valid json file for deserialization.", "OK");
                        return;
                    }
                    ShowAfterLoadingData(db);
                }
                else
                {
                    db = null;
                    MainScroll.IsVisible = false;
                    await DisplayAlert("Error", "Extension of a file is not .json", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                db = null;
                MainScroll.IsVisible = false;
                await DisplayAlert("Error", $"{ex.Message}", "OK");
                return;
            }
        }
        private void ShowAfterLoadingData(Database loaded)
        {
            tabView.Items.Clear();
            dataGrid.Columns.Clear();
            dt?.Rows.Clear();
            dt?.Columns.Clear();
            //comboBoxTables.Clear();
            //tablesName.Clear();
            TxtPathEntry.Text = "";
            dt = null;
            dataGrid.ItemsSource = null;

            foreach (var table in loaded.Tables)
            {
                SfTabItem tab = new SfTabItem { Header = table.Name };
                tab.BindingContext = table; // прив'язка моделі таблиці до вкладки
                tabView.Items.Add(tab); // додавання TabView графічно
            }
            if (tabView.Items.Count > 0)
            {
                tabView.SelectedIndex = 0; // переключаємось на перший таб відразу
                var table = CurrentTable;
                if (table == null) return;
                dt = table.DataTable;
                dataGrid.ItemsSource = dt.DefaultView;
                VisualizeGrid(table);
            }

            //UpdateTableList();
            DbNameLabel.Text = "Current database: " + loaded.Name;
            MainScroll.IsVisible = true;
        }

        async private void OnSaveDatabaseToFile(object sender, EventArgs e)
        {
            if (db == null)
            {
                await DisplayAlert("Error", "No database to save. Please create a database first.", "OK");
                return;
            }

            try
            {
                using var stream = new MemoryStream();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonSerializer.SerializeAsync(stream, db, options);

                stream.Position = 0;
                var result = await fileSaver.SaveAsync("database.json", stream, cancellationTokenSource.Token);

                if (result.IsSuccessful)
                {
                    await DisplayAlert("Information", $"Database is succesfully saved in {result.FilePath}.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Database is not saved. {result.Exception?.Message}", "OK");
                }
            }

            catch (Exception ex)
            {
                await DisplayAlert("Error", $"{ex.Message}", "OK");
            }
        }

        private void DataGrid_CurrentCellBeginEdit(object sender, DataGridCurrentCellBeginEditEventArgs e)
        {
            if (dataGrid == null || dataGrid.View == null || e == null || e.Column == null)
                return;

            _selectedIndexForColDel = e.RowColumnIndex.ColumnIndex;
            _selectedColumnName = e.Column.MappingName ?? "";

            int recordIndex = e.RowColumnIndex.RowIndex - 1;
            if (recordIndex < 0 || recordIndex >= dataGrid.View.Records.Count)
                return;

            var record = dataGrid.View.Records[recordIndex];

            var cellObj = dataGrid.GetCellValue(record.Data, e.Column.MappingName);
            _cellValue = cellObj?.ToString() ?? "";
        }

        private void DataGrid_CurrentCellEndEdit(object sender, DataGridCurrentCellEndEditEventArgs e)
        {
            var table = CurrentTable;
            if (table == null) return;

            string columnName = _selectedColumnName;

            int recordIndex = dataGrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
            if (recordIndex < 0 || recordIndex >= table.Rows.Count) return;

            int colIndex = table.Columns.FindIndex(c => c.Name == columnName);
            if (colIndex < 0) return;

            string _cellNewValue = e.NewValue?.ToString() ?? "";

            if (db == null) return;

            var column = db.Tables[(int)tabView.SelectedIndex].Columns[colIndex];
            if (column.ColumnType.Validation(_cellNewValue) || _cellNewValue == "")
            {
                table.Rows[recordIndex].Values[colIndex] = _cellNewValue;
            }
            else
            {
                e.Cancel = true;    
                DisplayAlert("Invalid value", $"Value '{_cellNewValue}' is not valid for column '{column.Name}' of type '{column.TypeName}'.", "Ok");
            }
        }

        private void tabView_SelectionChanged(object sender, TabSelectionChangedEventArgs e)
        {
            var table = CurrentTable;
            if (table == null) return;
            dt = table.DataTable;
            dataGrid.ItemsSource = dt.DefaultView;
            VisualizeGrid(table);
        }

        private void VisualizeGrid(Table table)
        {
            if (table == null) return;

            var dataTable = table.DataTable;
            dataTable.Columns.Clear();
            dataTable.Rows.Clear();
            dataGrid.Columns.Clear();

            foreach (var col in table.Columns)
                dataTable.Columns.Add(col.Name);

            foreach (Column c in table.Columns)
            {
                dataGrid.Columns.Add(new DataGridTextColumn // візуально додаємо колонку
                {
                    MappingName = c.Name,
                    HeaderText = c.Name
                });
            }

            foreach (Row r in table.Rows)
            {
                var dr = dataTable.NewRow(); 
                for (int i = 0; i < table.Columns.Count; i++)
                    dr[i] = (i < r.Values.Count ? r.Values[i] : "");
                dataTable.Rows.Add(dr); // візуально додається рядок через dataGrid.ItemsSource = dataTable.DefaultView;
            }
        }

        private void OnAddRow(object sender, EventArgs e) 
        {
            if (db is null) return;

            var table = CurrentTable;
            if (table == null || table.Columns.Count == 0) return;

            manager.CreateRow(table);
            //var newRow = new Row();
            //newRow.Values.AddRange(Enumerable.Repeat("", table.Columns.Count));
            //table.Rows.Add(newRow);

            VisualizeGrid(table);
        }
        private void OnDeleteRow(object sender, EventArgs e)
        {
            if (db is null) return;

            var table = CurrentTable;
            var rowView = dataGrid.SelectedRow as DataRowView;
            if (table == null || rowView == null || dt == null || table.Columns == null)
            {
                DisplayAlert("Nothing selected", "Please select a cell in the row you want to delete.", "OK");
                return;
            }

            int index = dt.Rows.IndexOf(rowView.Row);
            if (index >= 0)
            {
                table.Rows.RemoveAt(index);
                dt.Rows.RemoveAt(index);
                VisualizeGrid(table);
            }
        }

        private void OnAddColumn(object sender, EventArgs e) // натиснувши на кнопку, додаємо стовпчик 
        {
            if (db is null) return;

            if (!string.IsNullOrEmpty(ColumnNameEntry.Text) && !string.IsNullOrEmpty(comboBox.Text))
            {
                var table = CurrentTable;
                if (table == null) return;

                if (table.Columns.Any(t => t.Name.Equals(ColumnNameEntry.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    DisplayAlert("Can't create a column", "Column with such name already exists.", "OK");
                    return;
                }

                string columnName = ColumnNameEntry.Text;
                manager.CreateColumn(table, columnName, comboBox.Text);
                //Column col = new Column(columnName, comboBox.Text);

                //table.Columns.Add(col);

                //foreach (var row in table.Rows)
                //{
                //    row.Values.Add("");
                //}
                VisualizeGrid(table);
                
                ColumnNameEntry.Text = string.Empty;
                comboBox.Text = string.Empty;
            }
            else
            {
                DisplayAlert("Can`t create a column", "Write a name and data type for your column.", "OK");
                return;
            }
        }
        private void OnDeleteColumn(object sender, EventArgs e) 
        {
            if (db is null) return;

            var table = CurrentTable;
            if (table == null || _selectedIndexForColDel == -1)
            {
                DisplayAlert("Nothing selected", "Please select a cell in the column you want to delete.", "OK");
                return;
            } 
           
            manager.DeleteColumn(table, _selectedIndexForColDel);

            //table.Columns.RemoveAt(_selectedIndexForColDel);
            //foreach (var row in table.Rows)
            //{
            //    if (row.Values.Count > _selectedIndexForColDel)
            //        row.Values.RemoveAt(_selectedIndexForColDel);
            //}

            VisualizeGrid(table);
            _selectedIndexForColDel = -1;
        }

        private void OnAddTable(object sender, EventArgs e) // натиснувши на кнопку, додаємо таблицю 
        {
            if (db is null) return;

            if (!string.IsNullOrEmpty(TableNameEntry.Text))
            {
                if (db.Tables.Any(t => t.Name.Equals(TableNameEntry.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    DisplayAlert("Can't create a table", "Table with such name already exists.", "OK");
                    return;
                }

                Table tbl = manager.CreateTable(TableNameEntry.Text);
                //Table tbl = new Table(TableNameEntry.Text);
                //db.Tables.Add(tbl);

                SfTabItem tab = new SfTabItem { Header = TableNameEntry.Text };
                tab.BindingContext = tbl; // прив'язка моделі таблиці до вкладки

                tabView.Items.Add(tab); // додавання TabView графічно
                tabView.SelectedIndex = tabView.Items.Count - 1; // переключаємось на доданий таб відразу

                dt = tbl.DataTable;
                dataGrid.ItemsSource = dt.DefaultView;

                TableNameEntry.Text = string.Empty;
                //UpdateTableList();
            }
            else
            {
                DisplayAlert("Can`t create a table", "Write a name for your table.", "OK");
                return;
            }
        }
        private void OnDeleteTable(object sender, EventArgs e) // натиснувши на кнопку, видаляємо обраний TabView
        {
            if (db is null) return;

            if (tabView.SelectedIndex < 0 || tabView.SelectedIndex >= tabView.Items.Count)
            {
                DisplayAlert("Nothing selected", "Please select a tab to delete.", "OK");
                return;
            }

            var selectedTab = tabView.Items[(int)tabView.SelectedIndex];
            string tableName = selectedTab.Header;

            var index = db.Tables.FindIndex(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                db.Tables.RemoveAt(index);

            tabView.Items.Remove(selectedTab);
            //UpdateTableList();

            if (tabView.Items.Count > 0)
            {
                int newIndex = Math.Min(index, tabView.Items.Count - 1);
                tabView.SelectedIndex = newIndex;

                var table = CurrentTable;
                if (table == null) return;
                dt = table.DataTable;
                dataGrid.ItemsSource = dt.DefaultView;
                VisualizeGrid(table);
            }
            else
            {
                dt = null;
                dataGrid.ItemsSource = null;
                dataGrid.Columns.Clear();
            }
        }

        private async void OnCreateDatabase(object sender, EventArgs e) //форма викликається, щоб вказати ім'я бази даних і створити її
        {
            if (db != null)
            {
                bool answer = await DisplayAlert("Warning", "Creating a new database will lose all unsaved changes to the current database. Do you want to continue?", "Yes", "No");
                if (!answer) return;
            }  

            tabView.Items.Clear();
            dataGrid.Columns.Clear();
            dt?.Rows.Clear();
            dt?.Columns.Clear();
            //comboBoxTables.Clear();
            //tablesName.Clear();
            db = null;
            dt = null;
            TxtPathEntry.Text = "";
            DbNameLabel.Text = "Current database: ";

            string result = await DisplayPromptAsync("Create a new database", "Please, name your DB: ", maxLength: 10);

            if (!string.IsNullOrEmpty(result))
            {
                db = manager.CreateDatabase(result);
                //db = new Database(result);
                DbNameLabel.Text = "Current database: " + db.Name;

                MainScroll.IsVisible = true;
            }
        }
    }
}

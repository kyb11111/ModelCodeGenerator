using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Data.Common;

namespace ModelCodeGenerator
{
    /// <summary>
    /// SechmaViewer.xaml 的交互逻辑
    /// </summary>
    public partial class SchemaViewer : Window
    {
        private DataGridView dgvTables = new DataGridView();
        private DataGridView dgvColumns = new DataGridView();
        private DbConnection conn;

        public SchemaViewer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            conn = SystemUtility.GetDbConnection(SystemUtility.m_project.DatabaseType);
            conn.ConnectionString = SystemUtility.m_project.ConnectionString;
            conn.Open();
            dgvTables.DataSource = conn.GetSchema("Tables");
            conn.Close();

            dgvTables.ReadOnly = true;
            dgvTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTables.SelectionChanged += new EventHandler(dgvTables_SelectionChanged);
            wfhTables.Child = dgvTables;

            wfhColumns.Child = dgvColumns;
        }

        void dgvTables_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvTables.SelectedRows.Count <= 0)
                return;
            conn.Open();
            string tableName = dgvTables.SelectedRows[0].Cells["TABLE_NAME"].Value.ToString();
            dgvColumns.DataSource = conn.GetSchema("Columns", new string[] { null, null, tableName });
            conn.Close();
        }
    }
}

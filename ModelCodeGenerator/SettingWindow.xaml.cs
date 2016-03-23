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
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Odbc;
using MySql.Data.MySqlClient;
using System.Data;

namespace ModelCodeGenerator
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private ProjectInfo m_project;
        public SettingWindow(ProjectInfo project)
        {
            InitializeComponent();
            m_project = project;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbDatabase.Items.Clear();
            cmbDatabase.Items.Add(SystemUtility.DBType_SqlServer);
            cmbDatabase.Items.Add(SystemUtility.DBType_SCODBC);
            cmbDatabase.Items.Add(SystemUtility.DBType_MySql);
            cmbDatabase.SelectedItem = m_project.DatabaseType;

            txtProjectName.Text = m_project.ProjectName;
            txtOutputPath.Text = m_project.OutputPath;
            txtConnectionString.Text = m_project.ConnectionString;
            txtRootClass.Text = m_project.RootClassName;
        }

        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            DbConnection conn = SystemUtility.GetDbConnection(cmbDatabase.Text);
            conn.ConnectionString = txtConnectionString.Text;
            conn.Open();
            GetTable(conn);
            conn.Close();
        }

        private void GetTable(DbConnection conn)
        {
            DataTable schema = conn.GetSchema("Tables");
            foreach (TableInfo info in m_project.Tables)
            {
                info.HasError = true;
            }
            foreach (DataRow row in schema.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();
                TableInfo info = m_project.FindTable(tableName);
                if (info == null)
                {
                    info = new TableInfo() { TableName = tableName };
                    info.ClassName = tableName;
                    m_project.Tables.Add(info);
                }
                info.HasError = false;
                GetColumn(conn, info);
            }
        }

        private void GetColumn(DbConnection conn,TableInfo tinfo)
        {
            DataTable schema = conn.GetSchema("Columns", new string[] { null, null, tinfo.TableName });
            foreach (ColumnInfo info in tinfo.Columns)
            {
                info.HasError = true;
            }
            foreach (DataRow row in schema.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();
                ColumnInfo info = tinfo.FindColumn(columnName);
                if (info == null)
                {
                    info = new ColumnInfo() { ColumnName = columnName};
                    info.PropertyName = columnName;
                    tinfo.Columns.Add(info);
                }
                info.PropertyType = Mapping(row["DATA_TYPE"].ToString());
                info.HasError = false;
            }
        }

        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtOutputPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            m_project.ProjectName = txtProjectName.Text;
            m_project.OutputPath = txtOutputPath.Text;
            m_project.ConnectionString = txtConnectionString.Text;
            m_project.RootClassName = txtRootClass.Text;
            m_project.DatabaseType = cmbDatabase.SelectedItem.ToString();
            this.Close();
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private Type Mapping(string typeName)
        {
            switch (cmbDatabase.SelectedItem.ToString())
            {
                case SystemUtility.DBType_SqlServer:
                    return MappingSqlServer(typeName);
                case SystemUtility.DBType_SCODBC:
                    return MappingSCODBC(typeName);
                case SystemUtility.DBType_MySql:
                    return MappingMySql(typeName);
                default:
                    return typeof(string);
            }
        }

        private Type MappingSCODBC(string typeName)
        {
            switch (int.Parse(typeName))
            {
                case 4:
                    return typeof(int);
                case 12:
                    return typeof(string);
                case 6:
                    return typeof(float);
                case 8:
                    return typeof(double);
                case 5:
                    return typeof(short);
                case -7:
                    return typeof(bool);
                case -5:
                    return typeof(long);
                case -6:
                    return typeof(byte);
                default:
                    return typeof(string);
            }
        }

        private Type MappingSqlServer(string typeName)
        {
            switch (typeName)
            {
                case "int":
                    return typeof(int);
                case "varchar":
                    return typeof(string);
                case "float":
                    return typeof(double);
                case "smallint":
                    return typeof(short);
                case "bit":
                    return typeof(bool);
                case "bigint":
                    return typeof(long);
                case "datetime":
                    return typeof(DateTime);
                case "tinyint":
                    return typeof(byte);
                default:
                    return typeof(string);
            }
        }

        private Type MappingMySql(string typeName)
        {
            switch (typeName)
            {
                case "int":
                    return typeof(int);
                case "varchar":
                    return typeof(string);
                case "float":
                    return typeof(double);
                case "double":
                    return typeof(double);
                case "smallint":
                    return typeof(short);
                case "bit":
                    return typeof(bool);
                case "bigint":
                    return typeof(long);
                case "datetime":
                    return typeof(DateTime);
                case "tinyint":
                    return typeof(byte);
                default:
                    return typeof(string);
            }
        }
    }
}

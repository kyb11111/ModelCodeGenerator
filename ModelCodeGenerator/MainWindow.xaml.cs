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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms.Integration;
using System.Data.Odbc;
using System.Xml.Serialization;
using System.IO;

namespace ModelCodeGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = System.Environment.CurrentDirectory + "ModelCodeGenerator.cfg";
            if (File.Exists(filename))
            {
                LoadSetting(filename);
            }
            lbTables.ItemsSource = SystemUtility.m_project.Tables;
        }

        private void lbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbTables.SelectedItem != null)
                editor.Table = lbTables.SelectedItem as TableInfo;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow win = new SettingWindow(SystemUtility.m_project);
            win.ShowDialog();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            SystemUtility.m_project.Tables.Clear();
            editor.Table = null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml documents (.xml)|*.xml";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                XmlSerializer xs = new XmlSerializer(typeof(ProjectInfo));
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                xs.Serialize(fs, SystemUtility.m_project);
                fs.Close();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml documents (.xml)|*.xml";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                LoadSetting(filename);
            }
            lbTables.ItemsSource = SystemUtility.m_project.Tables;
        }

        private void btnGenModel_Click(object sender, RoutedEventArgs e)
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += "\\Server\\";
            if (Directory.Exists(path))
            {
                MessageBox.Show("项目已存在,是否删除?", "是否删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                Directory.Delete(path, true);
            }

            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            SystemUtility.BuildServerPorject(guid1);
            //SystemUtility.BuildServerSln(guid2, guid1);
            MessageBox.Show("生成完成!");
        }

        private void btnGenServer_Click(object sender, RoutedEventArgs e)
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += "\\Server\\";
            if (Directory.Exists(path))
            {
                MessageBox.Show("项目已存在,是否删除?", "是否删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                Directory.Delete(path, true);
            }

            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            SystemUtility.BuildServicePorject(guid2, guid1);
            //SystemUtility.BuildServerSln(guid2, guid1);
            MessageBox.Show("生成完成!");
        }

        private void btnGenClient_Click(object sender, RoutedEventArgs e)
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += "\\Client\\";
            if (Directory.Exists(path))
            {
                MessageBox.Show("项目已存在,是否删除?", "是否删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                Directory.Delete(path, true);
            }

            SystemUtility.BuildClientPorject(Guid.NewGuid());
            MessageBox.Show("生成完成!");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string filename = System.Environment.CurrentDirectory + "ModelCodeGenerator.cfg";
            XmlSerializer xs = new XmlSerializer(typeof(ProjectInfo));
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            xs.Serialize(fs, SystemUtility.m_project);
            fs.Close();
        }

        private void btnAllGen_Click(object sender, RoutedEventArgs e)
        {
            foreach (TableInfo info in SystemUtility.m_project.Tables)
            {
                info.IsGenerateCode = true;
            }
        }

        private void btnNonGen_Click(object sender, RoutedEventArgs e)
        {
            foreach (TableInfo info in SystemUtility.m_project.Tables)
            {
                info.IsGenerateCode = false;
            }
        }

        private void btnViewSchema_Click(object sender, RoutedEventArgs e)
        {
            SchemaViewer win = new SchemaViewer();
            win.ShowDialog();
        }

        private void LoadSetting(string filename)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ProjectInfo));
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            SystemUtility.m_project = (ProjectInfo)xs.Deserialize(fs);
            fs.Close();
            foreach (TableInfo t in SystemUtility.m_project.Tables)
            {
                foreach (ColumnInfo c in t.Columns)
                {
                    string s = c.ParentTableName;
                    c.ParentTableName = string.Empty;
                    c.ParentTableName = s;
                }
            }
        }
    }
}

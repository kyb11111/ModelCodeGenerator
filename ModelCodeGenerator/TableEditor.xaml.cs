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

namespace ModelCodeGenerator
{
    /// <summary>
    /// TableEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TableEditor : UserControl
    {
        public TableEditor()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cmbCacheMode.Items.Clear();
            cmbCacheMode.Items.Add("不存");
            cmbCacheMode.Items.Add("服务器/客户端");
            cmbCacheMode.Items.Add("服务器");
            cmbCacheMode.Items.Add("客户端");
        }

        private TableInfo m_Table;
        public TableInfo Table
        {
            get
            {
                return m_Table;
            }
            set
            {
                if (value == null)
                {
                    this.IsEnabled = false;
                    gridErrorBlock.Visibility = Visibility.Visible;
                    return;
                }
                this.IsEnabled = true;
                editor.Column = null;
                m_Table = value;
                gridErrorBlock.Visibility = m_Table.HasError ? Visibility.Visible : Visibility.Collapsed;
                //chkGen.SetBinding(CheckBox.IsCheckedProperty,new Binding("IsGenerateCode"));
                //chkGen.DataContext = m_Table;
                chkGen.IsChecked = m_Table.IsGenerateCode;
                txtClassName.Text = m_Table.ClassName;
                dgColumns.ItemsSource = m_Table.Columns;

                cmbParent.Items.Clear();
                cmbParent.Items.Add("(none)");
                foreach (TableInfo tinfo in SystemUtility.m_project.Tables)
                {
                    cmbParent.Items.Add(tinfo);
                }
 
                if (m_Table.ParentTable != null)
                {
                    foreach (TableInfo tinfo in SystemUtility.m_project.Tables)
                    {
                        if (tinfo.TableName == m_Table.ParentTable.TableName)
                            cmbParent.SelectedItem = tinfo;
                    }
                }
                else
                {
                    cmbParent.SelectedItem = null;
                }

                switch (m_Table.CacheMode)
                {
                    case ModelCodeGenerator.CacheMode.Both:
                        cmbCacheMode.SelectedItem = "服务器/客户端";
                        break;
                    case ModelCodeGenerator.CacheMode.CacheInClient:
                        cmbCacheMode.SelectedItem = "客户端";
                        break;
                    case ModelCodeGenerator.CacheMode.CacheInServer:
                        cmbCacheMode.SelectedItem = "服务器";
                        break;
                    case ModelCodeGenerator.CacheMode.NoCache:
                        cmbCacheMode.SelectedItem = "不存";
                        break;
                    default:
                        break;
                }
            }
        }

        private void chkGen_Checked(object sender, RoutedEventArgs e)
        {
            if (Table == null) return;
            Table.IsGenerateCode = chkGen.IsChecked.Value;
            txtClassName.IsEnabled = chkGen.IsChecked.Value;
            cmbParent.IsEnabled = chkGen.IsChecked.Value;
            cmbCacheMode.IsEnabled = chkGen.IsChecked.Value;
            dgColumns.IsEnabled = chkGen.IsChecked.Value;
            editor.IsEnabled = chkGen.IsChecked.Value;
        }

        private void txtClassName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Table == null) return;
            Table.ClassName = txtClassName.Text;
        }

        private void dgColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgColumns.SelectedItem != null)
            {
                editor.Column = dgColumns.SelectedItem as ColumnInfo;
            }
        }

        private void cmbParent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_Table.ParentTable = cmbParent.SelectedItem as TableInfo;
        }

        private void cmbCacheMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCacheMode.SelectedItem != null)
            {
                string mode = cmbCacheMode.SelectedItem.ToString();
                switch (mode)
                {
                    case "服务器/客户端":
                        m_Table.CacheMode = ModelCodeGenerator.CacheMode.Both;
                        break;
                    case "客户端":
                        m_Table.CacheMode = ModelCodeGenerator.CacheMode.CacheInClient;
                        break;
                    case "服务器":
                        m_Table.CacheMode = ModelCodeGenerator.CacheMode.CacheInServer;
                        break;
                    case "不存":
                        m_Table.CacheMode = ModelCodeGenerator.CacheMode.NoCache;
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnAllGen_Click(object sender, RoutedEventArgs e)
        {
            foreach (ColumnInfo info in m_Table.Columns)
            {
                info.IsGenerateCode = true;
            }
        }

        private void btnNonGen_Click(object sender, RoutedEventArgs e)
        {
            foreach (ColumnInfo info in m_Table.Columns)
            {
                info.IsGenerateCode = false;
            }
        }

        private void btnGenClient_Click(object sender, RoutedEventArgs e)
        {
            ClassCodeBuilder cb = new ClassCodeBuilder(m_Table);
            cb.BuildClientFile();
            MessageBox.Show("finished!");
        }

        private void btnGenServer_Click(object sender, RoutedEventArgs e)
        {
            ClassCodeBuilder cb = new ClassCodeBuilder(m_Table);
            cb.BuildServerFile();
            MessageBox.Show("finished!");
        }
    }
}

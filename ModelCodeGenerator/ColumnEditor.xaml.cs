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
    /// ColumnEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnEditor : UserControl
    {
        public ColumnEditor()
        {
            InitializeComponent();
            this.IsEnabled = false;
            cmbDataType.Items.Add(typeof(bool));
            cmbDataType.Items.Add(typeof(byte));
            cmbDataType.Items.Add(typeof(short));
            cmbDataType.Items.Add(typeof(int));
            cmbDataType.Items.Add(typeof(long));
            cmbDataType.Items.Add(typeof(float));
            cmbDataType.Items.Add(typeof(double));
            cmbDataType.Items.Add(typeof(string));
            cmbDataType.Items.Add(typeof(DateTime));
        }

        private ColumnInfo m_Column;
        public ColumnInfo Column
        {
            get
            {
                return m_Column;
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
                m_Column = value;
                gridErrorBlock.Visibility = m_Column.HasError ? Visibility.Visible : Visibility.Collapsed;
                chkGen.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsGenerateCode"));
                chkGen.DataContext = m_Column;
                chkAltKey.IsChecked = m_Column.IsAltKey;
                chkRealtime.IsChecked = m_Column.IsRealTime;
                chkDbfield.IsChecked = m_Column.IsDbfield;
                txtPropertyName.Text = m_Column.PropertyName;
                cmbDataType.SelectedItem = m_Column.PropertyType;
                cmbParent.Items.Clear();
                cmbParent.Items.Add("(none)");
                foreach (TableInfo tinfo in m_Column.TableInfo.ProjectInfo.Tables)
                {
                    cmbParent.Items.Add(tinfo);
                }
                if (m_Column.TableInfo.ParentRelation.ContainsKey(m_Column))
                    cmbParent.SelectedItem = m_Column.TableInfo.ParentRelation[m_Column];
                else
                    cmbParent.SelectedItem = "(none)";
            }
        }

        private void chkGen_Checked(object sender, RoutedEventArgs e)
        {
            if (Column == null) return;
            Column.IsGenerateCode = chkGen.IsChecked.Value;
            chkAltKey.IsEnabled = chkGen.IsChecked.Value;
            chkRealtime.IsEnabled = chkGen.IsChecked.Value;
            txtPropertyName.IsEnabled = chkGen.IsChecked.Value;
        }

        private void chkAltKey_Checked(object sender, RoutedEventArgs e)
        {
            if (Column == null) return;
            Column.IsAltKey = chkAltKey.IsChecked.Value;
        }

        private void chkRealtime_Checked(object sender, RoutedEventArgs e)
        {
            if (Column == null) return;
            Column.IsRealTime = chkRealtime.IsChecked.Value;
        }

        private void chkDbfield_Checked(object sender, RoutedEventArgs e)
        {
            if (Column == null) return;
            Column.IsDbfield = chkDbfield.IsChecked.Value;
            if (!chkDbfield.IsChecked.Value)
            {
                chkAltKey.IsChecked = false;
                chkRealtime.IsChecked = false;
            }
        }

        private void txtPropertyName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Column == null) return;
            Column.PropertyName = txtPropertyName.Text;
        }

        private void cmbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Column.PropertyType = cmbDataType.SelectedItem as Type;
        }

        private void cmbParent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParent.SelectedItem == null)
                cmbDataType.IsEnabled = true;
            else if (cmbParent.SelectedItem.ToString() == "(none)")
            {
                cmbDataType.IsEnabled = true;
                Column.ParentTableName = null;
            }
            else
            {
                cmbDataType.SelectedItem = typeof(int);
                cmbDataType.IsEnabled = false;
                Column.ParentTableName = (cmbParent.SelectedItem as TableInfo).TableName;
            }
        }
    }
}

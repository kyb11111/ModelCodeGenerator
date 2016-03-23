using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Collections.ObjectModel;

namespace ModelCodeGenerator
{
    public class ProjectInfo
    {
        private ObservableCollection<TableInfo> m_tables;

        public ProjectInfo()
        {
            ProjectName = "Test";
            OutputPath = System.Environment.CurrentDirectory;
            ConnectionString = string.Empty;
            RootClassName = "RootModel";

            //ConnectionString = "Dsn=SuperControlDB Datasource;database=scada_1w;stepapi=0;notxn=0;timeout=100000;nowchar=0;longnames=0";
            //DatabaseType = SystemUtility.DBType_SCODBC;
            ConnectionString = "Data Source=.\\tin;Initial Catalog=ecodemodb;Integrated Security=True";
            DatabaseType = SystemUtility.DBType_SqlServer;
        }

        public string ProjectName
        {
            get;
            set;
        }

        public string ClientNameSpace
        {
            get { return string.Format("SuperControl.{0}Client", ProjectName); }
        }

        public string ServerNameSpace
        {
            get { return string.Format("SuperControl.{0}Model", ProjectName); }
        }

        public string ServiceName
        {
            get { return string.Format("{0}Service", ProjectName); }
        }

        public string ServerAppName
        {
            get { return string.Format("{0}Server", ProjectName); }
        }

        public string OutputPath
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }

        public string DatabaseType
        {
            get;
            set;
        }

        public string RootClassName
        {
            get;
            set;
        }

        public ObservableCollection<TableInfo> Tables
        {
            get
            {
                if (m_tables == null)
                    m_tables = new ObservableCollection<TableInfo>();
                return m_tables;
            }
        }

        internal TableInfo FindTable(string tableName)
        {
            foreach (TableInfo info in Tables)
            {
                if (tableName == info.TableName)
                {
                    return info;
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ModelCodeGenerator
{
    public enum CacheMode
    {
        /// <summary>
        /// 不需要在存根管理器中缓存
        /// </summary>
        NoCache,
        /// <summary>
        /// 需要在WCF服务器端的存根管理器中缓存
        /// </summary>
        CacheInServer,
        /// <summary>
        /// 需要在WCF客户端的存根管理器中缓存（暂时不起作用）
        /// </summary>
        CacheInClient,
        /// <summary>
        /// 在服务器与客户端两方都进行缓存
        /// </summary>
        Both
    }

    public class TableInfo : MetaInfo
    {
        public TableInfo()
        {
        }

        public ProjectInfo ProjectInfo
        {
            get { return SystemUtility.m_project; } 
        }

        private string m_tableName = string.Empty;
        public string TableName
        {
            get { return m_tableName; }
            set
            {
                if (m_tableName == value)
                    return;
                m_tableName = value;
                OnPropertyChanged("TableName");
            }
        }

        private TableInfo m_ParentTable = null;
        public TableInfo ParentTable
        {
            get { return m_ParentTable; }
            set
            {
                if (m_ParentTable == value)
                    return;
                m_ParentTable = value;
                OnPropertyChanged("ParentTable");
            }
        }

        private CacheMode m_CacheMode = CacheMode.Both;
        public CacheMode CacheMode
        {
            get { return m_CacheMode; }
            set
            {
                if (m_CacheMode == value)
                    return;
                m_CacheMode = value;
                OnPropertyChanged("CacheMode"); 
            }
        }

        private string m_ClassName = string.Empty;
        public string ClassName
        {
            get { return m_ClassName; }
            set
            {
                if (m_ClassName == value)
                    return;
                m_ClassName = value;
                OnPropertyChanged("ClassName");
            }
        }

        private ColumnInfoCollection m_columns;
        public ColumnInfoCollection Columns
        {
            get 
            {
                if (m_columns == null)
                    m_columns = new ColumnInfoCollection(this);
                return m_columns;
            }
        }

        Dictionary<ColumnInfo, TableInfo> m_parentRelation = new Dictionary<ColumnInfo, TableInfo>();
        /// <summary>
        /// 父表列
        /// </summary>
        internal Dictionary<ColumnInfo, TableInfo> ParentRelation
        {
            get { return m_parentRelation; }
        }

        Dictionary<TableInfo, ColumnInfo> m_childrenRelation = new Dictionary<TableInfo, ColumnInfo>();
        /// <summary>
        /// 子表列
        /// </summary>
        internal Dictionary<TableInfo, ColumnInfo> ChidrenRelation
        {
            get { return m_childrenRelation; }
        }

        public override string ToString()
        {
            return m_tableName;
        }

        internal ColumnInfo FindColumn(string columnName)
        {
            foreach (ColumnInfo info in Columns)
            {
                if (columnName == info.ColumnName)
                {
                    return info;
                }
            }
            return null;
        }
    }

    public class ColumnInfoCollection : ObservableCollection<ColumnInfo>
    {
        private TableInfo m_tinfo;
        public ColumnInfoCollection(TableInfo tinfo)
        {
            m_tinfo = tinfo;
        }

        protected override void InsertItem(int index, ColumnInfo item)
        {
            item.TableInfo = m_tinfo;
            base.InsertItem(index, item);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ModelCodeGenerator
{
    public class ColumnInfo : MetaInfo
    {
        private string m_columnName = string.Empty;
        private TableInfo m_tableInfo;
        public ColumnInfo()
        {
        }

        [XmlIgnoreAttribute]
        public TableInfo TableInfo
        {
            set 
            { 
                m_tableInfo = value;
                ParentTableName = m_ParentTableName;
            }
            get { return m_tableInfo; }
        }

        private string m_ParentTableName = string.Empty;
        public string ParentTableName
        {
            set
            {
                //if (string.IsNullOrWhiteSpace(value))
                //    return;
                //m_ParentTableName = value;
                if (m_tableInfo == null)
                {
                    m_ParentTableName = value;
                    return;
                }
                TableInfo tinfo = SystemUtility.GetTableInfo(m_ParentTableName);
                if (tinfo != null)
                {
                    if (tinfo.ChidrenRelation.ContainsKey(m_tableInfo))
                        tinfo.ChidrenRelation.Remove(m_tableInfo);
                }
                if (m_tableInfo.ParentRelation.ContainsKey(this))
                    m_tableInfo.ParentRelation.Remove(this);
                m_ParentTableName = value;
                tinfo = SystemUtility.GetTableInfo(m_ParentTableName);
                if (tinfo != null)
                {
                    m_tableInfo.ParentRelation.Add(this, tinfo);
                    tinfo.ChidrenRelation.Add(m_tableInfo, this);
                }
                OnPropertyChanged("ParentTableName");
            }
            get 
            {
                return m_ParentTableName;
            }
        }

        /// <summary>
        /// 数据库字段名
        /// </summary>
        public string ColumnName
        {
            get { return m_columnName; }
            set
            {
                if (m_columnName == value)
                    return;
                m_columnName = value;
                OnPropertyChanged("ColumnName");
            }
        }

        private bool m_IsRealTime = false;
        /// <summary>
        /// 是否是实时属性
        /// </summary>
        public bool IsRealTime
        {
            get { return m_IsRealTime; }
            set
            {
                if (m_IsRealTime == value)
                    return;
                m_IsRealTime = value;
                OnPropertyChanged("IsRealTime");
            }
        }

        private bool m_IsAltKey = false;
        /// <summary>
        /// 是否有AltKey
        /// </summary>
        public bool IsAltKey
        {
            get { return m_IsAltKey; }
            set
            {
                if (m_IsAltKey == value)
                    return;
                m_IsAltKey = value;
                OnPropertyChanged("IsAltKey");
            }
        }

        private bool m_IsDbfield = true;
        /// <summary>
        /// 是否需要传到客户端
        /// </summary>
        public bool IsDbfield
        {
            get { return m_IsDbfield; }
            set
            {
                if (m_IsDbfield == value)
                    return;
                m_IsDbfield = value;
                OnPropertyChanged("IsDbfield");
            }
        }

        private string m_PropertyName = string.Empty;
        /// <summary>
        /// 模型对象属性名
        /// </summary>
        public string PropertyName
        {
            get { return m_PropertyName; }
            set
            {
                if (m_PropertyName == value)
                    return;
                m_PropertyName = value;
                OnPropertyChanged("PropertyName");
            }
        }

        private Type m_PropertyType = typeof(string);
        /// <summary>
        /// 属性类型
        /// </summary>
        [XmlIgnoreAttribute]
        public Type PropertyType
        {
            get { return m_PropertyType; }
            set
            {
                if (m_PropertyType == value)
                    return;
                m_PropertyType = value;
                OnPropertyChanged("PropertyType");
            }
        }

        /// <summary>
        /// 属性类型全名
        /// </summary>
        public string PropertyTypeFullName
        {
            get { return m_PropertyType.FullName; }
            set { m_PropertyType = Type.GetType(value); }
        }

        public override string ToString()
        {
            return m_columnName;
        }
    }
}

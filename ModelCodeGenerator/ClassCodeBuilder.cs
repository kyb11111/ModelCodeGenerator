using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModelCodeGenerator
{
    public class ClassCodeBuilder
    {
        private TableInfo m_tableInfo;
        private List<ColumnInfo> m_realTimeList = new List<ColumnInfo>();
        private List<ColumnInfo> m_altKeyList = new List<ColumnInfo>();
        public ClassCodeBuilder(TableInfo tableInfo)
        {
            m_tableInfo = tableInfo;
        }

        #region server
        private void GetPropertyCode(ColumnInfo info, StreamWriter sw)
        {
            if (info.IsGenerateCode && !info.HasError)
            {
                sw.WriteLine("       private {0} m_{1};", info.PropertyType.Name, info.PropertyName);
                if (info.IsDbfield)
                    sw.WriteLine("       [DataMember, DbField(FieldName = \"{0}\")]", info.ColumnName);
                else
                    sw.WriteLine("       [DataMember]");
                sw.WriteLine("       public {0} {1}", info.PropertyType.Name, info.PropertyName);
                sw.WriteLine("       {");
                sw.WriteLine("           get {{ return m_{0}; }}", info.PropertyName);
                sw.WriteLine("           set");
                sw.WriteLine("           {");
                sw.WriteLine("              if (m_{0} != value)", info.PropertyName);
                sw.WriteLine("              {");
                sw.WriteLine("                  m_{0} = value;", info.PropertyName);
                sw.WriteLine("                  OnPropertyChanged(\"{0}\");", info.PropertyName);
                sw.WriteLine("              }");
                sw.WriteLine("           }");
                sw.WriteLine("       }");
                sw.WriteLine();
            }
        }

        public void BuildServerFile()
        {
            if (m_tableInfo.IsGenerateCode && !m_tableInfo.HasError)
            {
                string path = SystemUtility.GetServerPath();
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string file = path + m_tableInfo.ClassName + ".cs";
                FileStream stream = new FileStream(file, FileMode.Create);
                StreamWriter sw = new StreamWriter(stream);

                foreach (ColumnInfo info in m_tableInfo.Columns)
                {
                    if (info.ColumnName.ToLower() == "rid")
                        continue;
                    if (info.IsRealTime)
                        m_realTimeList.Add(info);
                    if (info.IsAltKey)
                        m_altKeyList.Add(info);
                }

                #region using
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.Linq;");
                sw.WriteLine("using System.Text;");
                sw.WriteLine("using System.Runtime.Serialization;");
                sw.WriteLine("using SuperControl.ServiceModel;");
                #endregion

                #region ClassAttribute
                sw.WriteLine();
                sw.WriteLine("namespace {0}", SystemUtility.m_project.ServerNameSpace);
                sw.WriteLine("{");
                sw.WriteLine("  [Serializable, DataContract]");
                sw.WriteLine("  [DbTable(HasAlternateKey = {0},CacheMode = CacheMode.{1},TableName = \"{2}\",Realtime = {3})]",
                    (m_altKeyList.Count > 0).ToString().ToLower(),
                    m_tableInfo.CacheMode,
                    m_tableInfo.TableName,
                    (m_realTimeList.Count > 0).ToString().ToLower());
                #endregion

                #region Class
                string baseclass = SystemUtility.m_project.RootClassName;
                if (m_tableInfo.ParentTable != null &&
                    m_tableInfo.ParentTable.IsGenerateCode &&
                    !m_tableInfo.ParentTable.HasError)
                {
                    baseclass = m_tableInfo.ParentTable.ClassName;
                }
                sw.WriteLine("  public class {0} : {1}", m_tableInfo.ClassName, baseclass);
                sw.WriteLine("  {");

                #region Property
                foreach (ColumnInfo info in m_tableInfo.Columns)
                {
                    if (info.ColumnName.ToLower() == "rid")
                        continue;
                    GetPropertyCode(info, sw);
                }
                #endregion

                #region altkey
                if (m_altKeyList.Count > 0)
                {
                    sw.WriteLine("      public override string AlternateKey");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              string altkey = string.Empty;");
                    for (int i = 0; i < m_altKeyList.Count; i++)
                    {
                        sw.WriteLine("              altkey += {0} == null ? string.Empty : {0}.ToString();", m_altKeyList[i].PropertyName);
                        if (i < m_altKeyList.Count - 1)
                            sw.WriteLine("              altkey += '|';");
                    }
                    sw.WriteLine("              return altkey;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();
                }
                #endregion

                #region realtime
                if (m_realTimeList.Count > 0)
                {
                    sw.WriteLine("      protected override void GetRealtimeData(RealtimeData data)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          base.GetRealtimeData(data);");
                    sw.Write("          data.AddData(");
                    for (int i = 0; i < m_realTimeList.Count; i++)
                    {
                        sw.Write(m_realTimeList[i].PropertyName);
                        if (i < m_realTimeList.Count - 1)
                            sw.Write(',');
                    }
                    sw.Write(");");
                    sw.WriteLine();
                    sw.WriteLine("      }");
                    sw.WriteLine();
                }
                #endregion

                #region set value
                sw.WriteLine("      public override void SetValueWithTableName(string fieldName, object value, out bool sendImmediately)");
                sw.WriteLine("      {");
                sw.WriteLine("          base.SetValueWithTableName(fieldName, value, out sendImmediately);");
                sw.WriteLine("          switch (fieldName)");
                sw.WriteLine("          {");
                //先设置realtime部分的属性
                foreach (ColumnInfo info in m_tableInfo.Columns)
                {
                    if (!info.IsGenerateCode || info.HasError)
                        continue;
                    if (info.ColumnName.ToLower() == "rid")
                        continue;
                    if (!info.IsRealTime)
                        continue;
                    sw.WriteLine("              case \"{0}\":", info.ColumnName);
                    sw.WriteLine("                  if (!m_{0}.Equals(value))", info.PropertyName);
                    sw.WriteLine("                  {");
                    sw.WriteLine("                      m_{0} = ({1})ModelDatabaseEnumerator.ChangeType(value, typeof({1}));", info.PropertyName, info.PropertyType);
                    sw.WriteLine("                      sendImmediately = false;");
                    sw.WriteLine("                      OnPropertyChanged(\"{0}\");", info.PropertyName);
                    sw.WriteLine("                  }");
                    sw.WriteLine("                  break;");
                }
                //其他非实时属性后设置
                foreach (ColumnInfo info in m_tableInfo.Columns)
                {
                    if (!info.IsGenerateCode || info.HasError)
                        continue;
                    if (info.ColumnName.ToLower() == "rid")
                        continue;
                    if (info.IsRealTime)
                        continue;
                    sw.WriteLine("              case \"{0}\":", info.ColumnName);
                    sw.WriteLine("                  if (!m_{0}.Equals(value))", info.PropertyName);
                    sw.WriteLine("                  {");
                    sw.WriteLine("                      m_{0} = ({1})ModelDatabaseEnumerator.ChangeType(value, typeof({1}));", info.PropertyName, info.PropertyType);
                    sw.WriteLine("                      sendImmediately = true;");
                    sw.WriteLine("                      OnPropertyChanged(\"{0}\");", info.PropertyName);
                    sw.WriteLine("                  }");
                    sw.WriteLine("                  break;");
                }
                sw.WriteLine("              default:");
                sw.WriteLine("                  sendImmediately = false;");
                sw.WriteLine("                  break;");
                sw.WriteLine("          }");
                sw.WriteLine("      }");

                #endregion

                sw.WriteLine("  }");
                sw.WriteLine("}");
                sw.WriteLine();
                #endregion

                sw.Flush();
                stream.Close();
            }
        }
        #endregion

        #region client
        private void GetClientParentPropertyCode(Dictionary<ColumnInfo, TableInfo> relation, StreamWriter sw)
        {
            if (relation.Count == 0)
                return;
            List<KeyValuePair<ColumnInfo, TableInfo>> parentRelation = new List<KeyValuePair<ColumnInfo, TableInfo>>();
            foreach (KeyValuePair<ColumnInfo, TableInfo> r in relation)
            {
                if (r.Key.IsGenerateCode)
                    parentRelation.Add(r);
            }
            #region Initialize
            sw.WriteLine("      public override void Initialize()");
            sw.WriteLine("      {");
            sw.WriteLine("          base.Initialize();");
            sw.WriteLine("          this.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(SelfPropertyChanged);");
            sw.WriteLine("          this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SelfPropertyChanged);");
            foreach (KeyValuePair<ColumnInfo, TableInfo> p in parentRelation)
            {
                sw.WriteLine("          m_{0}Rid = {0};", p.Key.PropertyName);
                sw.WriteLine("          {1} tmp{0} = ModelCacheManager.Instance[typeof({1}), m_{0}Rid] as {1};", p.Key.PropertyName, p.Value.ClassName);
                sw.WriteLine("          if (tmp{0} != null && !tmp{0}.Children{1}.Contains(this))", p.Key.PropertyName, m_tableInfo.ClassName);
                sw.WriteLine("              tmp{0}.Children{1}.Add(this);", p.Key.PropertyName, m_tableInfo.ClassName);
            }
            sw.WriteLine("      }");
            sw.WriteLine();
            #endregion

            #region SelfPropertyChanged
            sw.WriteLine("      private void SelfPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)");
            sw.WriteLine("      {");
            sw.WriteLine("          switch (e.PropertyName)");
            sw.WriteLine("          {");
            foreach (KeyValuePair<ColumnInfo, TableInfo> p in parentRelation)
            {
                sw.WriteLine("              case \"{0}\":", p.Key.PropertyName);
                sw.WriteLine("                  {1} tmp{0} = ModelCacheManager.Instance[typeof({1}), m_{0}Rid] as {1};", p.Key.PropertyName, p.Value.ClassName);
                sw.WriteLine("                  if (tmp{0} != null)", p.Key.PropertyName);
                sw.WriteLine("                      tmp{0}.Children{1}.Remove(this);", p.Key.PropertyName, m_tableInfo.ClassName);
                sw.WriteLine("                  m_{0}Rid = {0};", p.Key.PropertyName);
                sw.WriteLine("                  tmp{0} = ModelCacheManager.Instance[typeof({1}), m_{0}Rid] as {1};", p.Key.PropertyName, p.Value.ClassName);
                sw.WriteLine("                  if (tmp{0} != null && !tmp{0}.Children{1}.Contains(this))", p.Key.PropertyName, m_tableInfo.ClassName);
                sw.WriteLine("                      tmp{0}.Children{1}.Add(this);", p.Key.PropertyName, m_tableInfo.ClassName);
                sw.WriteLine("                  RaisePropertyChanged(\"Parent{0}\");", p.Key.PropertyName);
                sw.WriteLine("                  break;");
            }
            sw.WriteLine("              default:");
            sw.WriteLine("                  break;");
            sw.WriteLine("          }");
            sw.WriteLine("      }");
            sw.WriteLine();
            #endregion

            #region Property
            foreach (KeyValuePair<ColumnInfo, TableInfo> p in parentRelation)
            {
                sw.WriteLine("      private int m_{0}Rid;", p.Key.PropertyName);
                sw.WriteLine("      public {1} Parent{0}", p.Key.PropertyName, p.Value.ClassName);
                sw.WriteLine("      {");
                sw.WriteLine("          get");
                sw.WriteLine("          {");
                sw.WriteLine("              return ModelCacheManager.Instance[typeof({1}), m_{0}Rid] as {1};", p.Key.PropertyName.ToString(), p.Value.ClassName);
                sw.WriteLine("          }");
                sw.WriteLine("      }");
                sw.WriteLine();
            }
            #endregion
        }

        private void GetClientChildrenPropertyCode(Dictionary<TableInfo, ColumnInfo> relation, StreamWriter sw)
        {
            if (relation.Count <= 0)
                return;
            List<KeyValuePair<TableInfo, ColumnInfo>> childrenRelation = new List<KeyValuePair<TableInfo, ColumnInfo>>();
            foreach (KeyValuePair<TableInfo, ColumnInfo> r in relation)
            {
                if (r.Key.IsGenerateCode)
                    childrenRelation.Add(r);
            }
            foreach (KeyValuePair<TableInfo, ColumnInfo> c in childrenRelation)
            {
                sw.WriteLine("      private ChidrenModelCollection<{0}> m_children{0};", c.Key.ClassName);
                sw.WriteLine("      public ChidrenModelCollection<{0}> Children{0}", c.Key.ClassName);
                sw.WriteLine("      {");
                sw.WriteLine("          get");
                sw.WriteLine("          {");
                sw.WriteLine("              if (m_children{0} == null)", c.Key.ClassName);
                sw.WriteLine("              {");
                sw.WriteLine("                  m_children{0} = new ChidrenModelCollection<{0}>();", c.Key.ClassName);
                sw.WriteLine("                  foreach ({0} child in ModelCacheManager.Instance[typeof({0})])", c.Key.ClassName);
                sw.WriteLine("                  {");
                sw.WriteLine("                      if (child.{0} == this.Rid)", c.Value.PropertyName);
                sw.WriteLine("                          m_children{0}.Add(child);", c.Key.ClassName);
                sw.WriteLine("                  }");
                sw.WriteLine("              }");
                sw.WriteLine("              return m_children{0};", c.Key.ClassName);
                sw.WriteLine("          }");
                sw.WriteLine("      }");
                sw.WriteLine();
            }

            sw.WriteLine("      private ChildrenSummaryModelCollection m_children;");
            sw.WriteLine("      public ChildrenSummaryModelCollection Children");
            sw.WriteLine("      {");
            sw.WriteLine("          get");
            sw.WriteLine("          {");
            sw.WriteLine("              if (m_children == null)");
            sw.WriteLine("              {");
            sw.WriteLine("                  m_children = new ChildrenSummaryModelCollection();");
            foreach (KeyValuePair<TableInfo, ColumnInfo> c in childrenRelation)
            {
                sw.WriteLine("                  m_children.Add(Children{0});", c.Key.ClassName);
            }
            sw.WriteLine("              }");
            sw.WriteLine("              return m_children;");
            sw.WriteLine("          }");
            sw.WriteLine("      }");
            sw.WriteLine();
        }

        public void BuildClientFile()
        {
            foreach (ColumnInfo info in m_tableInfo.Columns)
            {
                if (info.IsGenerateCode && !info.HasError)
                {
                    if (info.IsRealTime)
                        m_realTimeList.Add(info);
                    if (info.IsAltKey)
                        m_altKeyList.Add(info);
                }
            }

            if (!m_tableInfo.IsGenerateCode)
                return;
            //if (m_altKeyList.Count == 0 && m_realTimeList.Count == 0)
            //    return;
            string path = SystemUtility.GetClientProxyPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string file = path + m_tableInfo.ClassName + ".cs";
            FileStream stream = new FileStream(file, FileMode.Create);
            StreamWriter sw = new StreamWriter(stream);

            #region using
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Net;");
            sw.WriteLine("using SuperControl.ServiceModel;");
            sw.WriteLine("using System.Collections.ObjectModel;");
            #endregion

            #region ClassAttribute
            sw.WriteLine();
            sw.WriteLine("namespace {0}", SystemUtility.m_project.ClientNameSpace + ".Proxy");
            sw.WriteLine("{");
            sw.WriteLine("  [DbTable(HasAlternateKey = {0},Realtime = {1})]",
                (m_altKeyList.Count > 0).ToString().ToLower(),
                (m_realTimeList.Count > 0).ToString().ToLower());
            sw.WriteLine("  public partial class {0}", m_tableInfo.ClassName);
            sw.WriteLine("  {");
            #endregion

            #region
            GetClientParentPropertyCode(m_tableInfo.ParentRelation, sw);
            sw.WriteLine();
            GetClientChildrenPropertyCode(m_tableInfo.ChidrenRelation, sw);
            #endregion

            #region altkey
            if (m_altKeyList.Count > 0)
            {
                sw.WriteLine("      public override string AlternateKey");
                sw.WriteLine("      {");
                sw.WriteLine("          get");
                sw.WriteLine("          {");
                sw.WriteLine("              string altkey = string.Empty;");
                for (int i = 0; i < m_altKeyList.Count; i++)
                {
                    sw.WriteLine("              altkey += {0} == null ? string.Empty : {0}.ToString();", m_altKeyList[i].PropertyName);
                    if (i < m_altKeyList.Count - 1)
                        sw.WriteLine("              altkey += '|';");
                }
                sw.WriteLine("              return altkey;");
                sw.WriteLine("          }");
                sw.WriteLine("      }");
            }
            #endregion

            #region realtime
            if (m_realTimeList.Count > 0)
            {
                sw.WriteLine("      internal override void SetRealtimeData(RealtimeData data)");
                sw.WriteLine("      {");
                for (int i = 0; i < m_realTimeList.Count; i++)
                {
                    sw.WriteLine("          this.{0} = ({1})data.Data[{2}];", 
                        m_realTimeList[i].PropertyName,
                        m_realTimeList[i].PropertyType.Name,
                        i);
                }
                sw.WriteLine("      }");
            }
            #endregion

            sw.WriteLine("  }");
            sw.WriteLine("}");

            sw.Flush();
            stream.Close();
        }
        #endregion
    }
}

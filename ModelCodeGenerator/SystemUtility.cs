using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Data.SqlClient;
using System.Data.Odbc;
using MySql.Data.MySqlClient;

namespace ModelCodeGenerator
{
    public static class SystemUtility
    {
        internal static ProjectInfo m_project = new ProjectInfo();

        public const string DBType_SqlServer = "Sql Server";
        public const string DBType_SCODBC = "SCODBC";
        public const string DBType_MySql = "MySql";

        public const string VarNamespace = "$(Namespace)";
        public const string VarProjectName = "$(ProjectName)";
        public const string VarRootClassName = "$(RootClassName)";
        public const string VarDate = "$(Date)";
        public const string VarGuid = "$(Guid)";
        public const string VarIncludeFiles = "$(IncludeFiles)";
        public const string VarModelGuid = "$(ModelGuid)";
        public const string VarConnectionString = "$(ConnectionString)";
        public const string VarProviderName = "$(ProviderName)";

        #region build server project
        private static void BuildRootClass()
        {
            string path = GetServerPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string file = path + SystemUtility.m_project.RootClassName + ".cs";
            FileStream stream = new FileStream(file, FileMode.Create);
            StreamWriter sw = new StreamWriter(stream);

            #region using
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine("using System.Linq;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Runtime.Serialization;");
            sw.WriteLine("using System.ComponentModel;");
            sw.WriteLine("using SuperControl.ServiceModel;");
            #endregion

            #region Class
            sw.WriteLine();
            sw.WriteLine("namespace {0}", SystemUtility.m_project.ServerNameSpace);
            sw.WriteLine("{");
            sw.WriteLine("  [Serializable, DataContract]");
            foreach (TableInfo info in SystemUtility.m_project.Tables)
            {
                if (info.IsGenerateCode && !info.HasError)
                {
                    sw.WriteLine("  [KnownType(typeof({0}))]", info.ClassName);
                }
            }
            sw.WriteLine("  public class {0} : ModelBase , INotifyPropertyChanged", SystemUtility.m_project.RootClassName);
            sw.WriteLine("  {");
            sw.WriteLine("  }");
            sw.WriteLine("}");
            #endregion

            #region altkey
            #endregion
            sw.Flush();
            stream.Close();
        }

        public static void BuildServerPorject(Guid projectGuid)
        {
            foreach (TableInfo tinfo in SystemUtility.m_project.Tables)
            {
                ClassCodeBuilder builder = new ClassCodeBuilder(tinfo);
                builder.BuildServerFile();
            }

            BuildRootClass();

            CopyTextFile("ServerFile.AssemblyInfo.tmp",
                GetServerPath() + "Properties\\",
                "AssemblyInfo.cs",
                VarNamespace, m_project.ServerNameSpace,
                VarDate, DateTime.Now.ToString(),
                VarGuid, Guid.NewGuid().ToString()
            );

            string path = GetServerPath();
            StringBuilder sb = new StringBuilder();
            if (File.Exists(string.Format("{0}{1}.cs", path, SystemUtility.m_project.RootClassName)))
            {
                sb.AppendLine(string.Format("<Compile Include=\"{0}.cs\" />", SystemUtility.m_project.RootClassName));
            }
            foreach (TableInfo info in m_project.Tables)
            {
                if (File.Exists(string.Format("{0}{1}.cs", path, info.ClassName)))
                {
                    sb.AppendLine(string.Format("    <Compile Include=\"{0}.cs\" />", info.ClassName));
                }
            }

            CopyTextFile("ServerFile.Project.tmp",
                path,
                string.Format("{0}.csproj", m_project.ServerNameSpace),
                VarNamespace, m_project.ServerNameSpace,
                VarIncludeFiles, sb.ToString(),
                VarGuid, projectGuid.ToString()
                );

            CopyFile("ServerFile.SuperControl.ServiceModel.dll", GetServerPath(), "SuperControl.ServiceModel.dll");
        }

        public static string GetServerPath()
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += string.Format("\\Server\\{0}\\", SystemUtility.m_project.ServerNameSpace);
            return path;
        }
        #endregion

        #region build client project
        public static string GetClientProxyPath()
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += string.Format("\\Client\\{0}\\Proxy\\", SystemUtility.m_project.ClientNameSpace);
            return path;
        }

        public static string GetClientPath()
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += string.Format("\\Client\\{0}\\", SystemUtility.m_project.ClientNameSpace);
            return path;
        }

        public static void BuildClientPorject(Guid projectGuid)
        {
            foreach (TableInfo tinfo in SystemUtility.m_project.Tables)
            {
                ClassCodeBuilder builder = new ClassCodeBuilder(tinfo);
                builder.BuildClientFile();
            }

            CopyFile("ClientFile.SuperControl.ServiceModel.dll", GetClientPath(), "SuperControl.ServiceModel.dll");

            CopyTextFile("ClientFile.AssemblyInfo.tmp",
                GetClientPath() + "Properties\\",
                "AssemblyInfo.cs",
                VarNamespace, m_project.ClientNameSpace,
                VarDate, DateTime.Now.ToString(),
                VarGuid, Guid.NewGuid().ToString()
                );

            CopyTextFile("ClientFile.ModelBase.tmp",
                GetClientProxyPath(),
                "ModelBase.cs",
                VarNamespace, m_project.ClientNameSpace
                );

            CopyTextFile("ClientFile.ModelRegisteEventArgs.tmp",
                GetClientProxyPath(),
                "ModelRegisteEventArgs.cs",
                VarNamespace, m_project.ClientNameSpace
                );

            CopyTextFile("ClientFile.ChildrenModel.tmp",
                GetClientProxyPath(),
                "ChildrenModel.cs",
                VarNamespace, m_project.ClientNameSpace
                );

            CopyTextFile("ClientFile.ParentModel.tmp",
               GetClientProxyPath(),
               "ParentModel.cs",
               VarNamespace, m_project.ClientNameSpace
               );

            CopyTextFile("ClientFile.ModelCacheManager.tmp",
               GetClientPath(),
               "ModelCacheManager.cs",
               VarNamespace, m_project.ClientNameSpace
               );

            CopyTextFile("ClientFile.ModelCollection.tmp",
               GetClientPath(),
               "ModelCollection.cs",
               VarNamespace, m_project.ClientNameSpace
               );

            CopyTextFile("ClientFile.ClientProxy.tmp",
               GetClientPath(),
               string.Format("{0}ClientProxy.cs", m_project.ProjectName),
               VarProjectName, m_project.ProjectName
               );

            string path = GetClientProxyPath();

            StringBuilder sb = new StringBuilder();
            foreach (TableInfo info in m_project.Tables)
            {
                if (File.Exists(string.Format("{0}{1}.cs", path, info.ClassName)))
                {
                    sb.AppendLine(string.Format("    <Compile Include=\"Proxy\\{0}.cs\" />", info.ClassName));
                }
            }

            CopyTextFile("ClientFile.Project.tmp",
               GetClientPath(),
               string.Format("{0}.csproj", m_project.ClientNameSpace),
               VarNamespace, m_project.ClientNameSpace,
               VarGuid, projectGuid.ToString(),
               VarIncludeFiles, sb.ToString(),
               VarProjectName, m_project.ProjectName
               );
        }
        #endregion

        #region build service project
        public static string GetServicePath()
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += string.Format("\\Server\\{0}\\", SystemUtility.m_project.ServerAppName);
            return path;
        }

        public static void BuildServicePorject(Guid projectGuid, Guid modelProjectGuid)
        {
            CopyFile("ServiceFile.SuperControl.ServiceModel.dll", GetServicePath(), "SuperControl.ServiceModel.dll");

            CopyFile("ServiceFile.MySql.Data.dll", GetServicePath(), "MySql.Data.dll");

            CopyTextFile("ServiceFile.AssemblyInfo.tmp",
                GetServicePath() + "Properties\\",
                "AssemblyInfo.cs",
                VarNamespace, m_project.ServerAppName,
                VarDate, DateTime.Now.ToString(),
                VarGuid, Guid.NewGuid().ToString()
                );

            CopyTextFile("ServiceFile.IService.tmp",
                GetServicePath() + "Generic\\",
                string.Format("I{0}.gc.cs", m_project.ServiceName),
                VarProjectName, m_project.ProjectName,
                VarRootClassName, m_project.RootClassName
                );

            CopyTextFile("ServiceFile.Service.tmp",
                GetServicePath() + "Generic\\",
                string.Format("{0}.gc.cs", m_project.ServiceName),
                VarProjectName, m_project.ProjectName,
                VarRootClassName, m_project.RootClassName
                );

            CopyTextFile("ServiceFile.ICustomService.tmp",
                 GetServicePath(),
                 string.Format("I{0}.cs", m_project.ServiceName),
                 VarProjectName, m_project.ProjectName
                 );

            CopyTextFile("ServiceFile.CustomService.tmp",
                 GetServicePath(),
                 string.Format("{0}.cs", m_project.ServiceName),
                 VarProjectName, m_project.ProjectName
                 );

            CopyTextFile("ServiceFile.Program.tmp",
                 GetServicePath(),
                 "Program.cs",
                 VarProjectName, m_project.ProjectName
                 );

            CopyTextFile("ServiceFile.Project.tmp",
                 GetServicePath(),
                 string.Format("{0}.csproj", m_project.ServerAppName),
                 VarProjectName, m_project.ProjectName,
                 VarGuid, projectGuid.ToString(),
                 VarModelGuid, modelProjectGuid.ToString()
                 );

            BuildAppConfig();
        }

        private static void BuildAppConfig()
        {
            string path = GetServerPath();
            StringBuilder sb = new StringBuilder();
            foreach (TableInfo info in m_project.Tables)
            {
                if (File.Exists(string.Format("{0}{1}.cs", path, info.ClassName)))
                {
                    sb.AppendLine(string.Format("       <add key=\"SuperControl.{0}Model.{1}\" value=\"default\" />", m_project.ProjectName, info.ClassName));
                }
            }

            string provider;
            switch (m_project.DatabaseType)
            {
                case DBType_SqlServer:
                    provider = "System.Data.SqlClient";
                    break;
                case DBType_SCODBC:
                    provider = "System.Data.Odbc,Factory=SCModelFactory";
                    break;
                case DBType_MySql:
                    provider = "MySql.Data.MySqlClient";
                    break;
                default:
                    provider = "System.Data.Odbc";
                    break;
            }

            CopyTextFile("ServiceFile.App.config.tmp",
                GetServicePath(),
                "App.config",
                VarProjectName, m_project.ProjectName,
                VarIncludeFiles, sb.ToString(),
                VarConnectionString, m_project.ConnectionString,
                VarProviderName, provider
                );
        }
        #endregion

        private static void CopyFile(string src, string path, string dest)
        {
            Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("ModelCodeGenerator." + src);
            byte[] bs = new byte[sm.Length];
            sm.Read(bs, 0, (int)sm.Length);
            sm.Close();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string file = path + dest;
            FileStream stream = new FileStream(file, FileMode.Create);
            stream.Write(bs, 0, bs.Length);
            stream.Flush();
            stream.Close();
        }

        private static void CopyTextFile(string src, string path, string dest, params string[] replace)
        {
            Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("ModelCodeGenerator." + src);
            byte[] bs = new byte[sm.Length];
            sm.Read(bs, 0, (int)sm.Length);
            sm.Close();
            UTF8Encoding con = new UTF8Encoding();
            string str = con.GetString(bs);
            for (int i = 0; i < replace.Length; i+=2)
            {
                str = str.Replace(replace[i], replace[i + 1]);
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string file = path + dest;
            FileStream stream = new FileStream(file, FileMode.Create);
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(str);
            sw.Flush();
            stream.Close();
        }

        public static void BuildServerSln(Guid appProjectGuid, Guid modelProjectGuid)
        {
            string path = SystemUtility.m_project.OutputPath.TrimEnd('/', '\\');
            path += "\\Server\\";

            CopyTextFile("ServerFile.sln.tmp",
                 path,
                 string.Format("SuperControl.{0}Server.sln", m_project.ProjectName),
                 VarProjectName, m_project.ProjectName,
                 VarGuid, appProjectGuid.ToString(),
                 VarModelGuid, modelProjectGuid.ToString()
                 );
        }

        public static DbConnection GetDbConnection(string dbType)
        {
            DbConnection conn;
            switch (dbType)
            {
                case SystemUtility.DBType_SqlServer:
                    conn = new SqlConnection();
                    break;
                case SystemUtility.DBType_SCODBC:
                    conn = new OdbcConnection();
                    break;
                case SystemUtility.DBType_MySql:
                    conn = new MySqlConnection();
                    break;
                default:
                    conn = new OdbcConnection();
                    break;
            }
            return conn;
        }

        public static TableInfo GetTableInfo(string tableName)
        {
            foreach (TableInfo tinfo in m_project.Tables)
            {
                if (tinfo.TableName == tableName)
                    return tinfo;
            }
            return null;
        }
    }
}

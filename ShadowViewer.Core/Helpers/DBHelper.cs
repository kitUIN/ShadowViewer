namespace ShadowViewer.Helpers
{
    public static class DBHelper
    {
        public static string DBPath { get; } = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "ShadowViewer.db");
        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="commandText">The command text.</param>
        public static void Init(string dbpath,string commandText)
        {
             
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand command = db.CreateCommand();
                command.CommandText = commandText;
                command.ExecuteReader();
            }
        }
        /// <summary>
        /// 在数据库添加一个新行
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Add(string dbpath, string table, Dictionary<string, object> parameters)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"insert into {table} values ({string.Join(",", parameters.Keys)});";
                foreach (var item in parameters)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
                command.ExecuteReader();
            }
        }
        /// <summary>
        /// 获取数据库中所有的行
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="convert">The convert.</param>
        /// <returns></returns>
        public static List<object> GetAll(string dbpath, string table, Func<SqliteDataReader, object> convert)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from {table}";
                var res = new List<object>();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(convert(reader));
                    }
                }
                return res;
            }
        }
        /// <summary>
        /// 从数据库中获取一个行
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="where">The where.</param>
        /// <param name="convert">The convert.</param>
        /// <returns></returns>
        public static List<object> Get(string dbpath, string table, KeyValuePair<string, object> where, Func<SqliteDataReader, object> convert)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from {table} where {where.Key} = @WhereArg;";
                command.Parameters.AddWithValue("@WhereArg", where.Value);
                var res = new List<object>();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(convert(reader));
                    }
                }
                return res;
            }
        }
        /// <summary>
        /// 从数据库中获取一个行(多个条件And)
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="where">The where.</param>
        /// <param name="convert">The convert.</param>
        /// <returns></returns>
        public static List<object> Get(string dbpath, string table, Dictionary<string, object> where, Func<SqliteDataReader, object> convert)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                var text = $"select * from {table} where ";
                bool flag = false;
                SqliteCommand command = db.CreateCommand();
                foreach (var item in where)
                {
                    if (flag)
                    {
                        text += " And ";
                    }
                    text += item.Key + " = @"+ item.Key;
                    flag = true;
                }
                command.CommandText = text + ";";
                foreach (var item in where)
                {
                    command.Parameters.AddWithValue("@" + item.Key, item.Value);
                }
                
                var res = new List<object>();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(convert(reader));
                    }
                }
                return res;
            }
        }
        /// <summary>
        /// 数据库中更新行
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="name">The name.</param>
        /// <param name="newArg">The new argument.</param>
        /// <param name="where">The where.</param>
        /// <param name="whereArg">The where argument.</param>
        public static void Update(string dbpath, string table, string name, object newArg, string where, object whereArg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"update {table} set [{name}] = @NewArg where [{where}] = @WhereArg;";
                command.Parameters.AddWithValue("@NewArg", newArg);
                command.Parameters.AddWithValue("@WhereArg", whereArg);
                command.ExecuteReader();
            }
        }


        /// <summary>
        /// 数据库中删除行
        /// </summary>
        /// <param name="dbpath">The dbpath.</param>
        /// <param name="table">The table.</param>
        /// <param name="where">The where.</param>
        /// <param name="whereArg">The value.</param>
        public static void Remove(string dbpath, string table , string where, object whereArg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"delete from {table} where {where} = @ID;";
                command.Parameters.AddWithValue("@ID", whereArg);
                command.ExecuteReader();
            }
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="where">The where.</param>
        /// <param name="whereArg">The where argument.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified table]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string dbpath, string table ,string where, string whereArg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select count(*) from {table} where {where} = @WhereArg;";
                command.Parameters.AddWithValue("@WhereArg", whereArg); 
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetInt32(0) > 0;
                    }
                }
                return false;
            }
        }
        
    }
    
}

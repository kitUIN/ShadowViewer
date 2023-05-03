

using ShadowViewer.Models;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace ShadowViewer.Helpers
{
    public static class DBHelper
    {
        public static string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "ShadowViewer.db");
        public  static void InitializeDatabase()
        {
            ShadowTableInit();
            ShadowTagsTableInit();
        }
        private static void ShadowTableInit()
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {

                db.Open();
                SqliteCommand command = db.CreateCommand();
                command.CommandText = "create table if not exists ShadowTable " +
                        "(Name nvarchar(2048) primary key," +
                        "Author nvarchar(2048) null," +
                        "Parent nvarchar(2048) null," +
                        "Percent nchar(128) null," +
                        "CreateTime text null," +
                        "LastReadTime text null," +
                        "Link nvarchar(2048) null," +
                        "Tags nvarchar(2048) null," +
                        "AnotherTags nvarchar(2048) null," +
                        "Img nvarchar(2048) null, " +
                        "Size bigint null, " +
                        "IsFolder boolean false);";
                command.ExecuteReader();
                Log.ForContext<SqliteConnection>().Information("ShadowTable数据库初始化");
            }
        }
        private static void ShadowTagsTableInit()
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand command = db.CreateCommand();
                command.CommandText = "create table if not exists ShadowTagsTable " +
                        "("+
                        "Tag nvarchar(2048) primary key," +
                        "Name nvarchar(2048) null," +
                        "Background nchar(128) null," +
                        "Foreground nchar(128) null" +
                        ");";
                command.ExecuteReader();
                Log.ForContext<SqliteConnection>().Information("ShadowTagsTable数据库初始化");
            }
        }
        public static void AddShadowTag(ShadowTag shadowTag)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = "insert into ShadowTagsTable values (@Tag, @Name, @Foreground, @Background);";
                command.Parameters.AddWithValue("@Tag", shadowTag.tag);
                command.Parameters.AddWithValue("@Name", shadowTag.name);
                command.Parameters.AddWithValue("@Background", shadowTag.BackgroundHex);
                command.Parameters.AddWithValue("@Foreground", shadowTag.ForegroundHex);
                try
                {
                    command.ExecuteReader();
                    Log.ForContext<SqliteConnection>().Information("添加标签数据{shadowTag}", shadowTag.ToString());
                }
                catch (Exception ex)
                {
                    Log.ForContext<SqliteConnection>().Error("添加标签数据失败:\n {Ex}", ex);
                }
            }
        }
        
        public static List<ShadowTag> GetAllShadowTags()
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from ShadowTagsTable";
                var shadowTags  = new List<ShadowTag>();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        shadowTags.Add(ShadowTag.LoadFromDB(reader));
                    }
                }
                Log.ForContext<SqliteConnection>().Information("获取标签(counts={Count})", shadowTags.Count);
                return shadowTags;
            }
        }
        public static ShadowTag GetShadowTag(string tag)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from ShadowTagsTable where Tag = @Tag;";
                command.Parameters.AddWithValue("@Tag", tag);
                 
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    ShadowTag shadowTag = new ShadowTag(reader.GetString(0), reader.GetString(1),
                        reader.GetString(2), reader.GetString(3));
                    Log.ForContext<SqliteConnection>().Information("获取数据标签:", shadowTag.ToString());
                    return shadowTag;
                } 
            }
        }
        public static List<ShadowTag> GetShadowTagFrom(string name, string arg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from ShadowTagsTable where {name} = @Tag;";
                command.Parameters.AddWithValue("@Tag", arg);
                List < ShadowTag > res = new List<ShadowTag >();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        ShadowTag shadowTag = ShadowTag.LoadFromDB(reader);
                        res.Add(shadowTag);
                    }
                }
                Log.ForContext<SqliteConnection>().Information("从{name}={arg}获取数据标签(counts={c})", name, arg, res.Count);
                return res;
            }
        }
        private static void Update(string table,string name,  string newArg, string where, string whereArg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"update {table} set {name} = @NewArg where {where} = @WhereArg;";
                command.Parameters.AddWithValue("@NewArg", newArg);
                command.Parameters.AddWithValue("@WhereArg", whereArg);
                command.ExecuteReader();
            }
        }
        public static void UpdateShadowTag(string name, string where, string newArg, string whereArg)
        {
            try
            {
                Update("ShadowTagsTable", name,  newArg,where, whereArg);
                Log.ForContext<SqliteConnection>().Information("更新数据标签:{name}({old}->{new})", name, whereArg, newArg);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("更新数据标签:{name}({old}->{new})失败:\n{Ex}", name, whereArg, newArg, ex);
            }
        }
        public static void UpdateLocalComic(string name, string where, string newArg, string whereArg)
        {
            try
            {
                Update("ShadowTable", name, newArg, where,  whereArg);
                Log.ForContext<SqliteConnection>().Information("漫画:{name}({old}->{new})", name, whereArg, newArg);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("漫画:{name}({old}->{new})失败:\n{Ex}", name, whereArg, newArg, ex);
            }
        }
        private static void Remove(string table , string where, string id)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"delete from {table} where {where} = @ID;";
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteReader();
            }
        }
        public static void RemoveShadowTag(string where, string id)
        {
            try
            {
                Remove("ShadowTagsTable", where, id);
                Log.ForContext<SqliteConnection>().Information("删除数据标签:{where}={id}", where, id);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("删除数据标签:{where}={id} 失败:\n{Ex}", where, id, ex);
            }
        }
        public static void RemoveLocalComic(string where, string id)
        {
            try
            {
                Remove("ShadowTable", where, id);
                Log.ForContext<SqliteConnection>().Information("删除漫画:{where}={id}", where, id);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("删除漫画:{where}={id} 失败:\n{Ex}", where, id, ex);
            }
        }
        public static void AddComic(LocalComic localComic)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = "insert into ShadowTable values (@Name, @Author, @Parent, @Percent, @CreateTime, @LastReadTime, @Link, @Tags, @AnotherTags, @Img, @Size, @IsFolder);";
                command.Parameters.AddWithValue("@Name", localComic.Name);
                command.Parameters.AddWithValue("@Author", localComic.Author);
                command.Parameters.AddWithValue("@Parent", localComic.Parent);
                command.Parameters.AddWithValue("@Percent", localComic.Percent);
                command.Parameters.AddWithValue("@CreateTime", localComic.CreateTime);
                command.Parameters.AddWithValue("@LastReadTime", localComic.LastReadTime);
                command.Parameters.AddWithValue("@Link", localComic.Link);
                command.Parameters.AddWithValue("@Tags", localComic.Tags.JoinToString());
                command.Parameters.AddWithValue("@AnotherTags", localComic.AnotherTags.JoinToString());
                command.Parameters.AddWithValue("@Img", localComic.Img);
                command.Parameters.AddWithValue("@Size", localComic.Size);
                command.Parameters.AddWithValue("@IsFolder", localComic.IsFolder);
                try
                {
                    command.ExecuteReader();
                    Log.ForContext<SqliteConnection>().Information("添加本地漫画(id={ID})", localComic.Name);
                }
                catch (Exception ex)
                {
                    Log.ForContext<SqliteConnection>().Error("添加本地漫画失败:\n {Ex}", ex);
                }
            }
            
        }
        public static void AddComic(string name,string img,string parent)
        {
            if (img == "") { img = "ms-appx:///Assets/Default/folder.png"; }
            if (name == "") { name = Guid.NewGuid().ToString("N"); }
            var comic = LocalComic.CreateFolder(name ,"", img, parent);
            AddComic(comic);
        } 
        public static Collection<LocalComic> GetLocalComicFrom(string name, string arg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"select * from ShadowTable where {name} = @Parent;";
                command.Parameters.AddWithValue("@Parent", arg);
                Collection<LocalComic> res = new Collection<LocalComic>();
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(LocalComic.ReadComicFromDB(reader));
                    }
                }
                Log.ForContext<SqliteConnection>().Information("从{name}={Parent} 获取漫画(counts={Count})", name , arg,res.Count );
                return res;
            }
        }

        public static bool Contains(string table ,string where, string whereArg)
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
        public static bool ContainsTag(string where, string whereArg)
        {
            return Contains("ShadowTagsTable", where, whereArg);
        }
    }
    
}

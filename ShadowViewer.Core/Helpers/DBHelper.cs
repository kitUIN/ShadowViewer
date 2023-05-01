

using ShadowViewer.Models;

namespace ShadowViewer.Helpers
{
    public class DBHelper
    {
        public static string dbpath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "ShadowViewer.db");
        public  static void InitializeDatabase()
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
                Log.ForContext<DBHelper>().Information("数据库初始化");
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
                    Log.ForContext<DBHelper>().Information("添加本地漫画(id={ID})", localComic.Name);
                }
                catch (Exception ex)
                {
                    Log.ForContext<DBHelper>().Error("添加本地漫画失败:\n {Ex}", ex);
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

        public static Collection<LocalComic> GetFrom(string name, string arg)
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
                Log.ForContext<DBHelper>().Information("从{name}={Parent} 获取漫画(counts={Count})", name , arg,res.Count );
                return res;
            }
        }
        public static void Update(string name,string where, string newArg, string oldArg)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = $"update ShadowTable SET {name} = @NewArg where {where} = @OldArg;";
                command.Parameters.AddWithValue("@NewArg", newArg);
                command.Parameters.AddWithValue("@OldArg", oldArg);
                try
                {
                    command.ExecuteReader();
                    Log.ForContext<DBHelper>().Information("漫画:{name}({old}->{new})", name, oldArg, newArg);
                }
                catch (Exception ex)
                {
                    Log.ForContext<DBHelper>().Error("漫画:{name}({old}->{new})失败:\n{Ex}", name, oldArg, newArg, ex);
                } 
            }
        }
    }
}

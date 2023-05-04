namespace ShadowViewer.DataBases
{
    public static class ComicDB
    {
        public static string DBTable { get; } = "ShadowTable";
        public static void Init()
        {
            DBHelper.Init(DBHelper.DBPath, "create table if not exists ShadowTable " +
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
                        "IsFolder boolean false);");
            Log.ForContext<SqliteConnection>().Information(messageTemplate: "Comic数据库初始化");
        }
        public static void Update(string name, string where, string newArg, string whereArg)
        {
            try
            {
                DBHelper.Update(DBHelper.DBPath, DBTable, name, newArg, where, whereArg);
                Log.ForContext<SqliteConnection>().Information("漫画:{name}({old}->{new})", name, whereArg, newArg);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("漫画:{name}({old}->{new})失败:\n{Ex}", name, whereArg, newArg, ex);
            }
        }
        public static void Add(LocalComic localComic)
        {
            try
            {
                DBHelper.Add(DBHelper.DBPath, DBTable, new Dictionary<string, object>
                {
                    { "@Name", localComic.Name                                             },
                    { "@Author", localComic.Author                                      },
                    { "@Parent", localComic.Parent                                      },
                    { "@Percent", localComic.Percent                                    },
                    { "@CreateTime", localComic.CreateTime                                },
                    { "@LastReadTime", localComic.LastReadTime                            },
                    { "@Link", localComic.Link                                            },
                    { "@Tags", localComic.Tags.JoinToString()                             },
                    { "@AnotherTags", localComic.AnotherTags.JoinToString()               },
                    { "@Img", localComic.Img                                              },
                    { "@Size", localComic.Size                                            },
                    { "@IsFolder", localComic.IsFolder },

                });
                Log.ForContext<SqliteConnection>().Information("添加本地漫画(id={ID})", localComic.Name);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("添加本地漫画失败:\n {Ex}", ex);
            }
        }
        public static void Add(string name, string img, string parent)
        {
            if (img == "") { img = "ms-appx:///Assets/Default/folder.png"; }
            if (name == "") { name = Guid.NewGuid().ToString("N"); }
            var comic = LocalComic.CreateFolder(name, "", img, parent);
            Add(comic);
        }

        public static void Remove(string where, string id)
        {
            try
            {
                DBHelper.Remove(DBHelper.DBPath,DBTable, where, id);
                Log.ForContext<SqliteConnection>().Information("删除漫画:{where}={id}", where, id);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("删除漫画:{where}={id} 失败:\n{Ex}", where, id, ex);
            }
        }
        public static List<LocalComic> Get(string where, string whereArg)
        {
            List<LocalComic> res = DBHelper.Get(DBHelper.DBPath, DBTable, KeyValuePair.Create(where, whereArg as object), LocalComic.ReadComicFromDB).Cast<LocalComic>().ToList();
            Log.ForContext<SqliteConnection>().Information("[{name}={Parent}]获取漫画(counts={Count})", where, whereArg, res.Count);
            return res;
        }
        public static LocalComic GetFirst(string where, string whereArg)
        {
            List<LocalComic> res = DBHelper.Get(DBHelper.DBPath, DBTable, KeyValuePair.Create(where, whereArg as object), LocalComic.ReadComicFromDB).Cast<LocalComic>().ToList();
            Log.ForContext<SqliteConnection>().Information("[{name}={Parent}]获取漫画(counts={Count})", where, whereArg, res.Count);
            if(res.Count > 0 )
            {
                return res[0];
            }
            return null;
        }
        public static bool Contains(string where, string whereArg)
        {
            return DBHelper.Contains(DBHelper.DBPath, DBTable, where, whereArg);
        }
    }
}

namespace ShadowViewer.DataBases
{
    public static class ComicDB
    {
        public static string DBTable { get; } = "ShadowComics";
        public static void Init()
        {
            DBHelper.Init(DBHelper.DBPath, "create table if not exists ShadowComics " +
                        "([Id] nchar(32) primary key," +
                        "[Name] nvarchar(2048) null," +
                        "[Author] nvarchar(2048) null," +
                        "[Group] nvarchar(2048) null," +
                        "[Description] ntext null," +
                        "[Parent] nvarchar(2048) null," +
                        "[Percent] nvarchar(12) null," +
                        "[CreateTime] text null," +
                        "[LastReadTime] text null," +
                        "[Link] nvarchar(2048) null," +
                        "[Tags] ntext null," +
                        "[Affiliation] nvarchar(2048) null," +
                        "[Img] nvarchar(2048) null, " +
                        "[Size] bigint null, " +
                        "[IsFolder] boolean false);");
            Log.ForContext<SqliteConnection>().Information(messageTemplate: "Comic数据库初始化");
        }
        public static void Update(string name, string where, string newArg, string whereArg)
        {
            try
            {
                DBHelper.Update(DBHelper.DBPath, DBTable, name, newArg, where, whereArg);
                Log.ForContext<SqliteConnection>().Information("修改漫画:{name}({old}->{new})", name, whereArg, newArg);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("修改漫画:{name}({old}->{new})失败:\n{Ex}", name, whereArg, newArg, ex);
                throw;
            }
        }
        public static void Add(LocalComic localComic)
        {
            try
            {
                DBHelper.Add(DBHelper.DBPath, DBTable, new Dictionary<string, object>
                {
                    { "@ID", localComic.Id                                             },
                    { "@Name", localComic.Name                                             },
                    { "@Author", localComic.Author                                      },
                    { "@Group", localComic.Group                                      },
                    { "@Description", localComic.Description                                      },
                    { "@Parent", localComic.Parent                                      },
                    { "@Percent", localComic.Percent                                    },
                    { "@CreateTime", localComic.CreateTime                                },
                    { "@LastReadTime", localComic.LastReadTime                            },
                    { "@Link", localComic.Link                                            },
                    { "@Tags", localComic.Tags.JoinToString()                             }, 
                    { "@Affiliation", localComic.Affiliation                         }, 
                    { "@Img", localComic.Img                                              },
                    { "@Size", localComic.Size                                            },
                    { "@IsFolder", localComic.IsFolder },

                });
                Log.ForContext<SqliteConnection>().Information("添加本地漫画(id={ID})", localComic.Id);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("添加本地漫画失败:\n {Ex}", ex);
                throw;
            }
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
                throw;
            }
        }
        public static List<LocalComic> Get(string where, string whereArg)
        {
            List<LocalComic> res = DBHelper.Get(DBHelper.DBPath, DBTable, KeyValuePair.Create(where, whereArg as object), ReadComicFromDB).Cast<LocalComic>().ToList();
            Log.ForContext<SqliteConnection>().Information("[{name}={Parent}]获取漫画(counts={Count})", where, whereArg, res.Count);
            return res;
        }
        public static List<LocalComic> Get(Dictionary<string,object> where)
        {
            List<LocalComic> res = DBHelper.Get(DBHelper.DBPath, DBTable, where, ReadComicFromDB).Cast<LocalComic>().ToList();
            Log.ForContext<SqliteConnection>().Information("[Dictionary]获取漫画(counts={Count})", res.Count);
            return res;
        }
        public static LocalComic GetFirst(string where, string whereArg)
        { 
            List<LocalComic> res = DBHelper.Get(DBHelper.DBPath, DBTable, KeyValuePair.Create(where, whereArg as object), ReadComicFromDB).Cast<LocalComic>().ToList();
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
        
        public static LocalComic ReadComicFromDB(SqliteDataReader reader)
        {
            return new LocalComic(reader.GetString(0), reader.GetString(1),
                reader.GetString(7), reader.GetString(8), author: reader.GetString(2),
                group: reader.GetString(3), description: reader.GetString(4), parent: reader.GetString(5),
                percent: reader.GetString(6), link: reader.GetString(9),
                tags:reader.GetString(10),affiliation: reader.GetString(11), img: reader.GetString(12),
                size:reader.GetInt64(13),isFolder: reader.GetBoolean(14));
        }
        public static void RemoveInDB(this LocalComic comic)
        {
            Remove(nameof(comic.Name), comic.Name);
        }
       
    }
}

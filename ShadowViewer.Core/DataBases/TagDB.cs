﻿using ShadowViewer.Helpers;
using ShadowViewer.Models;
using System.Reflection.PortableExecutable;

namespace ShadowViewer.DataBases
{
    public static class TagDB
    {
        public static string DBTable { get; } = "ShadowTagsTable";
        public static void Init()
        {
            DBHelper.Init(DBHelper.DBPath, "create table if not exists ShadowTagsTable " +
                        "(" +
                        "Tag nvarchar(2048) primary key," +
                        "Name nvarchar(2048) null," +
                        "Background nchar(128) null," +
                        "Foreground nchar(128) null" +
                        ");");
            Log.ForContext<SqliteConnection>().Information(messageTemplate: "Tag数据库初始化");
        }
        public static void Add(ShadowTag shadowTag)
        {
             
            try
            {
                DBHelper.Add(DBHelper.DBPath, DBTable, new Dictionary<string, object>
            {
                { "@Tag", shadowTag.tag },
                { "@Name", shadowTag.name },
                { "@Foreground", shadowTag.ForegroundHex },
                { "@Background", shadowTag.BackgroundHex },
            });
                Log.ForContext<SqliteConnection>().Information("添加标签数据{shadowTag}", shadowTag.ToString());
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("添加标签数据失败:\n {Ex}", ex);
            }
        }
        public static List<ShadowTag> GetAll()
        {
            List<ShadowTag>  shadowTags =  DBHelper.GetAll(DBHelper.DBPath, DBTable, ShadowTag.LoadFromDB).Cast<ShadowTag>().ToList(); 
            Log.ForContext<SqliteConnection>().Information("获取标签(counts={Count})", shadowTags.Count);
            return shadowTags;
        }
        public static List<ShadowTag> Get(string where, string value)
        {
            List<ShadowTag> shadowTags = DBHelper.Get(DBHelper.DBPath, DBTable, KeyValuePair.Create(where, value as object), ShadowTag.LoadFromDB).Cast<ShadowTag>().ToList();
            Log.ForContext<SqliteConnection>().Information("[{where}={tag}]获取标签(counts={Count})", where, value, shadowTags.Count);
            return shadowTags;
        }
        public static List<ShadowTag> Get(string tag)
        {
            return Get("Tag", tag);
        }
        public static void Update(string name, string where, string newArg, string whereArg)
        {
            try
            {
                DBHelper.Update(DBHelper.DBPath, DBTable, name, newArg, where, whereArg);
                Log.ForContext<SqliteConnection>().Information("更新数据标签:{name}({old}->{new})", name, whereArg, newArg);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("更新数据标签:{name}({old}->{new})失败:\n{Ex}", name, whereArg, newArg, ex);
            }
        }
        public static void Remove(string where, string id)
        {
            try
            {
                DBHelper.Remove(DBHelper.DBPath, DBTable, where, id);
                Log.ForContext<SqliteConnection>().Information("删除数据标签:{where}={id}", where, id);
            }
            catch (Exception ex)
            {
                Log.ForContext<SqliteConnection>().Error("删除数据标签:{where}={id} 失败:\n{Ex}", where, id, ex);
            }
        }
        public static bool Contains(string where, string whereArg)
        {
            return DBHelper.Contains(DBHelper.DBPath, DBTable, where, whereArg);
        }
    }
    
}
    

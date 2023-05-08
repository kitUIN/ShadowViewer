﻿

namespace ShadowViewer.Models
{
    public class ShadowTag
    {
        public string tag;
        public string name;
        public SolidColorBrush foreground;
        public SolidColorBrush background;
        public ShadowTag(string tag,string name, SolidColorBrush foreground , SolidColorBrush background)
        {
            this.tag = tag;
            this.name = name;
            this.foreground = foreground;
            this.background = background;
        }
        public ShadowTag(string tag, string name, Color foreground, Color background) : 
            this(tag, name, new SolidColorBrush(foreground),
                new SolidColorBrush(background)) { }
        public ShadowTag(string tag, string name, string foreground, string background) :
            this(tag, name,new SolidColorBrush(foreground.ToColor()),
                new SolidColorBrush(background.ToColor()))
        { }
        public string BackgroundHex
        {
            get => background.Color.ToHex(); 
        }
        public string ForegroundHex
        {
             get => foreground.Color.ToHex(); 
        }
        public override string ToString()
        {
            return name;
        }
        public string Log()
        {
            return $"ShadowTag(id={tag},name={name},foreground={ForegroundHex},background={BackgroundHex})";
        }
        public static ShadowTag LoadFromDB(SqliteDataReader reader)
        {
            return new ShadowTag(reader.GetString(0), reader.GetString(1),
                        reader.GetString(2), reader.GetString(3));
        }
    }
}

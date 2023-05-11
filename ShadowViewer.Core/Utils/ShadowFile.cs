using Microsoft.UI.Xaml.Controls;

namespace ShadowViewer.Utils
{
    
    public class ShadowFile
    {
        public IStorageItem Self { get; set; }
        public int Depth { get; set; } = 0 ;
        public int Counts { get; set; } = 0;
        public List<ShadowFile> Children { get; } = new List<ShadowFile>();
        public ShadowFile(IStorageItem item) 
        {
            Self=item;
        }
        private async Task<Tuple<int, int>> LoadChildren(Action<IStorageItem> sizeAdd)
        {
            if(Self is StorageFolder folder)
            {
                foreach (var item in await folder.GetItemsAsync())
                {
                    var file = new ShadowFile(item);
                    sizeAdd.Invoke(item);
                    var t = await file.LoadChildren(sizeAdd);
                    file.Depth = t.Item1;
                    file.Counts = t.Item2;
                    Children.Add(file);
                }
                if(Children.Count == 0) return Tuple.Create(0, 1);
                return Tuple.Create(Children.Max(x => x.Depth) + 1, Children.Sum(x=>x.Counts) + 1);
            }
            return Tuple.Create(0, 1);
        }
        public static async Task<ShadowFile> Create(IStorageItem item, Action<IStorageItem> sizeAdd)
        {
            var f = new ShadowFile(item);
            var t = await f.LoadChildren(sizeAdd);
            f.Depth = t.Item1;
            f.Counts = t.Item2;
            return f;
        }
    }
}

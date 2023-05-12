using Microsoft.UI.Xaml.Controls;

namespace ShadowViewer.Utils
{
    
    public class ShadowFile
    {
        public IStorageItem Self { get; set; }
        public int Depth { get; set; } = 0 ;
        public int Counts { get; set; } = 0;
        public long Size { get; set; } = 0;
        public List<ShadowFile> Children { get; } = new List<ShadowFile>();
        public ShadowFile(IStorageItem item) 
        {
            Self=item;
        }
        private async Task LoadChildren()
        {
            if(Self is StorageFolder folder)
            {
                foreach (var item in await folder.GetItemsAsync())
                {
                    var file = new ShadowFile(item);
                    await file.LoadChildren();
                    Children.Add(file);
                }
                if(Children.Count == 0)
                {
                    Size = 0;
                    Depth = 0;
                    Counts = 1;
                }
                else
                {
                    Size = Children.Sum(x => x.Size);
                    Depth = Children.Max(x => x.Depth) + 1;
                    Counts = Children.Sum(x => x.Counts) + 1;
                }
                
            }
            else if(Self is StorageFile file)
            {
                Size = (long)(await file.GetBasicPropertiesAsync()).Size;
                Depth = 0;
                Counts = 1;
            }
            
        }
        public static async Task<ShadowFile> Create(IStorageItem item)
        {
            var f = new ShadowFile(item);
            await f.LoadChildren();
            return f;
        }
    }
}

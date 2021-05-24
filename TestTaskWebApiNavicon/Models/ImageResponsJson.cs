using System.Collections.Generic;

namespace TestTaskWebApiNavicon.Models
{
    public class ImageResponsJson
    {
        public string Host { set; get; }
        public List<ImageInfo> images { set; get; }
        public ImageResponsJson()
        {
            images = new List<ImageInfo>();
        }
    }
    public class ImageInfo
    {
        public string Alt { set; get; }
        public string Src { set; get; }
        public long Size { set; get; }
    }

}

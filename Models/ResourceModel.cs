using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WutheringDownloader.Models
{
    public class Resource
    {
        public string dest {  get; set; }
        public string md5 { get; set; }
        public string sampleHash { get; set; }
        public long size { get; set; }
    }
    public class ResourceRoot
    {
        public List<Resource> resource { get; set; }
    }
}

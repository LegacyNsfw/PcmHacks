using System;

namespace PcmHacking
{
    public class PathDisplayAdapter
    {
        private string path;

        public string Path
        {
            get { return this.path; }
        }

        public PathDisplayAdapter(string path)
        {
            this.path = path;
        }

        public override string ToString()
        {
            // Have to use the fully qualified name here 
            // since it conflicts with the Path property.
            return System.IO.Path.GetFileNameWithoutExtension(this.path);
        }
    }
}

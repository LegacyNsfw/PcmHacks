using System;

namespace PcmHacking
{
    /// <summary>
    /// This is used to reduce the file path string down to just the file name,
    /// so that file names won't be pushed out of view if someone has a small
    /// monitor and a deep directory structure.
    /// </summary>
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

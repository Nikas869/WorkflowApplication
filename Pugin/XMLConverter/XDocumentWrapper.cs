using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLConverter
{
    public class XDocumentWrapper : IDisposable
    {
        public string FilePath { get; private set; }

        FileStream fileStream = null;
        XDocument xDocument = null;

        public XDocumentWrapper(string XmlDataFile)
        {
            this.FilePath = XmlDataFile;
        }

        public void OpenDocument()
        {
            if (!File.Exists(FilePath))
            {
                throw new Exception($"Xml File [{FilePath}] does not exist");
            }

            fileStream = File.OpenRead(FilePath);
            xDocument = XDocument.Load(fileStream, LoadOptions.SetLineInfo);
        }

        public IEnumerable<XElement> GetNodes(string ParentName, string ChildNodeName)
        {
            foreach (var pnode in xDocument.Elements(ParentName))
            {
                foreach (var node in pnode.Elements(ChildNodeName))
                {
                    yield return node;
                }
            }
        }

        public float GetProgress()
        {
            if (fileStream == null)
                return 0;

            double pos = fileStream.Position;

            return float.Parse((100 * (pos / fileStream.Length)).ToString());


        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (fileStream != null)
                    {
                        try { fileStream.Close(); } catch { }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

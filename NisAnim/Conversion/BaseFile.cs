using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NisAnim.Conversion
{
    public abstract class BaseFile : IDisposable
    {
        [Browsable(false)]
        public string FilePath { get; private set; }

        bool disposed;

        protected BaseFile()
        {
            disposed = false;
        }

        protected BaseFile(string filePath)
            : this()
        {
            FilePath = filePath;
        }

        ~BaseFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    /* Nothing to dispose here, really */
                }

                this.disposed = true;
            }
        }
    }
}

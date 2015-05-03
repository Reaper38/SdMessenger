using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal partial class FileTransferView : UserControl
    {
        private readonly List<FileTransferViewItem> items;

        public FileTransferView()
        {
            InitializeComponent();
            items = new List<FileTransferViewItem>();
            Items = new ItemCollection(this);
        }
        
        public ItemCollection Items { get; private set; }

        public sealed class ItemCollection : IList<FileTransferViewItem>
        {
            private readonly FileTransferView owner;

            internal ItemCollection(FileTransferView owner)
            { this.owner = owner; }

            #region IList<FileTransferViewItem> Members

            public int IndexOf(FileTransferViewItem item)
            { return owner.items.IndexOf(item); }

            public void Insert(int index, FileTransferViewItem item)
            {
                owner.items.Insert(index, item);
                owner.tlp.SuspendLayout();
                owner.tlp.Controls.Add(item);
                owner.tlp.Controls.SetChildIndex(item, index);
                owner.tlp.ResumeLayout();
            }

            public void RemoveAt(int index)
            {
                owner.items.RemoveAt(index);
                owner.tlp.Controls.RemoveAt(index);
            }

            public FileTransferViewItem this[int index]
            {
                get { return owner.items[index]; }
                set
                {
                    owner.items[index] = value;
                    owner.tlp.SuspendLayout();
                    owner.tlp.Controls.RemoveAt(index);
                    owner.tlp.Controls.Add(value);
                    owner.tlp.Controls.SetChildIndex(value, index);
                    owner.tlp.ResumeLayout();
                }
            }

            #endregion

            #region ICollection<FileTransferItem> Members

            public void Add(FileTransferViewItem item)
            {
                owner.items.Add(item);
                owner.tlp.Controls.Add(item);
            }

            public void Add(string fileName, long fileSize, FileTransferDirection dir)
            {
                var item = new FileTransferViewItem(fileName, fileSize, dir);
                Add(item);
            }

            public void Clear()
            {
                owner.items.Clear();
                owner.tlp.Controls.Clear();
            }

            public bool Contains(FileTransferViewItem item)
            { return owner.items.Contains(item); }

            public void CopyTo(FileTransferViewItem[] array, int arrayIndex)
            { owner.items.CopyTo(array, arrayIndex); }

            public int Count
            { get { return owner.items.Count; } }

            public bool IsReadOnly
            { get { return false; } }

            public bool Remove(FileTransferViewItem item)
            {
                if (owner.items.Remove(item))
                {
                    owner.tlp.Controls.Remove(item);
                    return true;
                }
                return false;
            }

            #endregion

            #region IEnumerable<FileTransferItem> Members

            public IEnumerator<FileTransferViewItem> GetEnumerator()
            { return owner.items.GetEnumerator(); }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            { return owner.items.GetEnumerator(); }

            #endregion
        }
    }
}

using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace EvernoteClone.Model
{
    public class Note
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int NotebookId { get; set; }
        public string Title { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public string FileLocation { get; set; }
    }
}

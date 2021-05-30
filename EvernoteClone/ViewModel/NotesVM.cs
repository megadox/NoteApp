using EvernoteClone.Model;
using EvernoteClone.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace EvernoteClone.ViewModel
{
    public class NotesVM : INotifyPropertyChanged
    {
        public ObservableCollection<Notebook> Notebooks { get; set; }

		private Notebook selectedNotebook;

        public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler SelectedNoteChanged;

        public Notebook SelectedNotebook
		{
			get { return selectedNotebook; }
			set 
			{ 
				selectedNotebook = value;
				OnPropertyChanged("SelectedNotebook");
				GetNotes();
			}
		}

		private Note selectedNote;

		public Note SelectedNote
		{
			get { return selectedNote; }
			set
			{
				selectedNote = value;
				OnPropertyChanged("SelectedNote");
				SelectedNoteChanged?.Invoke(this, new EventArgs());
			}

		}



		private Visibility isVisible;

        public Visibility IsVisible
        {
            get { return isVisible; }
            set 
			{ 
				isVisible = value;
				OnPropertyChanged("IsVisible");
			}
        }


        public ObservableCollection<Note> Notes { get; set; }

		public NewNotebookCommand NewNotebookCommand { get; set; }

		public NewNoteCommand NewNoteCommand { get; set; }

        public EditCommand EditCommand { get; set; }

        public EndEditingCommand EndEditingCommand { get; set; }



        public NotesVM()
		{
			NewNotebookCommand = new NewNotebookCommand(this);
			NewNoteCommand = new NewNoteCommand(this);
			EditCommand = new EditCommand(this);
			EndEditingCommand = new EndEditingCommand(this);

			Notebooks = new ObservableCollection<Notebook>();
			Notes = new ObservableCollection<Note>();

			IsVisible = Visibility.Collapsed;

			GetNotebooks();
			GetNotes();
		}

		public async void CreateNotebook()
		{
			Notebook newNotebook = new Notebook()
			{
				Name="Notebook",
				UserId = App.UserId
			};

			await Helpers.DatabaseHelper.Insert(newNotebook);

			GetNotebooks();
		}

		public async void CreateNote(string notebookId)
		{
			Note newNote = new Note()
			{
				NotebookId = notebookId,
				CreatedTime = DateTime.Now,
				UpdatedTime = DateTime.Now,
				Title = $"Note for {DateTime.Now.ToString()}"
			};

			 await Helpers.DatabaseHelper.Insert(newNote);

			GetNotes();
		}

		public async void GetNotebooks()
        {
			var notebooks = (await Helpers.DatabaseHelper.Read<Notebook>()).Where(n =>n.UserId == App.UserId).ToList();

			Notebooks.Clear();
            foreach (var notebook in notebooks)
            {
				Notebooks.Add(notebook);
            }

        }

		private async void GetNotes()
		{
			if (SelectedNotebook == null) return;

			var notes = (await Helpers.DatabaseHelper.Read<Note>()).Where(n =>n.NotebookId == SelectedNotebook.Id).ToList();

			Notes.Clear();
			foreach (var note in notes)
			{
				Notes.Add(note);
			}

		}

		private void OnPropertyChanged(string propertyname)
        {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

		public void StartEditing()
        {
			//TODO : start editing to true
			IsVisible = Visibility.Visible;
		}

		public  void StopEditing(Notebook notebook)
        {
			IsVisible = Visibility.Collapsed;
			Helpers.DatabaseHelper.Update(notebook);
			GetNotebooks();
		}
	}
}

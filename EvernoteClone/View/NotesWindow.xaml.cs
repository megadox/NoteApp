using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Speech;
using System.Speech.Recognition;
using System.Linq;
using System.Threading;
using System.Windows.Controls.Primitives;
using EvernoteClone.ViewModel;
using System.IO;
using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace EvernoteClone.View
{
    /// <summary>
    /// Interaction logic for NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window
    {
        SpeechRecognitionEngine recognizer;

        NotesVM viewModel;

        public NotesWindow()
        {
            InitializeComponent();

            viewModel = Resources["vm"] as NotesVM;
            viewModel.SelectedNoteChanged += ViewModel_SelectedNoteChanged;

            //var currentCulture = (from r in SpeechRecognitionEngine.InstalledRecognizers()
            //                     where r.Culture.Equals(Thread.CurrentThread.CurrentCulture)
            //                     select r).FirstOrDefault();


            //recognizer = new SpeechRecognitionEngine(Thread.CurrentThread.CurrentCulture);

            //GrammarBuilder builder = new GrammarBuilder();
            //builder.AppendDictation();
            //Grammar grammar = new Grammar(builder);
            //recognizer.LoadGrammar(grammar);
            //recognizer.SetInputToDefaultAudioDevice();
            //recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            var fontFailies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontFamilyComboBox.ItemsSource = fontFailies;

            List<double> fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 28, 48, 72 };
            fontSizeComboBox.ItemsSource = fontSizes;
            
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if(string.IsNullOrEmpty(App.UserId))
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.ShowDialog();

                viewModel.GetNotebooks();
            }
        }

        private async void ViewModel_SelectedNoteChanged(object sender, EventArgs e)
        {
            contentRichTextBox.Document.Blocks.Clear();
            if(viewModel.SelectedNote !=null)
            {
                if (!string.IsNullOrEmpty(viewModel.SelectedNote.FileLocation))
                {
                    string downloadPath = $"{viewModel.SelectedNote.Id}.rtf";
                    await new BlobClient(new Uri(viewModel.SelectedNote.FileLocation)).DownloadToAsync(downloadPath);

                    using (FileStream fileStream = new FileStream(downloadPath, FileMode.Open))
                    {
                        
                        var contents = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
                        contents.Load(fileStream, DataFormats.Rtf);
                    }
                        
                }
            }
            
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizeText = e.Result.Text;
            contentRichTextBox.Document.Blocks.Add(new Paragraph(new Run(recognizeText)));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        bool isRecognize = false;

        private void SpeechButton_Click(object sender, RoutedEventArgs e)
        {
            //Azure Server 이용  : 나중에 구현
             
            // Implement with .Net Framework Speech Reconizer
            if(!isRecognize)
            {
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
                isRecognize = true;
            }
            else
            {
                recognizer.RecognizeAsyncStop();
                isRecognize = false;
            }
            
        }

        private void contentRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int amountCharacters = (new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd)).Text.Length;

            statusTextBox.Text = $"Document length : {amountCharacters} characters"; 
        }

        private void boldButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;
            if(isButtonChecked)
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
            else
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
        }

        private void contentRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectedWeight = contentRichTextBox.Selection.GetPropertyValue(FontWeightProperty);
            boldButton.IsChecked = (selectedWeight != DependencyProperty.UnsetValue) && (selectedWeight.Equals(FontWeights.Bold));

            var selectedStyle = contentRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            italicButton.IsChecked = (selectedStyle != DependencyProperty.UnsetValue) && (selectedStyle.Equals(FontStyles.Italic));

            var selectedDecoration = contentRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            underlineButton.IsChecked = (selectedDecoration != DependencyProperty.UnsetValue) && (selectedDecoration.Equals(TextDecorations.Underline));

            fontFamilyComboBox.SelectedItem = contentRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            fontSizeComboBox.Text = (contentRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty)).ToString();
        }

        private void italicButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;
            if (isButtonChecked)
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
            else
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
        }

        private void underlineButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;
            if (isButtonChecked)
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
            {
                TextDecorationCollection textDecorations;
                (contentRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection).TryRemove(TextDecorations.Underline, out textDecorations);
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, textDecorations);
            }
                
        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fontFamilyComboBox.SelectedItem !=null)
            {
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, fontFamilyComboBox.SelectedItem);
            }
        }

        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, fontSizeComboBox.Text);
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = $"{viewModel.SelectedNote.Id}.rtf";
            string rtfFile = System.IO.Path.Combine(Environment.CurrentDirectory,fileName);            
            
            using (FileStream fileStream = new FileStream(rtfFile, FileMode.Create))
            {
                var contents = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
                contents.Save(fileStream, DataFormats.Rtf);
            }

            viewModel.SelectedNote.FileLocation = await UpdateFile(rtfFile, fileName);
            await ViewModel.Helpers.DatabaseHelper.Update(viewModel.SelectedNote);
        }

        private async Task<string> UpdateFile(string rtfFilePath, string fileName)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=evernotstoragelpa;AccountKey=3A1WOckUjE25NfaP9KwVZGy/DKOkgwe+gyo4E27qJLeoqDUXPKtWMOLpzlCFW1wftBHzHhwQjhRSRNnuxOUXpQ==;EndpointSuffix=core.windows.net";
            string containerName = "notes";

            var container = new BlobContainerClient(connectionString, containerName);
            //container.CreateIfNotExistsAsync
            var blob = container.GetBlobClient(fileName);
            await blob.UploadAsync(rtfFilePath);

            return $"https://evernotstoragelpa.blob.core.windows.net/notes/{fileName}";

        }
    }
}

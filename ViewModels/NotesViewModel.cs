using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Helpers;
using Psicho_Support.Models;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;

namespace Psicho_Support.ViewModels
{
    public class NotesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly TextEmotionAnalyzer _emotionAnalyzer;
        private readonly UserStateService _stateService;
        private readonly AppSession _session;
        private readonly AppState _appState;
        private readonly IDialogService _dialogService;
        private bool _disposed = false;

        // Состояния интерфейса
        private bool _isInEditorMode;
        public bool IsInEditorMode
        {
            get => _isInEditorMode;
            set
            {
                _isInEditorMode = value;
                OnPropertyChanged(nameof(IsInEditorMode));
                OnPropertyChanged(nameof(IsInListMode));
                OnPropertyChanged(nameof(ListModeVisibility));
                OnPropertyChanged(nameof(EditorModeVisibility));
            }
        }

        public bool IsInListMode => !IsInEditorMode;
        public Visibility ListModeVisibility => IsInListMode ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditorModeVisibility => IsInEditorMode ? Visibility.Visible : Visibility.Collapsed;

        // Коллекции
        public ObservableCollection<Notes> Notes { get; set; }

        private ICollectionView _notesView;
        public ICollectionView NotesView
        {
            get => _notesView;
            set
            {
                _notesView = value;
                OnPropertyChanged(nameof(NotesView));
            }
        }

        // Выбранные элементы
        private Notes _selectedNote;
        public Notes SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged(nameof(SelectedNote));

                if (value != null && !IsInEditorMode)
                {
                    EditNote();
                }
            }
        }

        private Notes _editingNote;
        public Notes EditingNote
        {
            get => _editingNote;
            set
            {
                _editingNote = value;
                OnPropertyChanged(nameof(EditingNote));

                if (value != null)
                {
                    EditModeTitle = value.NoteID == 0 ? "Новая заметка" : "Редактирование";
                    UpdateStressProperties();
                }

                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        // Поиск
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                NotesView?.Refresh();
            }
        }

        private string _editModeTitle;
        public string EditModeTitle
        {
            get => _editModeTitle;
            set
            {
                _editModeTitle = value;
                OnPropertyChanged(nameof(EditModeTitle));
            }
        }

        private string _notesStatistics;
        public string NotesStatistics
        {
            get => _notesStatistics;
            set
            {
                _notesStatistics = value;
                OnPropertyChanged(nameof(NotesStatistics));
            }
        }

        public bool CanSave => EditingNote != null &&
                               !string.IsNullOrWhiteSpace(EditingNote.Content);

        public bool CanDelete => EditingNote?.NoteID > 0;

        public Visibility StressInfoVisibility =>
            (EditingNote != null && EditingNote.StressLevel > 0) ? Visibility.Visible : Visibility.Collapsed;

        private string _stressText;
        public string StressText
        {
            get => _stressText;
            set
            {
                _stressText = value;
                OnPropertyChanged(nameof(StressText));
            }
        }

        private string _stressColor;
        public string StressColor
        {
            get => _stressColor;
            set
            {
                _stressColor = value;
                OnPropertyChanged(nameof(StressColor));
            }
        }

        private string _emotionIcon;
        public string EmotionIcon
        {
            get => _emotionIcon;
            set
            {
                _emotionIcon = value;
                OnPropertyChanged(nameof(EmotionIcon));
            }
        }

        private string _emotionColor;
        public string EmotionColor
        {
            get => _emotionColor;
            set
            {
                _emotionColor = value;
                OnPropertyChanged(nameof(EmotionColor));
            }
        }

        public ICommand SaveNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand BackToListCommand { get; }
        public ICommand CreateNewNoteCommand { get; }

        public NotesViewModel(
            AppSession session,
            AppState appState,
            UserStateService stateService,
            IDialogService dialogService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _emotionAnalyzer = new TextEmotionAnalyzer();

            Notes = new ObservableCollection<Notes>();

            LoadNotes();

            NotesView = CollectionViewSource.GetDefaultView(Notes);
            NotesView.Filter = FilterNotes;

            _stateService.StateChanged += OnStateChanged;

            SaveNoteCommand = new RelayCommand(SaveNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            BackToListCommand = new RelayCommand(BackToList);
            CreateNewNoteCommand = new RelayCommand(CreateNewNote);

            UpdateStatistics();
        }

        private void OnStateChanged(int newValue)
        {
            UpdateStatistics();
        }

        private bool FilterNotes(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var note = obj as Notes;
            if (note == null) return false;

            bool titleMatch = note.Title != null &&
                             note.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

            bool contentMatch = note.Content != null &&
                               note.Content.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;

            return titleMatch || contentMatch;
        }

        private void UpdateEmotionForNote(Notes note)
        {
            if (note == null || string.IsNullOrWhiteSpace(note.Content)) return;

            var result = _emotionAnalyzer.AnalyzeAdvanced(note.Content);
            note.StressLevel = result.StressLevel;

            // Добавляем расширенные свойства в заметку (если нужно)
            // Для этого нужно расширить модель Notes или использовать attached properties
        }


        private void LoadNotes()
        {
            try
            {
                if (!_appState.IsAuthenticated) return;

                using (var db = new HealthPsicho_DBEntities())
                {
                    var notes = db.Notes
                        .Where(n => n.UserID == _appState.CurrentUser.UserID)
                        .OrderByDescending(n => n.CreatedAt)
                        .ToList();

                    Notes.Clear();

                    foreach (var note in notes)
                    {
                        // Обновляем эмоциональные свойства для каждой заметки
                        var result = _emotionAnalyzer.AnalyzeAdvanced(note.Content);
                        note.StressLevel = result.StressLevel;

                        // Добавляем расширенные свойства через attached property
                        SetNoteEmotionProperties(note, result);

                        Notes.Add(note);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessageAsync("Ошибка", $"Ошибка загрузки заметок: {ex.Message}",
                    Enums.DialogType.Error);
            }
        }

        private void SetNoteEmotionProperties(Notes note, EmotionResult result)
        {
            // Используем attached properties или временное хранилище
            // Так как модель Notes не имеет полей для эмоций, используем словарь
            _noteEmotionIcons[note.NoteID] = GetEmotionIcon(result.DominantEmotion);
            _noteEmotionColors[note.NoteID] = GetEmotionColor(result.DominantEmotion);
        }

        // Словари для хранения эмоциональных свойств заметок
        private Dictionary<int, string> _noteEmotionIcons = new Dictionary<int, string>();
        private Dictionary<int, string> _noteEmotionColors = new Dictionary<int, string>();

        // Свойства для привязки в UI
        public string GetEmotionIconForNote(Notes note)
        {
            return _noteEmotionIcons.TryGetValue(note.NoteID, out var icon) ? icon : "📝";
        }

        public string GetEmotionColorForNote(Notes note)
        {
            return _noteEmotionColors.TryGetValue(note.NoteID, out var color) ? color : "#2A2A3D";
        }

        private string GetEmotionIcon(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Happiness:
                    return "😊";
                case EmotionType.Calm:
                    return "😌";
                case EmotionType.Anxiety:
                    return "😰";
                case EmotionType.Stress:
                    return "😣";
                case EmotionType.Anger:
                    return "😡";
                case EmotionType.Sadness:
                    return "😔";
                case EmotionType.Burnout:
                    return "🥱";
                default:
                    return "😐";
            }
        }

        private string GetEmotionColor(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Happiness:
                    return "#4CAF50";
                case EmotionType.Calm:
                    return "#8BC34A";
                case EmotionType.Anxiety:
                    return "#FFC107";
                case EmotionType.Stress:
                    return "#FF9800";
                case EmotionType.Anger:
                    return "#F44336";
                case EmotionType.Sadness:
                    return "#9E9E9E";
                case EmotionType.Burnout:
                    return "#795548";
                default:
                    return "#2A2A3D";
            }
        }

        private void UpdateStatistics()
        {
            if (Notes.Any())
            {
                var avgStress = Notes.Where(n => n.StressLevel.HasValue)
                                    .Average(n => n.StressLevel.Value);
                NotesStatistics = $"Всего заметок: {Notes.Count} | Средний стресс: {avgStress:F0}%";
            }
            else
            {
                NotesStatistics = "Нет заметок. Создайте первую заметку!";
            }
        }

        private void UpdateEmotionUI(EmotionResult result)
        {
            switch (result.DominantEmotion)
            {
                case EmotionType.Happiness:
                    EmotionIcon = "😊";
                    EmotionColor = "#4CAF50";
                    break;
                case EmotionType.Calm:
                    EmotionIcon = "😌";
                    EmotionColor = "#8BC34A";
                    break;
                case EmotionType.Anxiety:
                    EmotionIcon = "😰";
                    EmotionColor = "#FFC107";
                    break;
                case EmotionType.Stress:
                    EmotionIcon = "😣";
                    EmotionColor = "#FF9800";
                    break;
                case EmotionType.Anger:
                    EmotionIcon = "😡";
                    EmotionColor = "#F44336";
                    break;
                case EmotionType.Sadness:
                    EmotionIcon = "😔";
                    EmotionColor = "#9E9E9E";
                    break;
                case EmotionType.Burnout:
                    EmotionIcon = "🥱";
                    EmotionColor = "#795548";
                    break;
                default:
                    EmotionIcon = "😐";
                    EmotionColor = "#BDBDBD";
                    break;
            }

            StressText = $"{result.DominantEmotion} ({result.Confidence:P0})";
        }

        private void CreateNewNote()
        {
            if (!_appState.IsAuthenticated) return;

            EditingNote = new Notes
            {
                UserID = _appState.CurrentUser.UserID,
                Title = "Новая заметка",
                Content = "",
                CreatedAt = DateTime.Now,
                StressLevel = 50
            };

            IsInEditorMode = true;
        }

        private void EditNote()
        {
            if (SelectedNote == null) return;

            EditingNote = new Notes
            {
                NoteID = SelectedNote.NoteID,
                UserID = SelectedNote.UserID,
                Title = SelectedNote.Title,
                Content = SelectedNote.Content,
                StressLevel = SelectedNote.StressLevel,
                CreatedAt = SelectedNote.CreatedAt
            };

            IsInEditorMode = true;
        }

        private async void SaveNote()
        {
            if (!CanSave) return;

            try
            {
                string title = EditingNote.Content
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? "Заметка";

                if (title.Length > 50)
                    title = title.Substring(0, 47) + "...";

                EditingNote.Title = title;

                var result = _emotionAnalyzer.AnalyzeAdvanced(EditingNote.Content);
                EditingNote.StressLevel = result.StressLevel;

                UpdateEmotionUI(result);

                using (var db = new HealthPsicho_DBEntities())
                {
                    if (EditingNote.NoteID == 0)
                    {
                        db.Notes.Add(EditingNote);
                        db.SaveChanges();

                        EditingNote.NoteID = db.Notes
                            .OrderByDescending(n => n.CreatedAt)
                            .First(n => n.UserID == EditingNote.UserID).NoteID;

                        Notes.Insert(0, EditingNote);
                    }
                    else
                    {
                        var dbNote = db.Notes.FirstOrDefault(n => n.NoteID == EditingNote.NoteID);
                        if (dbNote != null)
                        {
                            dbNote.Title = EditingNote.Title;
                            dbNote.Content = EditingNote.Content;
                            dbNote.StressLevel = EditingNote.StressLevel;
                            db.SaveChanges();
                        }

                        var existingNote = Notes.FirstOrDefault(n => n.NoteID == EditingNote.NoteID);
                        if (existingNote != null)
                        {
                            existingNote.Title = EditingNote.Title;
                            existingNote.Content = EditingNote.Content;
                            existingNote.StressLevel = EditingNote.StressLevel;
                        }
                    }
                }

                NotesView.Refresh();

                if (_appState.IsAuthenticated)
                    _stateService.RecalculateState(_appState.CurrentUser.UserID);

                UpdateStatistics();

                if (EditingNote.NoteID == 0)
                {
                    await _dialogService.ShowMessageAsync("Успех", "Заметка успешно создана!", Enums.DialogType.Success);
                }


                BackToList();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка", $"Ошибка сохранения: {ex.Message}", Enums.DialogType.Error);
            }
        }

        private async void DeleteNote()
        {
            if (!CanDelete) return;

            var result = await _dialogService.ShowConfirmationAsync("Подтверждение удаления",
                "Вы уверены, что хотите удалить эту заметку?");

            if (!result) return;

            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var dbNote = db.Notes.FirstOrDefault(n => n.NoteID == EditingNote.NoteID);
                    if (dbNote != null)
                    {
                        db.Notes.Remove(dbNote);
                        db.SaveChanges();
                    }
                }

                var noteToRemove = Notes.FirstOrDefault(n => n.NoteID == EditingNote.NoteID);
                if (noteToRemove != null)
                    Notes.Remove(noteToRemove);

                if (_appState.IsAuthenticated)
                    _stateService.RecalculateState(_appState.CurrentUser.UserID);

                UpdateStatistics();

                await _dialogService.ShowMessageAsync("Успех", "Заметка удалена.", Enums.DialogType.Success);

                BackToList();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка", $"Ошибка удаления: {ex.Message}", Enums.DialogType.Error);
            }
        }

        private void BackToList()
        {
            IsInEditorMode = false;
            EditingNote = null;
            SelectedNote = null;
        }

        private void UpdateStressProperties()
        {
            if (EditingNote == null) return;

            int stress = EditingNote.StressLevel ?? 50;

            if (stress <= 20) StressText = "Низкий";
            else if (stress <= 40) StressText = "Ниже среднего";
            else if (stress <= 60) StressText = "Средний";
            else if (stress <= 80) StressText = "Выше среднего";
            else StressText = "Высокий";

            if (stress <= 20) StressColor = "#4CAF50";
            else if (stress <= 40) StressColor = "#8BC34A";
            else if (stress <= 60) StressColor = "#FFC107";
            else if (stress <= 80) StressColor = "#FF9800";
            else StressColor = "#F44336";
        }

        private int ConvertScoreToStress(double score)
        {
            int stress = (int)((1 - score) * 50);
            if (stress < 0) stress = 0;
            if (stress > 100) stress = 100;
            return stress;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_stateService != null)
                {
                    _stateService.StateChanged -= OnStateChanged;
                }
                _disposed = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
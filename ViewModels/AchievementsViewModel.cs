// ViewModels/AchievementsViewModel.cs (исправленная версия)
using Psicho_Support.Data;
using Psicho_Support.Enums;
using Psicho_Support.Helpers;
using Psicho_Support.Services;
using Psicho_Support.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class AchievementsViewModel : BaseViewModel, IDisposable
    {
        private readonly AppSession _session;
        private bool _disposed = false;

        public ObservableCollection<AchievementItem> Achievements { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private int _totalProgress;
        public int TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        private int _unlockedCount;
        public int UnlockedCount
        {
            get => _unlockedCount;
            set => SetProperty(ref _unlockedCount, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public ICommand RefreshCommand { get; }

        public AchievementsViewModel(IDialogService dialogService, INavigationService navigationService, AppSession session)
            : base(dialogService, navigationService)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            Achievements = new ObservableCollection<AchievementItem>();
            RefreshCommand = new RelayCommand(LoadAchievements);

            LoadAchievements();
        }

        private async void LoadAchievements()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                if (_session.CurrentUser == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return;
                }

                var userId = _session.CurrentUser.UserID;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    using (var db = new HealthPsicho_DBEntities())
                    {
                        // Получаем все достижения
                        var allAchievements = db.Achievements.ToList();

                        // Получаем ID достижений, которые получил пользователь
                        var userAchievementIds = db.UserAchievements
                            .Where(ua => ua.UserID == userId)
                            .Select(ua => ua.AchievementID)
                            .ToHashSet();

                        // Получаем даты получения достижений (используем правильное имя поля DateEarned)
                        var userAchievementsDates = db.UserAchievements
                            .Where(ua => ua.UserID == userId)
                            .ToDictionary(ua => ua.AchievementID, ua => ua.DateEarned);

                        // Формируем список элементов
                        var items = new List<AchievementItem>();

                        foreach (var achievement in allAchievements)
                        {
                            bool isUnlocked = userAchievementIds.Contains(achievement.AchievementID);

                            var item = new AchievementItem
                            {
                                Id = achievement.AchievementID,
                                Title = achievement.Title ?? "Без названия",
                                Description = achievement.Description ?? "Описание отсутствует",
                                IconPath = achievement.IconPath ?? "🏆",
                                IsUnlocked = isUnlocked,
                                UnlockedDate = isUnlocked && userAchievementsDates.ContainsKey(achievement.AchievementID)
                                    ? userAchievementsDates[achievement.AchievementID]
                                    : null
                            };

                            items.Add(item);
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Achievements.Clear();
                            foreach (var item in items)
                            {
                                Achievements.Add(item);
                            }

                            TotalCount = allAchievements.Count;
                            UnlockedCount = items.Count(i => i.IsUnlocked);
                            TotalProgress = TotalCount > 0 ? (UnlockedCount * 100 / TotalCount) : 0;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки достижений: {ex.Message}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Не удалось загрузить достижения: {ex.Message}";
                    DialogService?.Show("Ошибка", ErrorMessage, DialogType.Error);
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsLoading = false);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Achievements?.Clear();
                _disposed = true;
            }
        }
    }

    public class AchievementItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedDate { get; set; }

        public string UnlockedDateText
        {
            get
            {
                if (!IsUnlocked) return "🔒 Не получено";
                if (!UnlockedDate.HasValue) return "📅 Дата неизвестна";
                return $"🏅 Получено: {UnlockedDate.Value:dd.MM.yyyy}";
            }
        }

        public string StatusColor
        {
            get => IsUnlocked ? "#4CAF50" : "#757575";
        }

        public double Opacity
        {
            get => IsUnlocked ? 1.0 : 0.5;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
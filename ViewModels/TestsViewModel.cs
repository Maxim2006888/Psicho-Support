using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Helpers;
using Psicho_Support.Models;
using Psicho_Support.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class TestsViewModel : BaseViewModel
    {
        private readonly AppState _appState;
        private readonly IDialogService _dialogService;

        // Коллекция доступных тестов
        private ObservableCollection<Tests> _availableTests;
        public ObservableCollection<Tests> AvailableTests
        {
            get => _availableTests;
            set => SetProperty(ref _availableTests, value);
        }

        // Поиск
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTests();
                }
            }
        }

        // Вопросы текущего теста
        public ObservableCollection<TestQuestion> CurrentQuestions { get; set; }

        private int _currentQuestionIndex;
        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
            }
        }

        public TestQuestion CurrentQuestion =>
            CurrentQuestions != null && CurrentQuestions.Count > 0 && CurrentQuestionIndex < CurrentQuestions.Count
            ? CurrentQuestions[CurrentQuestionIndex]
            : null;

        public string CurrentQuestionNumber =>
            CurrentQuestions != null && CurrentQuestions.Count > 0
            ? $"{CurrentQuestionIndex + 1} / {CurrentQuestions.Count}"
            : "0 / 0";

        private bool _isTestStarted;
        public bool IsTestStarted
        {
            get => _isTestStarted;
            set
            {
                _isTestStarted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTestNotStarted));
                // Убираем RaiseCanExecuteChanged, используем CommandManager
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsTestNotStarted => !IsTestStarted;

        private bool _isTestCompleted;
        public bool IsTestCompleted
        {
            get => _isTestCompleted;
            set
            {
                _isTestCompleted = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _totalScore;
        public int TotalScore
        {
            get => _totalScore;
            set { _totalScore = value; OnPropertyChanged(); }
        }

        private string _testResultMessage;
        public string TestResultMessage
        {
            get => _testResultMessage;
            set { _testResultMessage = value; OnPropertyChanged(); }
        }

        private string _resultColor;
        public string ResultColor
        {
            get => _resultColor;
            set { _resultColor = value; OnPropertyChanged(); }
        }

        private Tests _selectedTest;
        public Tests SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
                if (value != null && !IsTestStarted)
                {
                    StartTest(value);
                }
            }
        }

        public ICommand StartTestCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand ResetTestCommand { get; }

        public TestsViewModel(
            AppState appState,
            IDialogService dialogService,
            INavigationService navigationService)
            : base(dialogService, navigationService)
        {
            _appState = appState ?? throw new System.ArgumentNullException(nameof(appState));
            _dialogService = dialogService ?? throw new System.ArgumentNullException(nameof(dialogService));

            Title = "Тесты";

            // Инициализация коллекций
            AvailableTests = new ObservableCollection<Tests>();
            CurrentQuestions = new ObservableCollection<TestQuestion>();

            // Команды
            StartTestCommand = new RelayCommand(StartTest);
            SubmitAnswerCommand = new RelayCommand(SubmitAnswer, CanSubmitAnswer);
            ResetTestCommand = new RelayCommand(ResetTest);

            // Загрузка доступных тестов
            LoadAvailableTests();
        }

        private bool CanSubmitAnswer(object parameter)
        {
            return IsTestStarted && !IsTestCompleted;
        }

        private void LoadAvailableTests()
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var tests = db.Tests.ToList();
                    AvailableTests.Clear();
                    foreach (var test in tests)
                    {
                        AvailableTests.Add(test);
                    }
                }
            }
            catch
            {
                // Если нет данных в БД, создаем тестовые данные
                AvailableTests.Clear();
                AvailableTests.Add(new Tests
                {
                    TestID = 1,
                    TestName = "Оценка эмоционального состояния",
                    Description = "Тест для определения текущего эмоционального состояния"
                });
                AvailableTests.Add(new Tests
                {
                    TestID = 2,
                    TestName = "Уровень стресса",
                    Description = "Оценка уровня стресса за последнюю неделю"
                });
                AvailableTests.Add(new Tests
                {
                    TestID = 3,
                    TestName = "Качество сна",
                    Description = "Оценка качества сна и его влияния на состояние"
                });
            }
        }

        private void FilterTests()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadAvailableTests();
                return;
            }

            // Сохраняем оригинальную коллекцию
            var allTests = new ObservableCollection<Tests>();
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var tests = db.Tests.ToList();
                    foreach (var test in tests)
                    {
                        allTests.Add(test);
                    }
                }
            }
            catch
            {
                // Тестовые данные
                allTests.Add(new Tests { TestID = 1, TestName = "Оценка эмоционального состояния", Description = "Тест для определения текущего эмоционального состояния" });
                allTests.Add(new Tests { TestID = 2, TestName = "Уровень стресса", Description = "Оценка уровня стресса за последнюю неделю" });
                allTests.Add(new Tests { TestID = 3, TestName = "Качество сна", Description = "Оценка качества сна и его влияния на состояние" });
            }

            var filtered = allTests
                .Where(t => (t.TestName?.ToLower().Contains(SearchText.ToLower()) ?? false) ||
                           (t.Description?.ToLower().Contains(SearchText.ToLower()) ?? false))
                .ToList();

            AvailableTests.Clear();
            foreach (var test in filtered)
            {
                AvailableTests.Add(test);
            }
        }

        private void StartTest(object obj)
        {
            Tests test = null;

            if (obj is Tests selectedTest)
            {
                test = selectedTest;
            }
            else if (SelectedTest != null)
            {
                test = SelectedTest;
            }

            if (test == null) return;

            // Загружаем вопросы теста
            LoadTestQuestions(test);

            CurrentQuestionIndex = 0;
            TotalScore = 0;
            IsTestStarted = true;
            IsTestCompleted = false;
            TestResultMessage = string.Empty;
            ResultColor = string.Empty;

            CommandManager.InvalidateRequerySuggested();
        }

        private void LoadTestQuestions(Tests test)
        {
            CurrentQuestions.Clear();

            // Создаем вопросы в зависимости от теста
            switch (test.TestID)
            {
                case 1: // Эмоциональное состояние
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 1,
                        Text = "Как вы себя чувствуете сегодня?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Очень плохо", 0),
                            new TestAnswer("Плохо", 1),
                            new TestAnswer("Нормально", 2),
                            new TestAnswer("Хорошо", 3),
                            new TestAnswer("Отлично", 4),
                        }
                    });
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 2,
                        Text = "Как часто вы чувствуете радость?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Никогда", 0),
                            new TestAnswer("Редко", 1),
                            new TestAnswer("Иногда", 2),
                            new TestAnswer("Часто", 3),
                            new TestAnswer("Всегда", 4),
                        }
                    });
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 3,
                        Text = "Как вы оцениваете свою энергичность?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Очень низкая", 0),
                            new TestAnswer("Низкая", 1),
                            new TestAnswer("Средняя", 2),
                            new TestAnswer("Высокая", 3),
                            new TestAnswer("Очень высокая", 4),
                        }
                    });
                    break;

                case 2: // Уровень стресса
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 1,
                        Text = "Как часто вы чувствуете напряжение?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Постоянно", 4),
                            new TestAnswer("Часто", 3),
                            new TestAnswer("Иногда", 2),
                            new TestAnswer("Редко", 1),
                            new TestAnswer("Никогда", 0)
                        }
                    });
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 2,
                        Text = "Бывает ли у вас бессонница?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Почти каждую ночь", 4),
                            new TestAnswer("Часто", 3),
                            new TestAnswer("Иногда", 2),
                            new TestAnswer("Редко", 1),
                            new TestAnswer("Никогда", 0)
                        }
                    });
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 3,
                        Text = "Как часто вы испытываете раздражительность?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Постоянно", 4),
                            new TestAnswer("Часто", 3),
                            new TestAnswer("Иногда", 2),
                            new TestAnswer("Редко", 1),
                            new TestAnswer("Никогда", 0)
                        }
                    });
                    break;

                case 3: // Качество сна
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 1,
                        Text = "Сколько времени вам нужно чтобы уснуть?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Более 60 минут", 0),
                            new TestAnswer("30-60 минут", 1),
                            new TestAnswer("15-30 минут", 2),
                            new TestAnswer("5-15 минут", 3),
                            new TestAnswer("Меньше 5 минут", 4)
                        }
                    });
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 2,
                        Text = "Как вы оцениваете качество своего сна?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Очень плохое", 0),
                            new TestAnswer("Плохое", 1),
                            new TestAnswer("Среднее", 2),
                            new TestAnswer("Хорошее", 3),
                            new TestAnswer("Отличное", 4)
                        }
                    });
                    break;

                default:
                    CurrentQuestions.Add(new TestQuestion
                    {
                        QuestionID = 1,
                        Text = "Как вы себя чувствуете сегодня?",
                        Answers = new ObservableCollection<TestAnswer>
                        {
                            new TestAnswer("Очень плохо", 0),
                            new TestAnswer("Плохо", 1),
                            new TestAnswer("Нормально", 2),
                            new TestAnswer("Хорошо", 3),
                            new TestAnswer("Отлично", 4),
                        }
                    });
                    break;
            }
        }

        private void SubmitAnswer(object param)
        {
            if (!IsTestStarted || IsTestCompleted) return;

            if (param is TestAnswer answer)
            {
                TotalScore += answer.Score;

                if (CurrentQuestionIndex < CurrentQuestions.Count - 1)
                {
                    CurrentQuestionIndex++;
                }
                else
                {
                    FinishTest();
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void FinishTest()
        {
            IsTestStarted = false;
            IsTestCompleted = true;

            // Анализ результата
            int maxScore = CurrentQuestions.Count * 4;
            double percentage = maxScore > 0 ? (double)TotalScore / maxScore * 100 : 0;

            if (percentage >= 80)
            {
                TestResultMessage = "Отличный результат! Ваше эмоциональное состояние в норме.";
                ResultColor = "#4CAF50";
            }
            else if (percentage >= 60)
            {
                TestResultMessage = "Хороший результат. Есть небольшие зоны для улучшения.";
                ResultColor = "#8BC34A";
            }
            else if (percentage >= 40)
            {
                TestResultMessage = "Средний результат. Рекомендуется обратить внимание на свое состояние.";
                ResultColor = "#FFC107";
            }
            else if (percentage >= 20)
            {
                TestResultMessage = "Результат ниже среднего. Рекомендуется проконсультироваться со специалистом.";
                ResultColor = "#FF9800";
            }
            else
            {
                TestResultMessage = "Требуется внимание! Пожалуйста, обратитесь к психологу.";
                ResultColor = "#F44336";
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetTest(object obj)
        {
            IsTestStarted = false;
            IsTestCompleted = false;
            CurrentQuestions.Clear();
            TotalScore = 0;
            TestResultMessage = string.Empty;
            ResultColor = string.Empty;
            SelectedTest = null;

            CommandManager.InvalidateRequerySuggested();
        }

        public override async System.Threading.Tasks.Task InitializeAsync(object parameter = null)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
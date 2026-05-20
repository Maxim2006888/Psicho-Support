using Psicho_Support.Core;
using Psicho_Support.Data;
using Psicho_Support.Helpers;
using Psicho_Support.Models;
using Psicho_Support.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Psicho_Support.ViewModels
{
    public class TestsViewModel : BaseViewModel
    {
        private readonly AppState _appState;
        private readonly IDialogService _dialogService;

        private ObservableCollection<Tests> _availableTests;
        public ObservableCollection<Tests> AvailableTests
        {
            get => _availableTests;
            set => SetProperty(ref _availableTests, value);
        }

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
            CurrentQuestions != null &&
            CurrentQuestions.Count > 0 &&
            CurrentQuestionIndex < CurrentQuestions.Count
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
            set
            {
                _totalScore = value;
                OnPropertyChanged();
            }
        }

        private string _testResultMessage;
        public string TestResultMessage
        {
            get => _testResultMessage;
            set
            {
                _testResultMessage = value;
                OnPropertyChanged();
            }
        }

        private string _resultColor;
        public string ResultColor
        {
            get => _resultColor;
            set
            {
                _resultColor = value;
                OnPropertyChanged();
            }
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
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Title = "Тесты";

            AvailableTests = new ObservableCollection<Tests>();
            CurrentQuestions = new ObservableCollection<TestQuestion>();

            StartTestCommand = new RelayCommand(StartTest);
            SubmitAnswerCommand = new RelayCommand(SubmitAnswer, CanSubmitAnswer);
            ResetTestCommand = new RelayCommand(ResetTest);

            LoadAvailableTests();
        }

        private bool CanSubmitAnswer(object parameter)
        {
            return IsTestStarted && !IsTestCompleted;
        }

        private void LoadAvailableTests()
        {
            AvailableTests.Clear();

            foreach (var test in GetAvailableTestsSource())
            {
                AvailableTests.Add(test);
            }
        }

        private IEnumerable<Tests> GetAvailableTestsSource()
        {
            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var tests = db.Tests.ToList();

                    if (tests.Count > 0)
                    {
                        return tests;
                    }
                }
            }
            catch
            {
                // Если база данных недоступна, используем встроенный каталог тестов.
            }

            return CreateBuiltInTests();
        }

        private IEnumerable<Tests> CreateBuiltInTests()
        {
            return new List<Tests>
            {
                new Tests
                {
                    TestID = 1,
                    TestName = "Эмоциональный чек-ин",
                    Description = "Короткая ежедневная самооценка настроения, энергии и интереса к делам."
                },
                new Tests
                {
                    TestID = 2,
                    TestName = "Уровень стресса",
                    Description = "Помогает заметить напряжение, раздражительность и перегрузку за последнюю неделю."
                },
                new Tests
                {
                    TestID = 3,
                    TestName = "Качество сна",
                    Description = "Оценка засыпания, восстановления и дневной сонливости."
                },
                new Tests
                {
                    TestID = 4,
                    TestName = "Усталость от компьютера",
                    Description = "Для офисных сотрудников и учащихся: экранная нагрузка, перерывы, глаза и тело."
                },
                new Tests
                {
                    TestID = 5,
                    TestName = "Тревожность и безопасность",
                    Description = "Мягкий самоанализ беспокойства, навязчивых мыслей и чувства контроля."
                },
                new Tests
                {
                    TestID = 6,
                    TestName = "Ресурс восстановления",
                    Description = "Подходит во время реабилитации: отслеживает силы, поддержку и готовность к активности."
                }
            };
        }

        private void FilterTests()
        {
            var allTests = GetAvailableTestsSource();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var query = SearchText.Trim();

                allTests = allTests.Where(t =>
                    (t.TestName != null && t.TestName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.Description != null && t.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            AvailableTests.Clear();

            foreach (var test in allTests)
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

            if (test == null)
            {
                return;
            }

            _selectedTest = test;
            OnPropertyChanged(nameof(SelectedTest));

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

            switch (test.TestID)
            {
                case 1:
                    AddQuestion(1, "Какое настроение у вас преобладало сегодня?",
                        Answer("Подавленное или очень тяжелое", 0),
                        Answer("Скорее плохое", 1),
                        Answer("Ровное, без сильных эмоций", 2),
                        Answer("Скорее хорошее", 3),
                        Answer("Спокойное, радостное или вдохновленное", 4));

                    AddQuestion(2, "Насколько вам было интересно заниматься обычными делами?",
                        Answer("Совсем не было интереса", 0),
                        Answer("Интерес появлялся редко", 1),
                        Answer("Иногда было интересно", 2),
                        Answer("Чаще было интересно", 3),
                        Answer("Было много вовлеченности", 4));

                    AddQuestion(3, "Как вы оцениваете свой уровень энергии?",
                        Answer("Почти нет сил", 0),
                        Answer("Сил мало", 1),
                        Answer("Хватает на базовые дела", 2),
                        Answer("Сил достаточно", 3),
                        Answer("Чувствую бодрость и запас сил", 4));

                    AddQuestion(4, "Получалось ли вам замечать и называть свои эмоции?",
                        Answer("Не получалось", 0),
                        Answer("С большим трудом", 1),
                        Answer("Иногда получалось", 2),
                        Answer("Чаще получалось", 3),
                        Answer("Легко понимаю, что чувствую", 4));

                    AddQuestion(5, "Были ли у вас моменты спокойствия или удовольствия?",
                        Answer("Нет, совсем не было", 0),
                        Answer("Очень мало", 1),
                        Answer("Несколько коротких моментов", 2),
                        Answer("Достаточно заметные моменты", 3),
                        Answer("Да, много таких моментов", 4));
                    break;

                case 2:
                    AddQuestion(1, "Как часто за последнюю неделю вы чувствовали внутреннее напряжение?",
                        Answer("Почти постоянно", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Никогда", 4));

                    AddQuestion(2, "Насколько сложно было расслабиться после учебы, работы или лечения?",
                        Answer("Почти не получается", 0),
                        Answer("Часто сложно", 1),
                        Answer("Получается не всегда", 2),
                        Answer("Иногда нужно время", 3),
                        Answer("Легко расслабляюсь", 4));

                    AddQuestion(3, "Как часто вы раздражались из-за мелочей?",
                        Answer("Почти постоянно", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Никогда", 4));

                    AddQuestion(4, "Были ли телесные признаки стресса: зажимы, головная боль, сердцебиение?",
                        Answer("Очень часто", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Не замечал(а)", 4));

                    AddQuestion(5, "Насколько стресс мешал вашим обычным делам?",
                        Answer("Почти полностью мешал", 0),
                        Answer("Сильно мешал", 1),
                        Answer("Заметно мешал", 2),
                        Answer("Слегка мешал", 3),
                        Answer("Не мешал", 4));
                    break;

                case 3:
                    AddQuestion(1, "Сколько времени вам обычно нужно, чтобы уснуть?",
                        Answer("Более 60 минут", 0),
                        Answer("30-60 минут", 1),
                        Answer("15-30 минут", 2),
                        Answer("5-15 минут", 3),
                        Answer("Меньше 5 минут", 4));

                    AddQuestion(2, "Как часто вы просыпались ночью и долго не могли уснуть снова?",
                        Answer("Почти каждую ночь", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Почти никогда", 4));

                    AddQuestion(3, "Насколько восстановленным вы чувствовали себя утром?",
                        Answer("Совсем не восстановленным", 0),
                        Answer("Скорее уставшим", 1),
                        Answer("Средне", 2),
                        Answer("Скорее бодрым", 3),
                        Answer("Полностью восстановленным", 4));

                    AddQuestion(4, "Как часто сонливость мешала днем?",
                        Answer("Почти каждый день", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Не мешала", 4));
                    break;

                case 4:
                    AddQuestion(1, "Сколько часов в день вы проводите за экраном без учета коротких перерывов?",
                        Answer("Более 8 часов", 0),
                        Answer("6-8 часов", 1),
                        Answer("4-6 часов", 2),
                        Answer("2-4 часа", 3),
                        Answer("Менее 2 часов", 4));

                    AddQuestion(2, "Как часто вы делаете перерывы для глаз и тела?",
                        Answer("Почти никогда", 0),
                        Answer("Редко", 1),
                        Answer("Иногда", 2),
                        Answer("Каждый час", 3),
                        Answer("Регулярно каждые 30-45 минут", 4));

                    AddQuestion(3, "Как часто к концу дня появляются усталость глаз, сухость или головная боль?",
                        Answer("Почти всегда", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Почти никогда", 4));

                    AddQuestion(4, "Получается ли сохранять удобную позу и расслабленные плечи?",
                        Answer("Почти никогда", 0),
                        Answer("Редко", 1),
                        Answer("Иногда", 2),
                        Answer("Часто", 3),
                        Answer("Почти всегда", 4));

                    AddQuestion(5, "Остаются ли силы на отдых, прогулку или общение после экранной нагрузки?",
                        Answer("Совсем не остаются", 0),
                        Answer("Очень мало", 1),
                        Answer("Иногда остаются", 2),
                        Answer("Чаще остаются", 3),
                        Answer("Да, сил достаточно", 4));
                    break;

                case 5:
                    AddQuestion(1, "Как часто за последнюю неделю вас беспокоили тревожные мысли?",
                        Answer("Почти постоянно", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Почти никогда", 4));

                    AddQuestion(2, "Насколько легко вам переключаться с тревожных мыслей на дела?",
                        Answer("Почти невозможно", 0),
                        Answer("Очень сложно", 1),
                        Answer("Иногда получается", 2),
                        Answer("Чаще получается", 3),
                        Answer("Получается достаточно легко", 4));

                    AddQuestion(3, "Чувствовали ли вы, что ситуация выходит из-под контроля?",
                        Answer("Почти постоянно", 0),
                        Answer("Часто", 1),
                        Answer("Иногда", 2),
                        Answer("Редко", 3),
                        Answer("Почти никогда", 4));

                    AddQuestion(4, "Есть ли рядом взрослый, близкий человек или специалист, к кому можно обратиться?",
                        Answer("Нет никого", 0),
                        Answer("Не уверен(а)", 1),
                        Answer("Есть, но сложно обратиться", 2),
                        Answer("Есть один человек", 3),
                        Answer("Есть несколько надежных людей", 4));

                    AddQuestion(5, "Если становится тяжело, можете ли вы использовать безопасный способ успокоиться?",
                        Answer("Не знаю таких способов", 0),
                        Answer("Редко получается", 1),
                        Answer("Иногда помогает", 2),
                        Answer("Чаще помогает", 3),
                        Answer("Да, у меня есть рабочие способы", 4));
                    break;

                case 6:
                    AddQuestion(1, "Как вы оцениваете запас физических сил сегодня?",
                        Answer("Сил почти нет", 0),
                        Answer("Сил мало", 1),
                        Answer("Хватает на минимум", 2),
                        Answer("Сил достаточно", 3),
                        Answer("Чувствую хороший запас", 4));

                    AddQuestion(2, "Насколько понятен ваш план восстановления или режима на день?",
                        Answer("Совсем непонятен", 0),
                        Answer("Скорее непонятен", 1),
                        Answer("Понятен частично", 2),
                        Answer("В целом понятен", 3),
                        Answer("Полностью понятен", 4));

                    AddQuestion(3, "Получается ли соблюдать рекомендованный темп без перегрузки?",
                        Answer("Почти никогда", 0),
                        Answer("Редко", 1),
                        Answer("Иногда", 2),
                        Answer("Часто", 3),
                        Answer("Почти всегда", 4));

                    AddQuestion(4, "Как часто вы отмечаете даже небольшие успехи в восстановлении?",
                        Answer("Никогда", 0),
                        Answer("Редко", 1),
                        Answer("Иногда", 2),
                        Answer("Часто", 3),
                        Answer("Каждый день", 4));

                    AddQuestion(5, "Насколько вы чувствуете поддержку окружающих или специалистов?",
                        Answer("Не чувствую", 0),
                        Answer("Поддержки мало", 1),
                        Answer("Иногда чувствую", 2),
                        Answer("Часто чувствую", 3),
                        Answer("Чувствую устойчивую поддержку", 4));
                    break;

                default:
                    AddQuestion(1, "Как вы себя чувствуете сегодня?",
                        Answer("Очень плохо", 0),
                        Answer("Плохо", 1),
                        Answer("Нормально", 2),
                        Answer("Хорошо", 3),
                        Answer("Отлично", 4));
                    break;
            }
        }

        private void AddQuestion(int questionId, string text, params TestAnswer[] answers)
        {
            CurrentQuestions.Add(new TestQuestion
            {
                QuestionID = questionId,
                Text = text,
                Answers = new ObservableCollection<TestAnswer>(answers)
            });
        }

        private TestAnswer Answer(string text, int score)
        {
            return new TestAnswer(text, score);
        }

        private void SubmitAnswer(object param)
        {
            if (!IsTestStarted || IsTestCompleted)
            {
                return;
            }

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

            int maxScore = CurrentQuestions.Count * 4;
            double percentage = maxScore > 0
                ? (double)TotalScore / maxScore * 100
                : 0;

            TestResultMessage = BuildResultMessage(percentage);
            ResultColor = GetResultColor(percentage);

            SaveTestResult();
            CommandManager.InvalidateRequerySuggested();
        }

        private void SaveTestResult()
        {
            var userId = _appState.CurrentUser?.UserID;
            if (!userId.HasValue || SelectedTest == null)
            {
                return;
            }

            try
            {
                using (var db = new HealthPsicho_DBEntities())
                {
                    var testId = ResolveTestId(db, SelectedTest);

                    db.TestResults.Add(new TestResults
                    {
                        UserID = userId.Value,
                        TestID = testId,
                        Score = TotalScore,
                        Date = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var details = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения результата теста: {details}");
            }
        }

        private int ResolveTestId(HealthPsicho_DBEntities db, Tests selectedTest)
        {
            var byId = db.Tests.FirstOrDefault(t => t.TestID == selectedTest.TestID);
            if (byId != null)
            {
                return byId.TestID;
            }

            var byName = db.Tests.FirstOrDefault(t => t.TestName == selectedTest.TestName);
            if (byName != null)
            {
                return byName.TestID;
            }

            var created = db.Tests.Add(new Tests
            {
                TestName = selectedTest.TestName,
                Description = selectedTest.Description
            });

            db.SaveChanges();
            return created.TestID;
        }

        private string BuildResultMessage(double percentage)
        {
            var prefix = SelectedTest != null
                ? SelectedTest.TestName + ": "
                : string.Empty;

            if (percentage >= 80)
            {
                return prefix + "ресурсное состояние. Продолжайте поддерживать режим, перерывы и привычки, которые помогают вам чувствовать себя устойчиво.";
            }

            if (percentage >= 60)
            {
                return prefix + "в целом состояние стабильное, но есть зоны для бережной профилактики. Попробуйте отметить, какие факторы сильнее всего влияют на результат.";
            }

            if (percentage >= 40)
            {
                return prefix + "умеренная нагрузка. Рекомендуется снизить темп, добавить отдых, дыхательную паузу или обсудить состояние с близким взрослым/специалистом.";
            }

            if (percentage >= 20)
            {
                return prefix + "заметная зона риска. Запланируйте восстановление и обратитесь за поддержкой, если такое состояние держится несколько дней или мешает учебе, работе, лечению.";
            }

            return prefix + "низкий ресурс. Этот тест не является диагнозом, но сейчас важно не оставаться одному: обратитесь к доверенному взрослому, врачу, психологу или в экстренную службу при угрозе безопасности.";
        }

        private string GetResultColor(double percentage)
        {
            if (percentage >= 80) return "#4CAF50";
            if (percentage >= 60) return "#8BC34A";
            if (percentage >= 40) return "#FFC107";
            if (percentage >= 20) return "#FF9800";

            return "#F44336";
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
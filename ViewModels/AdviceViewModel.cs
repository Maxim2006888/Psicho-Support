// ViewModels/AdviceViewModel.cs
using Psicho_Support.Core;
using Psicho_Support.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Psicho_Support.ViewModels
{
    public class AdviceViewModel : INotifyPropertyChanged
    {
        private readonly AppSession _session;
        private readonly UserStateService _stateService;
        private readonly AppState _appState;

        private string _adviceText;
        private string _stateLevel;
        private string _stateColor;
        private int _stateValue;
        private bool _isBusy;

        public AdviceViewModel(AppSession session, AppState appState, UserStateService stateService)
        {
            _session = session;
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _stateService = stateService;

            _stateService.StateChanged += OnStateChanged;

            UpdateAdvice();
        }

        public string AdviceText
        {
            get => _adviceText;
            set
            {
                _adviceText = value;
                OnPropertyChanged();
            }
        }

        public string StateLevel
        {
            get => _stateLevel;
            set
            {
                _stateLevel = value;
                OnPropertyChanged();
            }
        }

        public string StateColor
        {
            get => _stateColor;
            set
            {
                _stateColor = value;
                OnPropertyChanged();
            }
        }

        public int StateValue
        {
            get => _stateValue;
            set
            {
                _stateValue = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private void OnStateChanged(int newValue)
        {
            UpdateAdvice();
        }

        private async void UpdateAdvice()
        {
            if (!_appState.IsAuthenticated) return;

            IsBusy = true;

            await Task.Delay(100);

            StateValue = _stateService.CurrentValue;
            StateLevel = GetStateLevelName(_stateService.CurrentLevel);
            StateColor = GetStateLevelColor(_stateService.CurrentLevel);
            AdviceText = GetAdviceByState(_stateService.CurrentValue);

            IsBusy = false;
        }

        private string GetStateLevelName(Enums.UserStateLevel level)
        {
            switch (level)
            {
                case Enums.UserStateLevel.Critical:
                    return "Критическое состояние";
                case Enums.UserStateLevel.Low:
                    return "Пониженное состояние";
                case Enums.UserStateLevel.Stable:
                    return "Стабильное состояние";
                case Enums.UserStateLevel.Good:
                    return "Хорошее состояние";
                case Enums.UserStateLevel.Excellent:
                    return "Отличное состояние";
                default:
                    return "Не определено";
            }
        }

        private string GetStateLevelColor(Enums.UserStateLevel level)
        {
            switch (level)
            {
                case Enums.UserStateLevel.Critical:
                    return "#F44336";
                case Enums.UserStateLevel.Low:
                    return "#FF9800";
                case Enums.UserStateLevel.Stable:
                    return "#FFC107";
                case Enums.UserStateLevel.Good:
                    return "#8BC34A";
                case Enums.UserStateLevel.Excellent:
                    return "#4CAF50";
                default:
                    return "#AAAAAA";
            }
        }

        private string GetAdviceByState(int stateValue)
        {
            if (stateValue <= 20)
            {
                return "⚠️ Ваше эмоциональное состояние критическое.\n\n" +
                       "РЕКОМЕНДАЦИИ:\n" +
                       "1. НЕМЕДЛЕННО обратитесь к специалисту\n" +
                       "2. Позвоните на горячую линию психологической поддержки: 8-800-200-0-122\n" +
                       "3. Не оставайтесь одни, поговорите с близкими\n" +
                       "4. Сделайте 10 глубоких вдохов и выдохов\n" +
                       "5. Используйте технику заземления: назовите 5 предметов вокруг, 4 звука, 3 тактильных ощущения\n" +
                       "6. Временно уберите раздражающие факторы (громкую музыку, яркий свет)\n\n" +
                       "Помните: обратиться за помощью — это признак силы, а не слабости.";
            }
            else if (stateValue <= 40)
            {
                return "📉 Ваше эмоциональное состояние понижено.\n\n" +
                       "РЕКОМЕНДАЦИИ:\n" +
                       "• Запишите свои мысли и чувства в заметки — это поможет их структурировать\n" +
                       "• Сделайте 15-минутную прогулку на свежем воздухе\n" +
                       "• Послушайте спокойную музыку или звуки природы\n" +
                       "• Выпейте теплый травяной чай\n" +
                       "• Попробуйте упражнение '5-4-3-2-1':\n" +
                       "  - 5 вещей, которые вы видите\n" +
                       "  - 4 вещи, которые можете потрогать\n" +
                       "  - 3 звука, которые слышите\n" +
                       "  - 2 запаха, которые чувствуете\n" +
                       "  - 1 вкус, который ощущаете\n" +
                       "• Сократите время в социальных сетях";
            }
            else if (stateValue <= 60)
            {
                return "➖ Ваше эмоциональное состояние стабильно.\n\n" +
                       "РЕКОМЕНДАЦИИ ДЛЯ ПОДДЕРЖАНИЯ:\n" +
                       "• Поддерживайте регулярный режим сна (7-8 часов)\n" +
                       "• Практикуйте благодарность — запишите 3 хороших события дня\n" +
                       "• Общайтесь с близкими людьми\n" +
                       "• Найдите 30 минут для любимого хобби\n" +
                       "• Соблюдайте баланс работы и отдыха\n" +
                       "• Пейте достаточное количество воды\n" +
                       "• Легкая физическая активность (йога, ходьба, растяжка)\n\n" +
                       "Помните: стабильность — это результат вашей работы над собой!";
            }
            else if (stateValue <= 80)
            {
                return "📈 Ваше эмоциональное состояние хорошее!\n\n" +
                       "РЕКОМЕНДАЦИИ ДЛЯ УЛУЧШЕНИЯ:\n" +
                       "• Продолжайте вести заметки о позитивных событиях\n" +
                       "• Делитесь хорошим настроением с окружающими\n" +
                       "• Ставьте новые небольшие цели и достигайте их\n" +
                       "• Занимайтесь физической активностью 3-4 раза в неделю\n" +
                       "• Практикуйте медитацию для укрепления результата\n" +
                       "• Изучайте новые техники саморегуляции\n" +
                       "• Помогайте другим — это повышает уровень счастья\n\n" +
                       "Вы на правильном пути! Продолжайте заботиться о себе.";
            }
            else
            {
                return "🎉 Отличное эмоциональное состояние!\n\n" +
                       "РЕКОМЕНДАЦИИ ДЛЯ СОХРАНЕНИЯ:\n" +
                       "• Помогайте другим, делитесь своим позитивным опытом\n" +
                       "• Продолжайте заботиться о себе и своем психическом здоровье\n" +
                       "• Осваивайте новые навыки и увлечения\n" +
                       "• Ведите дневник успехов и достижений\n" +
                       "• Планируйте будущие цели и мечты\n" +
                       "• Будьте примером для окружающих\n" +
                       "• Не забывайте отдыхать и восстанавливаться\n\n" +
                       "Вы отлично справляетесь! Так держать! 🌟";
            }
        }

        public void Dispose()
        {
            if (_stateService != null)
            {
                _stateService.StateChanged -= OnStateChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
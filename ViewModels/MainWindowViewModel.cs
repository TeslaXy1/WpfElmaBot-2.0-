using System;
using System.Windows.Input;
using WpfElmaBot_2._0_.Infrastructure.Commands;
using WpfElmaBot_2._0_.View.Windows;
using WpfElmaBot_2._0_.ViewModels.Base;

namespace WpfElmaBot_2._0_.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        private static MainWindowViewModel instance;
        public static MainWindowViewModel getInstance()
        {
            if (instance == null)
                instance = new MainWindowViewModel();
            return instance;
        }

        #region Свойства

        #region Статус состояния

        private string _Status;
        /// <summary>
        /// состояние программы
        /// </summary>
        public string Status
        {
            get => _Status;
            set=>Set(ref _Status, value);   
        }
        #endregion

        #region Консоль 
        private string _Consol;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string Consol
        {
            get => _Consol;
            set => Set(ref _Consol, value);
        }
        #endregion

        #region Главная кнопка
        private bool _IsDefaultMain;
        /// <summary>
        /// свойство главной кнопке
        /// </summary>
        public bool IsDefaultMain
        {
            get => _IsDefaultMain;
            set => Set(ref _IsDefaultMain, value);
        }
        #endregion

        #region Кнопка настроек
        private bool _IsDefaultSetting;
        /// <summary>
        /// свойство  кнопки настроек
        /// </summary>
        public bool IsDefaultSetting
        {
            get => _IsDefaultSetting;
            set => Set(ref _IsDefaultSetting, value);
        }
        #endregion

        #region Кнопка неполадок
        private bool _IsDefaultError;
        /// <summary>
        /// свойство кнопки неполадок
        /// </summary>
        public bool IsDefaultError
        {
            get => _IsDefaultError;
            set => Set(ref _IsDefaultError, value);
        }
        #endregion

        #endregion

        #region Команды

        #region Команда главной кнопки
        public ICommand MainBtnCommand { get; set; }
        private void OnMainBtnCommandExecuted(object p)
        {
            IsDefaultMain = true;
            IsDefaultSetting = false;
            IsDefaultError = false;
        }
        private bool CanMainBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки настроек
        public ICommand SettingBtnCommand { get; set; }
        private void OnSettingBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
            IsDefaultSetting = true;
            IsDefaultError = false;
            SettingPageViewModel.getInstance().ShowDialog();
        }
        private bool CanSettingBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки ошибок
        public ICommand ErrorBtnCommand { get; set; }
        private void OnErrorBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
            IsDefaultSetting = false;
            IsDefaultError = true;
        }
        private bool CanErrorBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки выхода
        public ICommand CloseAppCommand { get; set; }
        private void OnCloseAppCommandExecuted(object p)
        {
            Environment.Exit(0);
        }
        private bool CanCloseAppCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            
            #region Команды
            MainBtnCommand = new LambdaCommand(OnMainBtnCommandExecuted, CanMainBtnCommandExecute);
            SettingBtnCommand = new LambdaCommand(OnSettingBtnCommandExecuted, CanSettingBtnCommandExecute);
            ErrorBtnCommand = new LambdaCommand(OnErrorBtnCommandExecuted, CanErrorBtnCommandExecute);
            CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
            #endregion

        }
    }
}

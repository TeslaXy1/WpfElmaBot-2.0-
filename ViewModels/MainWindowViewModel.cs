using NLog;
using System;
using System.Configuration;
using System.Windows.Input;
using WpfElmaBot.Service;
using WpfElmaBot_2._0_.Infrastructure.Commands;
using WpfElmaBot_2._0_.Service;
using WpfElmaBot_2._0_.View.Windows;
using WpfElmaBot_2._0_.ViewModels.Base;

namespace WpfElmaBot_2._0_.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {

        public static string Adress;
        public static string Port;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

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

        #region Видимость консоли
        private string _visibleConsol;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string VisibleConsol
        {
            get => _visibleConsol;
            set => Set(ref _visibleConsol, value);
        }
        #endregion

        #region Консоль неполадок 
        private string _error;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }
        #endregion

        #region Видимость консоли ошибок
        private string _visibleError = "Hidden";
        /// <summary>
        /// свойство консоли ошибок
        /// </summary>
        public string VisibleError
        {
            get => _visibleError;
            set => Set(ref _visibleError, value);
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
            VisibleConsol = "Visible";
            VisibleError = "Hidden";

        }
        private bool CanMainBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки настроек
        public ICommand SettingBtnCommand { get; set; }
        private void OnSettingBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
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
            VisibleConsol = "Hidden";
            VisibleError = "Visible";
        }
        private bool CanErrorBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки выхода
        public ICommand CloseAppCommand { get; set; }
        private void OnCloseAppCommandExecuted(object p)
        {
            object locker = new();
            lock (locker)
            {
                Environment.Exit(1);
            }
        }
        private bool CanCloseAppCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
           
            #region Команды
            MainBtnCommand               = new LambdaCommand(OnMainBtnCommandExecuted, CanMainBtnCommandExecute);
            SettingBtnCommand            = new LambdaCommand(OnSettingBtnCommandExecuted, CanSettingBtnCommandExecute);
            ErrorBtnCommand              = new LambdaCommand(OnErrorBtnCommandExecuted, CanErrorBtnCommandExecute);
            CloseAppCommand              = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
            #endregion


            ELMA.appToken                = ConfigurationManager.AppSettings.Get("TokenElma");
            ELMA.FullURL                 = ConfigurationManager.AppSettings.Get("FullURL"); ;
            ELMA.FullURLpublic           = ConfigurationManager.AppSettings.Get("FullURLPublic");
            ELMA.login                   = ConfigurationManager.AppSettings.Get("Login");
            ELMA.password                = ConfigurationManager.AppSettings.Get("Password");
            ELMA.TypeUid                 = ConfigurationManager.AppSettings.Get("TypeUid");
            TelegramCore.TelegramToken   = ConfigurationManager.AppSettings.Get("TokenTelegram");

            string[] AdressPort = ELMA.FullURL.Split('/');
            string [] adresport = AdressPort[2].Split(':');
            Adress = adresport[0];
            Port = adresport[1];

            Log.Debug($"\nБот запущен со следующими настройками:\nТокен Ельмы: {ELMA.appToken}\nТокен телеграма: {TelegramCore.TelegramToken}\nTypeUid справочника: {ELMA.TypeUid}\nЛогин: {ELMA.login}\nПароль: {ELMA.password}\n-----------------------------------------------------------");//TODO Порт и адрес
            new TelegramCore(this).Start();
            new  ElmaMessages(this).Start();

            

        }

    }
}

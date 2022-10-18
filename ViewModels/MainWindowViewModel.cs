using NLog;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfElmaBot.Service;
using WpfElmaBot.Service.Commands;
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

        #region  Свойство очистки консоли
        private bool _attachedPropertyClear;
        public bool AttachedPropertyClear
        {
            get { return _attachedPropertyClear; }
            set { _attachedPropertyClear = value; OnPropertyChanged(); }
        }
        #endregion

        #region  Свойство консоли
        private string _attachedPropertyAppend;
        public string AttachedPropertyAppend
        {
            get { return _attachedPropertyAppend; }
            set { _attachedPropertyAppend = value; OnPropertyChanged(); }
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

        #region  Свойство очистки консоли ошибок
        private bool _attachedPropertyClearError;
        public bool AttachedPropertyClearError
        {
            get { return _attachedPropertyClearError; }
            set { _attachedPropertyClearError = value; OnPropertyChanged(); }
        }
        #endregion

        #region  Свойство консоли ошибок
        private string _attachedPropertyAppendError;
        public string AttachedPropertyAppendError
        {
            get { return _attachedPropertyAppendError; }
            set { _attachedPropertyAppendError = value; OnPropertyChanged(); }
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
                Environment.Exit(0);
                
            }
        }
        private bool CanCloseAppCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            try
            {


                #region Команды
                MainBtnCommand = new LambdaCommand(OnMainBtnCommandExecuted, CanMainBtnCommandExecute);
                SettingBtnCommand = new LambdaCommand(OnSettingBtnCommandExecuted, CanSettingBtnCommandExecute);
                ErrorBtnCommand = new LambdaCommand(OnErrorBtnCommandExecuted, CanErrorBtnCommandExecute);
                CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
                #endregion


                ELMA.appToken = ConfigurationManager.AppSettings.Get("TokenElma");
                ELMA.FullURL = ConfigurationManager.AppSettings.Get("FullURL"); ;
                ELMA.FullURLpublic = ConfigurationManager.AppSettings.Get("FullURLPublic");
                ELMA.login = ConfigurationManager.AppSettings.Get("Login");
                ELMA.password = ConfigurationManager.AppSettings.Get("Password");
                ELMA.TypeUid = ConfigurationManager.AppSettings.Get("TypeUid");
                TelegramCore.TelegramToken = ConfigurationManager.AppSettings.Get("TokenTelegram");
                Common.IsPass = ConfigurationManager.AppSettings.Get("IsPass");


                

                string[] AdressPort = ELMA.FullURL.Split('/');
                string[] adresport = AdressPort[2].Split(':');
                Adress = adresport[0];
                Port = adresport[1];

                Log.Debug($"\nБот запущен со следующими настройками:\nТокен Ельмы: {ELMA.appToken}\nТокен телеграма: {TelegramCore.TelegramToken}\nTypeUid справочника: {ELMA.TypeUid}\nЛогин: {ELMA.login}\nПароль: {ELMA.password}\nАдрес: {Adress}\nПорт: {Port}\n-----------------------------------------------------------");
                new ElmaMessages(this).Start();
                new TelegramCore(this).Start();
                
            }
            catch(Exception ex)
            {
                Log.Error("Ошибка конструктора MainWindowViewModel | " + ex);
            }

            

        }

    }
}

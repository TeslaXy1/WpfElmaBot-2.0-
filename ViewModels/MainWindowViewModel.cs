using NLog;
using RestSharp;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
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
        private static RestClient RestClient { get; set; }
        private static readonly HttpClient client = new HttpClient();
        private static string authToken;
        private static string sessionToken;
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static string Adress;
        public static string Port;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static MainWindowViewModel instance;
        public static MainWindowViewModel getMainWindowVM()
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
            set => Set(ref _Status, value);
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

        #region Видимость настроек
        private string _visibleSettings;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string VisibleSettings
        {
            get => _visibleSettings;
            set => Set(ref _visibleSettings, value);
        }
        #endregion


        #region чекбокс проверки кавычек
        public bool IsPass { get; set; }

        #endregion

        #region Токен Elma

        private string _tokenElma;
        /// <summary>
        /// токен Elma
        /// </summary>
        public string TokenElma
        {
            get => _tokenElma;
            set => Set(ref _tokenElma, value);
        }
        #endregion

        #region Токен бота

        private string _tokenBot;
        /// <summary>
        ///токен бота
        /// </summary>
        public string TokenBot
        {
            get => _tokenBot;
            set => Set(ref _tokenBot, value);
        }
        #endregion

        #region Адрес

        private string _settingAdress;
        /// <summary>
        /// адресс
        /// </summary>
        public string SettingAdress
        {
            get => _settingAdress;
            set => Set(ref _settingAdress, value);
        }
        #endregion

        #region Порт

        private string _settingPort;
        /// <summary>
        /// порт
        /// </summary>
        public string SettingPort
        {
            get => _settingPort;
            set => Set(ref _settingPort, value);
        }
        #endregion

        #region TypeUid справочника

        private string _typeUid;
        /// <summary>
        /// Uid справочника
        /// </summary>
        public string TypeUid
        {
            get => _typeUid;
            set => Set(ref _typeUid, value);
        }
        #endregion

        #region Логин

        private string _login;
        /// <summary>
        /// логин
        /// </summary>
        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }
        #endregion

        #region Пароль

        private string _password;
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }
        #endregion

        #region Анимация загрузки

        private string _loading = "Hidden"; //Visible
        /// <summary>
        /// Анимация загрузки
        /// </summary>
        public string Loading
        {
            get => _loading;
            set => Set(ref _loading, value);
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

        #region кнопка свернуть

        private WindowState _WindowState;

        public WindowState WindowState
        {
            get => _WindowState;
            set => Set(ref _WindowState, value);
        }
        #endregion

        #region цвет неполадок
        private string _colorError = "Hidden";
        /// <summary>
        /// свойство кнопки ошибок
        /// </summary>
        public string ColorError
        {
            get => _colorError;
            set => Set(ref _colorError, value);
        }
        #endregion

        #region Видимость кнопки запустить

        private string _visibleStartBtn = "Hidden";
        /// <summary>
        /// логин
        /// </summary>
        public string VisibleStartBtn
        {
            get => _visibleStartBtn;
            set => Set(ref _visibleStartBtn, value);
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
            VisibleSettings = "Hidden";

        }
        private bool CanMainBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки настроек
        public ICommand SettingBtnCommand { get; set; }
        private void OnSettingBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
            IsDefaultError = false;
            IsDefaultSetting = true;

            VisibleConsol = "Hidden";
            VisibleError = "Hidden";
            VisibleSettings = "Visible";

            //SettingPageViewModel.getSettingPage().ShowDialog();
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
            ColorError = "Hidden";
            VisibleSettings = "Hidden";
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

        #region Команда кнопки запустить
        public ICommand StartBtnCommand { get; set; }
        private void OnStartBtnCommandExecuted(object p)
        {
            //TelegramCore.getTelegramCore().RefreshTelegramCore();
            //StartTelegram();
            new ElmaMessages().Start();
            VisibleStartBtn = "Hidden";
        }
        private bool CanStartBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки остановить
        public ICommand StopBtnCommand { get; set; }
        private void OnStopBtnCommandExecuted(object p)
        {
            ElmaMessages.Stop();
           


        }
        private bool CanStopBtnCommandExecute(object p) => true;
        #endregion

        #region команда кнопки свернуть

        public ICommand RollUpCommand { get; set; }


        private void OnRollUpCommandExecuted(object p)
        {

            WindowState = WindowState.Minimized;


        }
        private bool CanRollUpCommandExecute(object p) => true;

        #endregion

        #region Команда кнопки сохранения 
        public ICommand SaveSettingsCommand { get; set; }
        private void OnSaveSettingsCommandExecuted(object p)
        {
            try
            {
                if (TokenBot != null && TokenElma != null && SettingAdress != null && SettingPort != null && TypeUid != null && Login != null && Password != null)
                {

                    Loading = "Visible";
                    Task.Run(() => CheckSetting());

                }
                else
                {
                    MessageBox.Show("Заполните все поля");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Что-то пошло не так.\nПопробуйте ещё раз.");
                Log.Error("Ошибка запуска потока проверки настроек | " + ex);
            }
        }
        private bool CanSaveSettingsCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            try
            {

                // new SettingPage().Show();

                WindowState = WindowState.Normal;
                IsDefaultMain = true;
                VisibleSettings = "Hidden";

                #region Команды
                MainBtnCommand = new LambdaCommand(OnMainBtnCommandExecuted, CanMainBtnCommandExecute);
                SettingBtnCommand = new LambdaCommand(OnSettingBtnCommandExecuted, CanSettingBtnCommandExecute);
                ErrorBtnCommand = new LambdaCommand(OnErrorBtnCommandExecuted, CanErrorBtnCommandExecute);
                CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
                RollUpCommand = new LambdaCommand(OnRollUpCommandExecuted, CanRollUpCommandExecute);
                StartBtnCommand = new LambdaCommand(OnStartBtnCommandExecuted, CanStartBtnCommandExecute);
                StopBtnCommand = new LambdaCommand(OnStopBtnCommandExecuted, CanStopBtnCommandExecute);
                SaveSettingsCommand = new LambdaCommand(OnSaveSettingsCommandExecuted, CanSaveSettingsCommandExecute);

                #endregion



                #region загрузка данных из конфига
                ELMA.appToken = ConfigurationManager.AppSettings.Get("TokenElma");
                ELMA.FullURL = ConfigurationManager.AppSettings.Get("FullURL"); ;
                ELMA.FullURLpublic = ConfigurationManager.AppSettings.Get("FullURLPublic");
                ELMA.login = ConfigurationManager.AppSettings.Get("Login");
                ELMA.password = ConfigurationManager.AppSettings.Get("Password");
                ELMA.TypeUid = ConfigurationManager.AppSettings.Get("TypeUid");
                TelegramCore.TelegramToken = ConfigurationManager.AppSettings.Get("TokenTelegram");
                Common.IsPass = ConfigurationManager.AppSettings.Get("IsPass");
                #endregion

                if (ELMA.FullURL != "")
                {
                    string[] AdressPort = ELMA.FullURL.Split('/');
                    string[] adresport = AdressPort[2].Split(':');
                    Adress = adresport[0];
                    Port = adresport[1];

                    TokenElma = ELMA.appToken;
                    TokenBot = TelegramCore.TelegramToken;
                    SettingAdress = Adress;
                    SettingPort = Port;
                    TypeUid = ELMA.TypeUid;
                    Login = ELMA.login;
                    Password = ELMA.password;

                    if (Common.IsPass == "true")
                    {
                        IsPass = true;
                    }
                    else { IsPass = false; }
                }
                else
                {
                    MessageBox.Show("Проверьте настройки!");
                    Telegram_OnCommonError("Проверьте настройки!", TelegramCore.TelegramEvents.Status);
                }


                Log.Debug($"\nБот запущен со следующими настройками:\nТокен Ельмы: {ELMA.appToken}\nТокен телеграма: {TelegramCore.TelegramToken}\nTypeUid справочника: {ELMA.TypeUid}\nЛогин: {ELMA.login}\nПароль: {ELMA.password}\nАдрес: {Adress}\nПорт: {Port}\n-----------------------------------------------------------");
                Task.Run(() => new ElmaMessages().Start());
                //new ElmaMessages().Start();

                StartTelegram();

                AttachedPropertyAppend = "Здесь будут отображаться сообщения из телеграма\n" + Environment.NewLine;
                AttachedPropertyAppendError = "Здесь будут отображаться неполадки в работе программы\n" + Environment.NewLine;


            }
            catch (Exception ex)
            {
                Log.Error("Ошибка конструктора MainWindowViewModel | " + ex);
            }



        }


        private void StartTelegram()
        {
            var telegram = TelegramCore.getTelegramCore();
            telegram.Start();
            telegram.OnCommonLog += Telegram_OnCommonLog;
            telegram.OnCommonError += Telegram_OnCommonError;
            telegram.OnCommonStatus += Telegram_OnCommonStatus;
            //telegram.OnColorError += Telegram_OnColorError;
        }


        private void Telegram_OnCommonStatus(string message, TelegramCore.TelegramEvents events)
        {
            if (message == "Обработка сообщений остановлена")
            {
                VisibleStartBtn = "Visible";
                Status = $"{DateTime.Now.ToString("g")} - {message}";
            }
            else
            {
                Status = $"{DateTime.Now.ToString("g")} - {message}";
            }

        }

        private void Telegram_OnCommonError(string message, TelegramCore.TelegramEvents events)
        {
            AttachedPropertyAppendError = $"{DateTime.Now.ToString("G")}: {message} \n" + Environment.NewLine;
            ColorError = "Visible";
        }

        private void Telegram_OnCommonLog(string message, TelegramCore.TelegramEvents events)
        {

            AttachedPropertyAppend = $"{DateTime.Now.ToString("G")}: {message} \n" + Environment.NewLine;

        }

        private async void CheckSetting()
        {
            try
            {
                bool botToken = SettingPageViewModel.CheckTokenBot(TokenBot);//проверка токена бота
                if (botToken == true)
                {
                    bool adresPort = SettingPageViewModel.CheckAdresPort(SettingAdress, SettingPort);//проверка адреса и порта
                    if (adresPort == true)
                    {
                        bool LoginAndTokenElmma = SettingPageViewModel.CheckTokenElmaandLoginPas(SettingAdress, SettingPort, Login, TokenElma, Password, IsPass);//проверка токена Ельмы, логина и пароля
                        if (LoginAndTokenElmma == true)
                        {
                            bool IsTypeUid = SettingPageViewModel.CheckEnt(SettingAdress, SettingPort, TypeUid);//проверка TypeUid справочника                          
                            if (IsTypeUid == true)
                            {
                                if (IsPass == false)
                                {
                                    config.AppSettings.Settings["IsPass"].Value = "false";
                                }
                                else { config.AppSettings.Settings["IsPass"].Value = "true"; }

                                ConfigurationManager.RefreshSection("appSettings");
                                SettingPageViewModel.config.Save(ConfigurationSaveMode.Modified);
                                Loading = "Hidden";
                                MessageBox.Show("Успешно.Настройка завершена. Для применения настроек перезапустите программу.");

                            }

                            else { Loading = "Hidden"; MessageBox.Show("Неверный Uid справочника.\nНастройка не завершена"); }
                        }
                        else { Loading = "Hidden"; MessageBox.Show("Неверный токен Elma или логин с паролем.\nНастройка не завершена"); }
                    }
                    else { Loading = "Hidden"; MessageBox.Show("Неверный адрес или порт.\nНастройка не завершена"); }
                }
                else { Loading = "Hidden"; MessageBox.Show("Неверный токен бота.\nНастройка не завершена"); }
            }
            catch (Exception ex)
            {
                Loading = "Hidden";
                MessageBox.Show("Что-то пошло не так. Попробуйте еще раз.");
                MainWindowViewModel.Log.Error("Ошибка проверки настроек | " + ex);

            }
        }


    }
}

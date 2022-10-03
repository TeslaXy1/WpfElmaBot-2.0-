using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telegram.Bot;
using WpfElmaBot_2._0_.Infrastructure.Commands;
using WpfElmaBot_2._0_.Service;
using WpfElmaBot_2._0_.View.Windows;
using WpfElmaBot_2._0_.ViewModels.Base;
using WpfElmaBot.Service;
using DataFormat = RestSharp.DataFormat;
using System.IO;
using WpfElmaBot_2._0_.Models.EntityPack;
using System.Configuration;

namespace WpfElmaBot_2._0_.ViewModels
{
    internal class SettingPageViewModel : ViewModel
    {
        private static RestClient RestClient { get; set; }
        private static string authToken;
        private static string sessionToken;
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


        private static SettingPage instance;
        public static SettingPage getInstance()
        {
            if (instance == null)
                instance = new SettingPage();
            return instance;
        }
        #region Свойства

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

        private string _adress;
        /// <summary>
        /// адресс
        /// </summary>
        public string Adress
        {
            get => _adress;
            set => Set(ref _adress, value);
        }
        #endregion

        #region Порт

        private string _port;
        /// <summary>
        /// порт
        /// </summary>
        public string Port
        {
            get => _port;
            set => Set(ref _port, value);
        }
        #endregion

        #region TypeUid справочника

        private string _typeUid ;
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

        private string _password ;
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

        #endregion

        #region Команды

        #region Команда кнопки сохранения 
        public ICommand SaveSettingsCommand { get; set; }
        private void OnSaveSettingsCommandExecuted(object p)
        {
            try
            {
                if (TokenBot != null && TokenElma != null && Adress != null && Port != null && TypeUid != null && Login != null && Password != null)
                {

                    Loading = "Visible";
                    Task.Run(() => CheckSetting());

                }
                else
                {
                    MessageBox.Show("Заполните все поля");
                }
            }
            catch { }
        }
        private bool CanSaveSettingsCommandExecute(object p) => true;
        #endregion

        #region Команда перехода на предыдущию 
        public ICommand BackCommand { get; set; }
        private void OnBackCommandExecuted(object p)
        {
            
            getInstance().Hide();

        }
        private bool CanBackCommandExecute(object p) => true;
        #endregion

        #endregion

        public SettingPageViewModel()
        {
            #region Команды
            SaveSettingsCommand = new LambdaCommand(OnSaveSettingsCommandExecuted, CanSaveSettingsCommandExecute);
            BackCommand = new LambdaCommand(OnBackCommandExecuted, CanBackCommandExecute);
            #endregion

            TokenElma = ELMA.appToken;
            TokenBot = TelegramCore.TelegramToken;
            Adress = MainWindowViewModel.Adress;
            Port = MainWindowViewModel.Port;
            TypeUid = ELMA.TypeUid;
            Login = ELMA.login;
            Password = ELMA.password;


            RestClient = new RestClient();
        }

        #region Функции проверки настроек

        private void CheckSetting()
        {
            try
            {
                bool botToken = CheckTokenBot();
                if (botToken == true)
                {
                    bool adresPort = CheckAdresPort();
                    if (adresPort == true)
                    {
                        bool LoginAndTokenElmma = CheckTokenElmaandLoginPas();
                        if (LoginAndTokenElmma == true)
                        {
                            bool sprav = CheckEntity();
                            if (sprav == true)
                            {
                                TelegramCore.TelegramToken = TokenBot;
                                ELMA.FullURL = $"http://{Adress}:{Port}/API/REST/";
                                ELMA.appToken = TokenElma;
                                ELMA.TypeUid = TypeUid;
                                ELMA.login = Login;
                                ELMA.password = Password;
                                ELMA.FullURLpublic = $"http://{Adress}:{Port}/PublicAPI/REST/";
                                //TODO записать конфиг

                                config.AppSettings.Settings["Login"].Value = Login;
                                config.AppSettings.Settings["Password"].Value = Password;
                                config.AppSettings.Settings["TokenElma"].Value = TokenElma;
                                config.AppSettings.Settings["TokenTelegram"].Value = TokenBot;
                                config.AppSettings.Settings["FullURL"].Value = $"http://{Adress}:{Port}/API/REST/";
                                config.AppSettings.Settings["FullURLPublic"].Value = $"http://{Adress}:{Port}/PublicAPI/REST/";
                                config.AppSettings.Settings["TypeUid"].Value = TypeUid;
                                ConfigurationManager.RefreshSection("appSettings");
                                config.Save(ConfigurationSaveMode.Modified);

                                Loading = "Hidden";
                                //ElmaMessages.Start();
                                MessageBox.Show("Успешно.Настройка завершена");
                            }
                            else { Loading = "Hidden"; MessageBox.Show("Неверный Uid справочника. Настройка не завершена"); }
                        }
                        else { Loading = "Hidden"; MessageBox.Show("Неверный токен Elma или логин с паролем. Настройка не завершена"); }
                    }
                    else { Loading = "Hidden"; MessageBox.Show("Неверный адрес или порт. Настройка не завершена"); }
                }
                else { Loading = "Hidden"; MessageBox.Show("Неверный токен бота. Настройка не завершена"); }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка проверки настроек | " + ex);
                Loading = "Hidden"; //TODO обработка ошибок }
            }
        }

        private bool CheckTokenBot()
        {
            try
            {
                ITelegramBotClient bot = new TelegramBotClient(TokenBot);
                bot.GetMeAsync().Wait();

                //TODO присвоить значение токена к переменой
                return true;
            }
            catch { return false; }

        }

        private bool CheckTokenElmaandLoginPas()
        {

            try
            {
                HttpWebRequest req = WebRequest.Create(String.Format($"http://{Adress}:{Port}/API/REST/Authorization/LoginWith?username={Login}")) as HttpWebRequest;
                req.Headers.Add("ApplicationToken", TokenElma);
                req.Method = "POST";
                req.Timeout = 10000;
                req.ContentType = "application/json; charset=utf-8";

                //данные для отправки. используется для передачи пароля. пароль нужно записать вместо пустой строки
                var sentData = Encoding.UTF8.GetBytes(Password);
                req.ContentLength = sentData.Length;
                Stream sendStream = req.GetRequestStream();
                sendStream.Write(sentData, 0, sentData.Length);

                //получение ответа
                var res = req.GetResponse() as HttpWebResponse;
                var resStream = res.GetResponseStream();
                var sr = new StreamReader(resStream, Encoding.UTF8);

                var dict = new JsonSerializer().Deserialize(sr, typeof(Dictionary<string, string>)) as Dictionary<string, string>;
                authToken = dict["AuthToken"];
                sessionToken = dict["SessionToken"];

                return true;
            }
            catch { return false;  }
        }

        private bool CheckAdresPort()
        {
            try
            {
                WebRequest request = WebRequest.Create($"http://{Adress}:{Port}/");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    // failed
                    
                    response.Close();
                    return false;
                }
                else
                {

                    //TODO присвоить значения переменным

                    response.Close();
                    return true;
                }
            }
            catch { return false; };


        }

        private bool CheckEntity()
        {
            try
            {
                var entity = new ELMA().GetEntity<EntityMargin>($"Entity/Query?type={TypeUid}", authToken, sessionToken);
                return true;
            }
            catch
            {
                return false;
            }
            
        }
        #endregion
    }
}

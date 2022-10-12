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
using WpfElmaBot.Service.Commands;
using System.Net.Http;

namespace WpfElmaBot_2._0_.ViewModels
{
    internal class SettingPageViewModel : ViewModel
    {
        private static RestClient RestClient { get; set; }
        private static readonly HttpClient client = new HttpClient();
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
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка запуска потока проверки настроек | " + ex);
            }
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
            try
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
                if (Common.IsPass == "true")
                {
                    IsPass = true;
                }
                else { IsPass = false; }

                RestClient = new RestClient();
            }
            catch(Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка конструкта SettingPageViewModel | " + ex);
            }
        }

        #region Функции проверки настроек

        private async void  CheckSetting()
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
                            bool IsTypeUid = CheckEnt();                          
                            if (IsTypeUid == true)
                            {
                                if (IsPass == true)
                                {
                                    config.AppSettings.Settings["IsPass"].Value = "true";
                                }
                                else { config.AppSettings.Settings["IsPass"].Value = "false"; }

                                ConfigurationManager.RefreshSection("appSettings");
                                config.Save(ConfigurationSaveMode.Modified);
                                Loading = "Hidden";
                                //ElmaMessages.Start();
                                MessageBox.Show("Успешно.Настройка завершена. Для применения настроек перезапустите программу.");
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
                Loading = "Hidden";
                MessageBox.Show("Что-то пошло не так. Попробуйте еще раз.");
                MainWindowViewModel.Log.Error("Ошибка проверки настроек | " + ex);
                
            }
        }

        private bool CheckTokenBot()
        {
            try
            {
                ITelegramBotClient bot = new TelegramBotClient(TokenBot);
                bot.GetMeAsync().Wait();
                TelegramCore.TelegramToken = TokenBot;
                config.AppSettings.Settings["TokenTelegram"].Value = TokenBot;
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);

                return true;
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка проверки бота | " + ex);
                return false; 
            }

        }

        private bool CheckTokenElmaandLoginPas()
        {
            byte [] sentData = new byte[5000];
            try
            {

                HttpWebRequest req = WebRequest.Create(String.Format($"http://{Adress}:{Port}/API/REST/Authorization/LoginWith?username={Login}")) as HttpWebRequest;
                req.Headers.Add("ApplicationToken", TokenElma);
                req.Method = "POST";
                req.Timeout = 10000;
                req.ContentType = "application/json; charset=utf-8";

                //данные для отправки. используется для передачи пароля. пароль нужно записать вместо пустой строки
                if(IsPass == false)
                {
                    sentData = Encoding.UTF8.GetBytes(Password);
                }
                else 
                {
                    sentData = Encoding.UTF8.GetBytes($@"""{Password}"""); 
                }
                
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


                
                ELMA.appToken = TokenElma;
                ELMA.login = Login;
                ELMA.password = Password;
                


                config.AppSettings.Settings["Login"].Value = Login;
                config.AppSettings.Settings["Password"].Value = Password;
                config.AppSettings.Settings["TokenElma"].Value = TokenElma;
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);

                return true;
            }
            catch (Exception ex)   
            {
                MainWindowViewModel.Log.Error("Ошибка проверки токена Elma, логина с паролем | " + ex);
                return false;  
            }
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

                    ELMA.FullURL = $"http://{Adress}:{Port}/API/REST/";
                    ELMA.FullURLpublic = $"http://{Adress}:{Port}/PublicAPI/REST/";
                    config.AppSettings.Settings["FullURL"].Value = $"http://{Adress}:{Port}/API/REST/";
                    config.AppSettings.Settings["FullURLPublic"].Value = $"http://{Adress}:{Port}/PublicAPI/REST/";
                    ConfigurationManager.RefreshSection("appSettings");
                    config.Save(ConfigurationSaveMode.Modified);

                    response.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка проверки адреса | " + ex);
                return false; 
            }


        }

        private bool CheckEnt()
        {

            try
            {
                HttpWebRequest taskReq1 = (HttpWebRequest)WebRequest.Create(String.Format($"http://{Adress}:{Port}/API/REST/Entity/Query?type={TypeUid}"));
                taskReq1.Method = "GET";
                taskReq1.Headers.Add("AuthToken", authToken);
                taskReq1.Headers.Add("SessionToken", sessionToken);
                taskReq1.Timeout = 15000;
                taskReq1.ContentType = "application/json; charset=utf-8";


                var res2 = taskReq1.GetResponse() as HttpWebResponse;
                var resStream2 = res2.GetResponseStream();
                var sr2 = new StreamReader(resStream2, Encoding.UTF8);

                ELMA.TypeUid = TypeUid;
                config.AppSettings.Settings["TypeUid"].Value = TypeUid;
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);
                return true;

            }
            catch
            {
                return false;
            }
            

        }

        private bool  CheckEntity()
        {
            try
            {
                var entity = ELMA.getInstance().GetEntityById<EntityMargin>(TypeUid,0,authToken,sessionToken);
                
                

                ELMA.TypeUid = TypeUid;
                config.AppSettings.Settings["TypeUid"].Value = TypeUid;
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);

                return true;
            }
            catch(Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка проверки справочника | " + ex);
                return false;
            }
            
        }
        #endregion
    }
}

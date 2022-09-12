using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfElmaBot_2._0_.ViewModels.Base;

namespace WpfElmaBot_2._0_.ViewModels
{
    internal class SettingPageViewModel : ViewModel
    {
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

        #endregion
    }
}

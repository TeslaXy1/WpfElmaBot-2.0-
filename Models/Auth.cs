using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfElmaBot.Models
{
    public class Auth
    {
        public string AuthToken { get; set; }
        public string CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }
        public string Lang { get; set; }
        public string SessionToken { get; set; }
    }
}

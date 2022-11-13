using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfElmaBot.Models;

namespace WpfElmaBot_2._0_.Models.EntityPack
{
    public class EntityMargin: Entity
    {
        public string IdTelegram { get; set; }
        public string IdUserElma { get; set; }
        public string AuthToken { get; set; }
        public string SessionToken { get; set; }
        public string IdLastSms { get; set; }
        public string AuthorizationUser { get; set; }
        public string Login { get; set; }

        public DateTime TimeMessage { get; set; }
    }
}

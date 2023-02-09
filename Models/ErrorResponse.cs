using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfElmaBot_2._0_.Models
{
    public class ErrorResponse
    {
        public object InnerException { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}

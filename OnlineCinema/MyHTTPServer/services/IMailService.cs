using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Services
{
    public interface IMailService
    {
        Task SendAsync(string email, string message, string path);
    }
}

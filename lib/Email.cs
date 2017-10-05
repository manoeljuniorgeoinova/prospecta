using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Net.Mail;

namespace resultys.prospecta.lib
{
    public class Email
    {
        public string assunto { get; set; }
        public string mensagem { get; set; }
        public string[] destinatarios { get; set; }

        public void send()
        {
            if (this.destinatarios == null)
            {
                this.destinatarios = Config.read("Email", "destinatarios").Split(',');
            }

            var th = new Thread(asynSendEmail);
            th.Start(this);
        }

        private void asynSendEmail(object parameter)
        {
            var email = parameter as Email;

            this.send(email.assunto, email.mensagem);
        }

        public void send(string assunto, string mensagem)
        {
            foreach (var to in this.destinatarios)
            {
                Email.send(to, assunto, mensagem);
            }
        }

        public static void send(string destinatario, string assunto, string mensagem)
        {
            var fromAddress = new MailAddress(Config.read("Email", "email"), Config.read("Email", "name"));
            var toAddress = new MailAddress(destinatario, destinatario);
            string subject = assunto;
            string body = mensagem;

            var smtp = new SmtpClient
            {
                Host = Config.read("Email", "host"),
                Port = int.Parse(Config.read("Email", "port")),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, Config.read("Email", "senha"))
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                IsBodyHtml = true,
                Subject = subject,
                Body = body
            })
            {
                try
                {
                    smtp.Send(message);
                }
                catch(Exception e) {
                    Log.WriteLine(e.Message);
                }
            }
        }

        //public static void send(string assunto, string mensagem)
        //{
        //    var tos = Config.read("Email", "destinatarios").Split(',');

        //    foreach (var to in tos)
        //    {
        //        var email = new Pillar.Util.SendEmail(
        //            Config.read("Email", "host"),
        //            Config.read("Email", "port"),
        //            Config.read("Email", "email"),
        //            Config.read("Email", "senha")
        //        );

        //        email.Send(
        //            Config.read("Email", "name"),
        //            Config.read("Email", "email"),
        //            to, 
        //            assunto, 
        //            mensagem, 
        //            null, 
        //            true
        //        );
        //    }
        //}

    }
}

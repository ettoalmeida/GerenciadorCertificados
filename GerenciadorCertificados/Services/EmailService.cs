using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http.Headers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Configuration;

namespace GerenciadorCertificados.Services
{
    public class EmailService
    {
        /// <summary>
        /// Método criado para enviar o e-mail com o certificado em .PDF em anexo
        /// </summary>
        /// <param name="EmailDestinatario">E-mail do Destinatário</param>
        /// <param name="NomeDestinatario">Nome do Destinatário</param>
        /// <param name="Certificado">Array de bytes do arquivo .PDF do Certificado</param>
        /// <returns></returns>
        public async Task<HttpStatusCode> EnviarEmailAsync(string EmailDestinatario, string NomeDestinatario, byte[] Certificado)
        {
            //Configurar dados do E-mail
            var sendMessage = new SendGridMessage();
            sendMessage.AddTo(EmailDestinatario, NomeDestinatario);
            //sendMessage.From = new EmailAddress();            
            sendMessage.SetFrom("falecoma@even3.com.br", "Suporte Even3");
            sendMessage.SetSubject("Certificado Disponível");
            sendMessage.AddContent(MimeType.Html, "<strong>Parabéns!</strong>Você conquistou um novo certificado!");
            var file = Convert.ToBase64String(Certificado);
            sendMessage.AddAttachment("Certificado-"+NomeDestinatario+".pdf", file);

            //(Configurei uma própria ApiKey e coloquei no Web.Config para agilizar os testes de vocês)
            var apiKey = ConfigurationManager.AppSettings["SendGridAPIKey"];

            var sendClient = new SendGridClient(apiKey);
            var response = await sendClient.SendEmailAsync(sendMessage);

            return response.StatusCode;
        }
    }
}
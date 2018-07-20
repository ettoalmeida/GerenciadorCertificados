using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using GerenciadorCertificados.Models;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Adapters;
using GerenciadorCertificados.Services;

namespace GerenciadorCertificados.Controllers
{
    public class HomeController : Controller
    {
        private EmailService EmailService;

        public HomeController()
        {
            this.EmailService = new EmailService();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> EnviarCertificados(string Participantes, string Texto, string Background, string Html)
        {
            try
            {
                JsonResult retorno = new JsonResult();

                //Verificar injeção de script, pois validação foi desativada para receber parâmetros em HTML
                if (Server.HtmlEncode(Html).Contains("script"))
                    throw new HttpRequestValidationException();

                if (Background.Equals("none"))
                    retorno.Data = new { Sucesso = false, Mensagem = "Vamos fazer um certificado tão bom quanto o Evento. Insira uma imagem de fundo para deixá-lo ainda melhor. Confere nossa Dica lá em cima." };
                else
                {
                    if (string.IsNullOrEmpty(Participantes))
                        retorno.Data = new { Sucesso = false, Mensagem = "Você esqueceu de importar os participantes!" };
                    else
                    {
                        //Separar informações dos Participantes
                        string[] linhas = Participantes.Split('\n');
                        string[] tags = linhas[0].Split('\t');

                        //Verificar se existe alguma tag inválida
                        string tagsInvalidas = ExisteTagInvalida(Texto, tags);
                        if (!string.IsNullOrEmpty(tagsInvalidas))
                            retorno.Data = new { Sucesso = false, Mensagem = tagsInvalidas };
                        else
                        {
                            //Tratar a string deixando o conteúdo em base64
                            Background = Background.Split(',')[1];
                            Background = Background.Substring(0, Background.Length - 2);

                            Html = TransformarTamanhoTexto(Html); //Transformar tags de Texto do Html

                            //Filtrar tags utilizadas
                            List<Tag> tagsUtilizadas = new List<Tag>();
                            for (int i = 0; i < tags.Length; ++i)
                            {
                                string tagComChaves = "{{" + tags[i] + "}}";
                                if (Texto.Contains(tagComChaves))
                                    tagsUtilizadas.Add(new Tag { Indice = i, Nome = tagComChaves });
                            }

                            string emailsSucesso = "";
                            string emailsFalha = "";

                            //Iterar sobre lista de participantes gerando pra cada o .PDF do certificado e enviando por email
                            for (int i = 1; i < linhas.Length; ++i) //Pula o primeiro item que é o cabeçalho
                            {
                                string modeloTexto = Html;
                                string[] dadosParticipante = linhas[i].Split('\t');

                                //Substituir tags pelos dados do Participante
                                foreach (Tag tag in tagsUtilizadas)
                                {
                                    modeloTexto = modeloTexto.Replace(tag.Nome, dadosParticipante[tag.Indice]);
                                }

                                var bytesImg = Convert.FromBase64String(Background); //Obter array de bytes da imagem de fundo
                                var bytesPdf = GerarPdfPeloHtml(modeloTexto, bytesImg); //Obter array de bytes do PDF do Certificado

                                HttpStatusCode statusCode = await this.EmailService.EnviarEmailAsync(dadosParticipante[3], dadosParticipante[0], bytesPdf);

                                if (statusCode == HttpStatusCode.Accepted)
                                    emailsSucesso = emailsSucesso + dadosParticipante[3] + " | ";
                                else
                                    emailsFalha = emailsFalha + dadosParticipante[3] + " | ";
                            }

                            //Tratamento de mensagens de retorno do envio dos Certificados
                            if (linhas.Length == 1) //So há uma linha no quadro de Participantes
                            {
                                retorno.Data = new { Sucesso = false, Mensagem = "Você esqueceu de importar os Participantes!" };
                            }
                            else if (string.IsNullOrEmpty(emailsFalha)) //Operação realizada com sucesso
                            {
                                emailsSucesso = emailsSucesso.Substring(0, emailsSucesso.Length - 3);
                                retorno.Data = new { Sucesso = false, Mensagem = "Mandamos com sucesso os certificados para o(s) email(s): " + emailsSucesso };
                            }
                            else if (string.IsNullOrEmpty(emailsSucesso)) //Nenhum e-mail foi enviado
                            {
                                retorno.Data = new { Sucesso = false, Mensagem = "Não conseguimos mandar os Certificados. Verifique os emails dos Participantes e tente novamente!"};
                            }
                            else //Enviaram alguns e falharam outros
                            {
                                emailsSucesso = emailsSucesso.Substring(0, emailsSucesso.Length - 3);
                                emailsFalha = emailsFalha.Substring(0, emailsFalha.Length - 3);
                                retorno.Data = new { Sucesso = false, Mensagem = "Mandamos com sucesso os certificados para o(s) email(s): " + emailsSucesso + ". Porém, não conseguimos enviar para o(s) email(s): " + emailsFalha};
                            }
                        }
                    }
                }

                return retorno;
            }
            catch (HttpRequestValidationException e)
            {
                return new JsonResult { Data = new { Sucesso = false, Mensagem = "Ops! Não podemos aceitar este modelo de Certificado." } };
            }
            catch (Exception e)
            {
                return new JsonResult { Data = new { Sucesso = false, Mensagem = "Ops! Não conseguimos enviar os Certificados. Verifique os dados e tente novamente!" } };
            }
        }

        /// <summary>
        /// Método criado para gerar o arquivo .PDF a partir do Html do Certificado
        /// </summary>
        /// <param name="Html">Html do Certificado</param>
        /// <returns>A representação em array de bytes do arquivo .PDF do Certificado</returns>
        private byte[] GerarPdfPeloHtml(string Html, byte[] img)
        {
            Byte[] pdfResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (MemoryStream msImg = new MemoryStream(img))
                {
                    //Configurar o arquivo .PDF do certificado
                    var config = new PdfGenerateConfig();
                    config.PageOrientation = PdfSharp.PageOrientation.Landscape;
                    config.PageSize = PdfSharp.PageSize.A4;
                    System.Drawing.Image image = System.Drawing.Image.FromStream(msImg);
                    PdfSharp.Drawing.XImage ximage = PdfSharp.Drawing.XImage.FromGdiPlusImage(image);
                    Html = "<br><br>" + Html; //Compensar os espaços do início que são omitidos pela biblioteca

                    //Gerar o arquivo inserindo o Html e a Imagem de fundo
                    var pdf = PdfGenerator.GeneratePdf(Html, config);
                    PdfSharp.Drawing.XGraphics xgraph = PdfSharp.Drawing.XGraphics.FromPdfPage(pdf.Pages[0], PdfSharp.Drawing.XGraphicsPdfPageOptions.Prepend);
                    xgraph.DrawImage(ximage, 0, 0, pdf.Pages[0].Width, pdf.Pages[0].Height); 
                    pdf.Save(ms);
                    pdfResult = ms.ToArray();
                }
            }
            return pdfResult;
        }

        /// <summary>
        /// Método criado para transformar o atributo 'size' não reconhecido pela biblioteca que renderiza o PDF
        /// em style 'font-size' com valor correspondente.
        /// </summary>
        /// <param name="Html">Html Genérico do Certificado</param>
        /// <returns>Html transformado do Certificado</returns>
        private string TransformarTamanhoTexto(string Html)
        {
            Html = Html.Replace("font size=\"1\"", "font style='font-size:10px'");
            Html = Html.Replace("font size=\"2\"", "font style='font-size:13px'");
            Html = Html.Replace("font size=\"3\"", "font style='font-size:16px'");
            Html = Html.Replace("font size=\"4\"", "font style='font-size:18px'");
            Html = Html.Replace("font size=\"5\"", "font style='font-size:24px'");
            Html = Html.Replace("font size=\"6\"", "font style='font-size:32px'");
            Html = Html.Replace("font size=\"7\"", "font style='font-size:48px'");

            return Html;
        }

        /// <summary>
        /// Método criado para verificar as tags que não possuem um cabeçalho correspondente.
        /// </summary>
        /// <param name="TextoCertificado">Texto do Certificado</param>
        /// <param name="TagsValidas">String com todos os cabeçalhos</param>
        /// <returns>Vazio, se sucesso. Lista de tags, se existir tags inválidas.</returns>
        private string ExisteTagInvalida(string TextoCertificado, string[] TagsValidas)
        {
            string retorno = "";
            string[] abreChaves = { "{{" };
            string[] fechaChaves = { "}}" };
            string[] separaAbreChaves = TextoCertificado.Split(abreChaves, StringSplitOptions.None);

            for(int i = 1; i < separaAbreChaves.Length; ++i) //Ignora o primeiro item por não possuir tag
            {
                string[] separaFechaChaves = separaAbreChaves[i].Split(fechaChaves, StringSplitOptions.None);
                if (TagsValidas.FirstOrDefault(x => x == separaFechaChaves[0]) == null)
                    retorno = retorno + separaFechaChaves[0] + " | ";
            }

            if (!string.IsNullOrEmpty(retorno))
            {
                retorno = retorno.Substring(0, retorno.Length - 3);
                retorno = "Não identificamos o(s) cabeçalho(s) da(s) tag(s): " + retorno;
            }

            return retorno;
        }
    }
}
# Gerenciador de Certificados

Sistema para criação de modelos de Certificados e distribuição por e-mail para uma lista importada de Participantes.

## Iniciando

As instruções abaixo irão lhe ajudar a configurar e a executar a aplicação em seu computador.


### Instalação

Clone o repositório em seu computador e abra a Solução correspondente utilizando o Visual Studio.

Foram utilizados os seguintes componentes externos que podem ser instalados com o Gerenciador de Pacotes do Nuget através dos seguintes comandos:

* [Bootstrap 4.1](https://getbootstrap.com/docs/4.1/getting-started/introduction/) - Versão mais recente do framework

```
PM> Install-Package bootstrap
```

* [HtmlRenderer.PdfSharp](https://www.nuget.org/packages/HtmlRenderer.PdfSharp/1.5.0.6) - Biblioteca de manipulação de PDF

```
PM> Install-Package HtmlRenderer.PdfSharp -Version 1.5.0.6
```

* [SendGrid](https://sendgrid.com/free/) - Biblioteca de envio de e-mails
```
PM> Install-Package SendGrid
```

## Autor

* **Welington Almeida** 

Matenha contato comigo e obtenha mais informações no meu [LinkedIn](https://www.linkedin.com/in/welington-almeida-571a7062/)

using Xunit;
using Zelo.Modules.Identity.Infrastructure.Emails;

namespace Zelo.Modules.Identity.Tests;

public class EmailTemplateLoaderTests
{
    [Fact]
    public void Load_TemplateExistente_DevolveConteudoNaoVazio()
    {
        var html = EmailTemplateLoader.Load("layout-button.html");

        Assert.False(string.IsNullOrWhiteSpace(html));
        Assert.Contains("{{Heading}}", html);
    }

    [Fact]
    public void Load_MesmoTemplateDuasVezes_DevolveOMesmoConteudo()
    {
        var first = EmailTemplateLoader.Load("layout-code.html");
        var second = EmailTemplateLoader.Load("layout-code.html");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Load_TemplateInexistente_LancaInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => EmailTemplateLoader.Load("nao-existe.html"));
    }
}

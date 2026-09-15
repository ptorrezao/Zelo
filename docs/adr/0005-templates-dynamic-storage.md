# ADR-005: Templates Dinâmicos — Storage e Motor de Render

**Estado:** aceite
**Data:** 2026-09-15

## Contexto

O repositório já contém templates HTML embutidos (EmbeddedResource) usados por `Zelo.Modules.Identity`.
As notificações de veículos exigem templates dinâmicos, i18n e possibilidade de evolução sem deploy.

## Decisão

Adotar abordagem gradual:

1. MVP: templates como `EmbeddedResource` por locale dentro do assembly de `Zelo.Modules.Core`, renderizados
   com `Scriban` para segurança e expressividade leve. Recomendamos adicionar o pacote NuGet `Scriban` ao projeto:

```xml
<PackageReference Include="Scriban" Version="5.7.0" />
```

E colocar ficheiros em `Zelo.Modules.Core/Infrastructure/Emails/Templates/vehicle-reminder.pt.html`.

Exemplo mínimo de render (C#):

```csharp
var template = EmailTemplateLoader.Load("vehicle-reminder.pt.html");
var scribanTpl = Scriban.Template.Parse(template);
var model = new {
  Plate = plate,
  Brand = brand,
  DueOn = dueOn.ToString("yyyy-MM-dd"),
  DaysUntilDue = daysUntil
};
var body = scribanTpl.Render(model);
```
2. V2: permitir armazenamento editável (Blob storage ou tabela DB) com versionamento e `TemplateVersion`.

## Consequências

- MVP rápido e consistente com padrões existentes (EmailTemplateLoader).
- Scriban permite lógica simples (if/for) sem segurança inerente de execução de código.
- V2 aumenta operação (UI para edição, permissões, audit trail).

## Alternativas

- Usar `RazorLight` para Razor templates — mais poder, maior superfície de risco.
- Somente EmbeddedResource com `string.Replace` — simples mas limitado para i18n e condições.

## Testes sugeridos

- Unit: renderizar template com objetos de exemplo e verificar substituições e escaping.
- Integration: enviar email para Mailhog em ambiente dev e validar o HTML gerado.

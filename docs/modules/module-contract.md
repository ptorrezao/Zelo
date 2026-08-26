# Contrato de módulo

Para um projeto ser um módulo Zelo tem de cumprir os quatro pontos abaixo.
Um módulo novo que os cumpra funciona sem alterar o núcleo.

## 1. Registo

Expõe dois métodos de extensão, e nada mais público:

```csharp
public static IServiceCollection AddXptoModule(this IServiceCollection s, IConfiguration c);
public static IServiceCollection AddXptoConsumers(this IServiceCollection s);
```

O host `Api` chama o primeiro. O host `Worker` chama ambos.
Tudo o resto no módulo é `internal`.

## 2. Manifesto

Declara os tipos de ativo que gere e o JSON Schema dos atributos de cada
um. O shell do frontend lê o manifesto para construir menus e formulários
— nenhum formulário é escrito à mão por tipo de ativo.

## 3. Eventos

Publica os eventos definidos em `Zelo.Contracts`, nunca tipos próprios:

- `AssetCreated` / `AssetArchived`
- `ObligationScheduled` / `ObligationUpdated` / `ObligationCompleted`

O módulo `Core` consome-os para manter a timeline agregada. Nunca chama
os módulos para a construir.

## 4. Persistência

`DbContext` próprio com `HasDefaultSchema("xpto")` e migrations dentro do
módulo. Sem `JOIN` para fora do seu schema, em circunstância alguma.

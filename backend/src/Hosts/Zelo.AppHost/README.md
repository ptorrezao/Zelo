# Zelo.AppHost

Orquestração local com .NET Aspire: Postgres, Api, Worker e o dashboard
de traces sobem com um comando.

O projeto tem de ser criado pelo template (precisa de propriedades de SDK
específicas que não vale a pena escrever à mão):

```bash
cd backend/src/Hosts
dotnet new aspire-apphost -n Zelo.AppHost -o Zelo.AppHost --force
```

Depois adicionar as referências à Api e ao Worker e registá-los no
`AppHost.cs`. Quando um módulo for extraído para serviço próprio, passa a
ser mais uma linha aqui.

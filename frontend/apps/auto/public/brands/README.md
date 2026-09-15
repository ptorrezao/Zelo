# Logótipos das marcas

Um PNG por marca, servido em `/brands/<slug>.png`. O nome sai do campo
`brand` do veículo pela mesma regra das fotografias: minúsculas, sem
acentos, e o resto convertido em hífen.

| Marca | Ficheiro |
|---|---|
| Yamaha | `yamaha.png` |
| Seat | `seat.png` |
| Smart | `smart.png` |
| Mercedes-Benz | `mercedes-benz.png` |
| Volkswagen | `volkswagen.png` |
| Volvo | `volvo.png` |
| Fiat | `fiat.png` |
| Toyota | `toyota.png` |
| Ford | `ford.png` |
| Audi | `audi.png` |
| BMW | `bmw.png` |
| Citroën | `citroen.png` |
| Hyundai | `hyundai.png` |
| Kia | `kia.png` |
| Opel | `opel.png` |
| Peugeot | `peugeot.png` |
| Renault | `renault.png` |

Faltam do catálogo (`vehicle-catalog.json`): Dacia, Honda, Nissan — o
`unavatar.io` (ver "Fonte" abaixo) devolveu logos errados/de outra marca
para Dacia e Nissan em todos os domínios tentados, Honda só deu versões
muito pequenas (≤48px, ficam desfocadas a 128px). Também bateu no limite
diário gratuito do serviço a meio da tentativa — ficam pendentes.

Enquanto o ficheiro faltar, o avatar mostra as iniciais da marca e do
modelo, e a consola regista um 404 por marca em falta.

## Tamanho

**128 × 128 px**, quadrado, PNG com fundo transparente.

O avatar da lista tem 36 px e o do cabeçalho 44 px, aos quais o componente
tira 15% de recuo. Os 128 px cobrem 3× no maior dos dois casos, que é o
suficiente para um ecrã de alta densidade — um logótipo não ganha nada em
ser maior do que isto.

Quadrado é indispensável: o avatar é redondo e uma imagem alongada fica
descentrada. O logótipo deve vir com margem própria mínima, porque o recuo
do avatar já lhe dá ar; margem a dobrar deixa a marca minúscula no círculo.

`ford.png` e `hyundai.png` são exceção — publicam este emblema como crachá
cheio (fundo até às bordas), sem versão solta. Dentro do avatar circular
fica recortado num círculo sólido, visualmente consistente com os outros
(ex. `yamaha.png`, que também é um disco opaco, só com os cantos fora do
círculo transparentes).

Poucos KB cada um. Se a marca tiver versão vetorial, exporta daí em vez de
ampliar um ficheiro pequeno.

## Fonte

Logos reais (nunca gerados por IA — risco de imprecisão e de marca
registada, ver plans/vehicle-image-generation.md). Todos vieram de
[unavatar.io](https://unavatar.io) por domínio da marca
(`unavatar.io/<domínio>?fallback=false`, às vezes um domínio regional -
`.pt`, `.de` - quando o principal só tinha uma versão pequena), com fundo
removido quando não vinha transparente (`ffmpeg colorkey`), ou convertido
de SVG com `sharp-cli` quando só havia vetorial. Sem fonte fixa/automática
- cada marca nova é avaliada visualmente antes de entrar aqui (ver lista
de "faltam" acima - a taxa de acerto por domínio varia bastante, e o
serviço tem limite diário sem conta).
